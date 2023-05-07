namespace MoogleEngine;
using System.Text.Json;

// ^ makes word necessary
// * multiplies word tf by 2
// ! makes absence of word necessary
public struct Query
{
    public bool synonymsFound = false;
    // it is important to call the method AddToQuery, because that is the one which will
    // update this variable each time a word is added to the query vector
    private int wordCtr = 0;
    public string suggestion {get; private set;} = "";
    public bool errorFound  {get; private set;} = false;
    public List<string> Include = new List<string>();
    public List<string> Exclude = new List<string>();
    public Vector<string> queryVector = new Vector<string>();
    // after a query is instanced, the vector that represents it has no reason to be changed
    // so we will precompute its norm for faster cosine computation
    public double VectorNorm {get; private set;}

    public Query(string query, Corpus content, bool AddSynonyms = false)
    {
        VectorNorm = 0; // in order to use some methods we initialize this, it will be updated at the end of the constructor
        
        int asteriskCounter = 0;
        var OPERATORS = new char[]{'*', '!', '^'};

        var addSynonymsOf = new List<(string word, double original_tf)>();

        for (int i = 0; i < query.Length; i++)
        {

            if (char.IsLetterOrDigit(query[i]))
            {
                string nextWord = Utils.GetNextWord(i, query).ToLower();

                var correction = SpellCheck(nextWord, content);

                // if the word has a valid operator modifying it, it must be behind it
                char Operator;
                if (i > 0) Operator = query[i - 1];
                else Operator = default;

                i = i + nextWord.Length - 1;

                if (correction.errorFound)
                {
                    if (Operator == '^') Include.Add(nextWord); // no valid documents will be found
                    if (Operator == '!') continue; // no reason to add synonyms of or correct a word the user does not want looked for and isn't in the corpus
                    
                    // word simply does not appear in the Lexicon, but does not have spelling errors
                    if (SynonymDictionary.Contains(nextWord))
                    {   
                        synonymsFound = true;
                        suggestion += nextWord + ' ';
                        // here if the operator modifying the word is not * (or none) asteriskCounter = 0, 2^asteriskCounter = 1
                        // if the operator is *, then it makes sense we give more importance to the synonyms of that word
                        
                        // we do not indicate the word to be added with a lower tf, because it will be multiplied by half
                        // when added as a synomym
                        if (AddSynonyms) AddTo_AddSynomymsOfList(addSynonymsOf, nextWord, Math.Pow(2, asteriskCounter), content);
                    }
                    else
                    {
                        suggestion += correction.correctWord + ' ';
                        this.errorFound = true;
                        AddToQueryWithOPERATOR(correction.correctWord, content, Operator, asteriskCounter, 0.5);
                    }
                }
                else
                {
                    suggestion += nextWord + ' ';
                    AddToQueryWithOPERATOR(nextWord, content, Operator, asteriskCounter);
                }

                asteriskCounter = 0;
                
            } 
            else if (query[i] == '*') asteriskCounter++;
            else asteriskCounter = 0;  
        }

        if (!errorFound) suggestion = "";
        else suggestion += '\b'; // delete last space


        // if the query ends with a word, it's added with getNextWord, if not,
        // whatever symbols it ends with are not important for our purposes
        
        foreach(var word in queryVector.Dimensions)
        {
            // no need to iterate through the query terms if this is true
            if (!AddSynonyms && synonymsFound) break; 
            
            if (Exclude.Contains(word)) continue;
            if (SynonymDictionary.Contains(word)) 
            {
                synonymsFound = true;
                if (AddSynonyms) AddTo_AddSynomymsOfList(addSynonymsOf, word, queryVector[word], content);
            }
        }

        if (AddSynonyms) AddSynonymsToQuery(content, addSynonymsOf);
        AddIdf(content);

        // normalizing tf of the query
        foreach(var word in queryVector.Dimensions)
        {
            queryVector[word]/=wordCtr;
        }
        this.VectorNorm = queryVector.Norm();
    }

