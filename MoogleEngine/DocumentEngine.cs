namespace MoogleEngine;
using System.Text.Json;
public class Corpus
{
    public struct Document
    {
        public string path;

        public Vector<string> WordVector; 

        // upon creation of the Corpus insance, the tf-idf Vector of each document has no reason
        // to be changed, so in order to increase efficiency, we will hold the norm of the tf-idf
        // vector instead of computing it every time we want to find the cosine of it and a query
        public double VectorNorm;

        public bool HasWord(string word)
        {
            return WordVector.Dimension_Not_0(word);
        }
    }

    private Dictionary<string, double> Lexicon;
    private Document[] documents;
   
    #region Constructor_Caching

    public Corpus(string contentFolder, bool messages = false)
    {   
        int timer = Environment.TickCount;

        string lastModifiedDataPath = Path.Combine("..", "MoogleEngine", "lastModified.json"); //"../MoogleEngine/lastModified.json";
        string documentsJsonPath = Path.Combine("..", "MoogleEngine", "documents.json"); //"../MoogleEngine/documents.json";
        string lexiconJsonPath = Path.Combine("..", "MoogleEngine", "lexicon.json"); //"../MoogleEngine/lexicon.json";

        var cache = GetCache(contentFolder, lastModifiedDataPath, documentsJsonPath, lexiconJsonPath);
        if (cache.valid)
        {
            if(messages) System.Console.WriteLine("Valid Cache Found");
            documents = cache.docs;
            Lexicon = cache.lexicon;
        }
        else
        {
            if (messages) Console.WriteLine("Building Corpus");
            documents = GetDocuments(contentFolder);
            Lexicon = GetLexicon(documents);
            
            // originally, each Document only has the normalized tf of each of its words
            // we make it so that each document holds a vector with the tf-idf of each of its words
            for(int i = 0; i < documents.Length; i++)
            {
                foreach(var word in documents[i].WordVector.Dimensions)
                {
                    documents[i].WordVector[word] *= GetIdf(word);
                }
            }

            // filling out the Norm field in the documents, it is constant, so it's better
            // to compute it only once
            for (int i = 0; i < documents.Length; i++)
            {
                documents[i].VectorNorm = documents[i].WordVector.Norm();
            }

            CreateCache(contentFolder, lastModifiedDataPath, documentsJsonPath, lexiconJsonPath);
        }


        if (messages) System.Console.WriteLine($"Corpus ctor Finished: {(Environment.TickCount - timer)/1000.0}s");        
    }

    private void CreateCache(string contentFolder, string lastModifiedDataPath, string documentsJsonPath, string lexiconJsonPath)
    {
        string jsonText = JsonSerializer.Serialize<DateTime>(Directory.GetLastWriteTime(contentFolder));
        File.WriteAllText(lastModifiedDataPath, jsonText);

        var options = new JsonSerializerOptions{IncludeFields = true};
        jsonText = JsonSerializer.Serialize<Document[]>(this.documents, options);
        File.WriteAllText(documentsJsonPath, jsonText);

        jsonText = JsonSerializer.Serialize<Dictionary<string, double>>(this.Lexicon);
        File.WriteAllText(lexiconJsonPath, jsonText);
    }

    private (bool valid, Document[] docs, Dictionary<string, double> lexicon) GetCache(string contentFolder, string lastModifiedDataPath, string documentsJsonPath, string lexiconJsonPath)
    {   
        if(File.Exists(lastModifiedDataPath) && File.Exists(documentsJsonPath) && File.Exists(lexiconJsonPath))
        {
            DateTime contentLastModifiedTime;
            using (var lastModifiedJson = new StreamReader(lastModifiedDataPath))
            {
                string text = lastModifiedJson.ReadToEnd();
                contentLastModifiedTime = JsonSerializer.Deserialize<DateTime>(text);
            }
            if (contentLastModifiedTime == Directory.GetLastWriteTime(contentFolder))
            {
                Document[] newDocs;
                Dictionary<string, double> newLexicon;
                
                using(var docJsonReader = new StreamReader(documentsJsonPath))
                {
                    var options = new JsonSerializerOptions{IncludeFields = true};
                    var json = docJsonReader.ReadToEnd();
                    newDocs = JsonSerializer.Deserialize<Document[]>(json, options);
                }

                using(var lexiconJsonReader = new StreamReader(lexiconJsonPath))
                {
                    var json = lexiconJsonReader.ReadToEnd();
                    newLexicon = JsonSerializer.Deserialize<Dictionary<string, double>>(json);
                }
                
                return (true, newDocs, newLexicon);
            }
        }

        return (false, null, null);
    }


    // given that we need to bake the idf into the vectors of the documents, we cannot fill the
    // VectorNorm field in this method, because the vectors in each document are not completed yet
    // and therefore the Lexicon isn't either
    private static Document[] GetDocuments(string contentFolder)
    {
        // counting the files to get the length of the Document array
        int fileCounter = Directory.EnumerateFiles(contentFolder, "*.txt").Count();

        var newDocArray = new Document[fileCounter];

        fileCounter = 0;

        // here we extract the data from the files
        foreach (string file in Directory.EnumerateFiles(contentFolder, "*.txt"))
        {
            var docDict = GetWordsTF(File.ReadAllText(file));
            
            newDocArray[fileCounter].WordVector = new Vector<string>(docDict, true);
            newDocArray[fileCounter].path = file;
            
            fileCounter++;
        }
        
        return newDocArray;
    }