    private void AddTo_AddSynomymsOfList(List<(string word, double original_tf)> synList, string word, double newTf, Corpus content)
    {
        if (!SynonymDictionary.Contains(word)) return; // just in case
        for(int i = 0; i < synList.Count; i++)
        {
            if (word == synList[i].word)
            {
                synList[i] = (word, synList[i].original_tf + newTf);
                return;
            }
        }

        synList.Add((word, newTf));
    }

    private void AddIdf(Corpus content)
    {
        foreach(var word in queryVector.Dimensions)
        {
            queryVector[word] *= content.GetIdf(word);
        }
    }

    private void AddSynonymsToQuery(Corpus content, List<(string word, double original_tf)> addSynonymsOf)
    {
        foreach(var word_tf in addSynonymsOf)
        { 
            foreach(var synonymList in SynonymDictionary.GetSynonymLists(word_tf.word))
            {
                foreach(var synonym in synonymList)
                {
                    AddtoQuery(synonym, content, word_tf.original_tf*0.5);
                }
            }
        }
    }

    private void AddToQueryWithOPERATOR(string word, Corpus content, char symbol = default, int ctr = 0, double multiplier = 1)
    {
        if (symbol == '*')
        {
            AddtoQuery(word, content, (int)Math.Pow(2, ctr) * multiplier);
        }
        else if (symbol == '^')
        {
            Include.Add(word);
            // it doesn't matter if the word is in the corpus, if the user is asking for a word
            // that isn't there, no results will be returned, which makes sense
            AddtoQuery(word, content, multiplier);
        }
        else if (symbol == '!')
        {
            Exclude.Add(word);
            // word does not need to be in the vector, for it will never add anything
            // to the similarity calculation, because it will never be executed if the word is
            // in the document we're comparing the query with
        }
        else
        {   
            AddtoQuery(word, content, multiplier);
        }
    }

   
    private void AddtoQuery(string word, Corpus content, double importance = 1)
    {
        wordCtr++;
        if (content.LexiconContains(word) && !content.IsStopWord(word))
        {
            queryVector[word] += importance;
        }
    }

    // returns a corrected word based in the Content and whether or not an error was found in the
    // submitted word
    private (string correctWord, bool errorFound) SpellCheck(string word, Corpus content)
    {
        if (content.LexiconContains(word)) return (word, false);
        return (content.GetClosestWord(word), true);
    }
}

public static class SynonymDictionary
{
    private static Dictionary<string, List<string[]>> dict;
    static SynonymDictionary()
    {
        string jsonText = null;
        dict = new Dictionary<string, List<string[]>>();
        try
        {
            var json = new StreamReader(Path.Combine("..", "MoogleEngine", "sinonimos.json"));
            jsonText = json.ReadToEnd();
            json.Close();
        }
        catch(FileNotFoundException)
        {
            System.Console.WriteLine("Synonym File not Found");
            return;
        }

        if (jsonText == null)
        {
            return;
        }
        
        var synonymLists = JsonSerializer.Deserialize<List<string[]>>(jsonText);
        dict = new Dictionary<string, List<string[]>>();

        foreach(var synonymList in synonymLists)
        {
            foreach(var word in synonymList)
            {
                // some synonyms in this dictionary are comprised of more than one word
                // we ignore those
                if (word.Split(Utils.splitChars).Length > 1) continue;
                var lowerWord = word.ToLower();
                // we must remember each word will have a list of lists of synonyms, not merely a list of synonyms
                if (dict.ContainsKey(lowerWord)) dict[word.ToLower()].Add(synonymList);
                else 
                {
                    dict[lowerWord] = new List<string[]>();
                    dict[lowerWord].Add(synonymList);
                }
            }
        }
    }
    // this synonym dictionary holds words which may appear in several lists
    // therefore, each word will have a list of lists of synonyms that can be iterated through
    // upon calling this method
    public static IEnumerable<string[]> GetSynonymLists(string word)
    {
        if (dict.ContainsKey(word.ToLower()))
        {
            return dict[word.ToLower()];
        }
        return new string[][]{};
    }
    public static bool Contains(string word)
    {
        return dict.ContainsKey(word);
    }
}