    // extracts the Lexicon of an array of Documents
    private static Dictionary<string, double> GetLexicon(Document[] documents)
    {
        var newCorpus = new Dictionary<string, double>();
        foreach(var doc in documents)
        { 
            // we iterate through the words of the doc, if the word isn't in the
            // new Corpus, we add it, saying it appeared in one document
            // if it is in the corpus, we simply note it has appeared in one more
            // document 
            foreach(var word in doc.WordVector.Dimensions)
            {
                if (newCorpus.Keys.Contains(word))
                {
                    newCorpus[word]++;
                }
                else
                {
                    newCorpus[word] = 1;
                }

            }
        }

        // newCorpus[word] now indicates in how many docs word appears

        // now that we know in how many docs each word appears, and all the words
        // that are in each doc, and the number of docs, we must calculate idf.

        // turning document frequency into IDF
        foreach(var word in newCorpus.Keys)
        {
            // the less times the word appears over our dataset, the rarer
            // it is, the more meaning it probably carries, the higher its IDF
            newCorpus[word] = Math.Log((double)documents.Length/newCorpus[word]);
        }

        return newCorpus;
    }

    // returns normalized tf vector of the content of the document
    private static Dictionary<string, double> GetWordsTF(string docText) 
    {
        // tf vector of the document
        var answ = new Dictionary<string, double>();

        string[] DocWords = docText.Split(Utils.splitChars, StringSplitOptions.RemoveEmptyEntries & StringSplitOptions.TrimEntries);
        foreach(var word in DocWords)
        {
            string lowerWord = word.ToLower();
            if (answ.Keys.Contains(lowerWord))
            {
                answ[lowerWord]++;
            }
            else
            {
                answ[lowerWord] = 1;
            }
        }

        foreach(var word in answ.Keys)
        {
            answ[word] /= (double)DocWords.Length;
        }

        return answ;
    }

    #endregion

    #region Word Data Retrieval

    public bool LexiconContains(string word)
    {
        return this.Lexicon.Keys.Contains(word);
    }

    // returns true if word appears in 95% of documents or more
    public bool IsStopWord(string word)
    {
        return Lexicon[word] <= Math.Log(100/95.0);
    }

    public double GetIdf(string word)
    {
        if (!LexiconContains(word)) throw new ArgumentException();

        return Lexicon[word];
    }


    // will iterate through all the words in the Lexicon and return the Levenshtein-closest
    public string GetClosestWord(string word)
    {
        int minDist = int.MaxValue;
        string answ = "";
        foreach(var corpusWord in this.Lexicon.Keys)
        {
            int currDist = editDistance(word, corpusWord);
            if (currDist == 1) return corpusWord;
            if (currDist < minDist)
            {
                minDist = currDist;
                answ = corpusWord;
            }
        }

        return answ;
    }

    // bottom-up iterative Levenshtein distance implementation
    private static int editDistance(string a, string b)
    {
        int[,] subproblems = new int[b.Length + 1, a.Length + 1];
        for (int i = 0; i < subproblems.GetLength(0); i++)
        {
            subproblems[i, 0] = i;
        }
        for (int i = 0; i < subproblems.GetLength(1); i++)
        {
            subproblems[0, i] = i;
        }

        for (int i = 1; i <= b.Length; i++)
        {
            for (int j = 1; j <= a.Length; j++)
            {

                if (a[j - 1] == b[i - 1]) subproblems[i, j] = subproblems[i - 1, j - 1];
                else
                {

                   int min = subproblems[i - 1, j];
                   min = Math.Min(min, subproblems[i - 1, j - 1]);
                   min = Math.Min(min, subproblems[i, j - 1]);
                   subproblems[i, j] = min + 1; 
                }
            }
        }

        return subproblems[b.Length, a.Length];
    }



    #endregion
    
    #region Query Handling

    // returns the <amount> documents who's tf-idf vector is more similar to
    // that of the query 
    public SearchItem[] GetClosestDocuments(Query query, int amount = 10, bool ExtendedSearch = false)
    {
        var list = new List<(Document doc, double score)>(amount + 1);
        foreach(var doc in documents)
        {   
            if (!ValidDocument(query, doc)) continue;

            var score = query.queryVector.DotProduct(doc.WordVector);
            score /= (query.VectorNorm*doc.VectorNorm);

            var submission = (doc, score);
            // this will insert the submission in the document list, conserving its order by score
            if (submission.score > 0) InsertInDocumentList(list, submission, amount);
        }
        
        var answ = new SearchItem[list.Count];
        for (int i = 0; i < answ.Length; i++)
        {
            double multiplier = 1;
            if (ExtendedSearch) multiplier = 0.75;
            
            answ[i] = new SearchItem
            (
                GetName(list[i].doc.path),
                SnippetEngine.GetSnippet(query, list[i].doc.path, this),
                (float)(list[i].score * multiplier)
            );
        }

        return answ;
    }

    private static bool ValidDocument(Query query, Document doc)
    {
        foreach(var necessaryWord in query.Include)
        {
            if (!doc.HasWord(necessaryWord))
            {
                return false;
            }
        }

        foreach(var absencence_necessaryWord in query.Exclude)
        {
            if (doc.HasWord(absencence_necessaryWord))
            {
                return false;
            }
        }
        

        return true;
    }

    private static void InsertInDocumentList(List<(Document doc, double score)> items, (Document doc, double score) submission, int targetAmount)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if(submission.score > items[i].score)
            {
                items.Insert(i, submission);
                // if inserting the submission makes the list exceed the targe amount, we remove the least
                // similar document from the list
                if (items.Count > targetAmount) items.RemoveAt(items.Count - 1);
                return;
            }
        }
        // if the method reaches this line, then the only reason to add a document
        // would be that there's enough space for it at the end of the list
        if (items.Count < targetAmount)
        {
            items.Add(submission);
        }
    }
    
    private static string GetName(string path)
    {
        return Path.GetFileNameWithoutExtension(path);
    }


    #endregion

}
