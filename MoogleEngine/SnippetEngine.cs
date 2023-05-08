namespace MoogleEngine;

public static class SnippetEngine
{
    public static string GetSnippet(Query query, string filePath, Corpus content, int Length = 60)
    {
        if (Length <= 0) return "";
        var currSnippet = new SnippetBuilder(Length);
        string text;

        using (var reader = new StreamReader(filePath))
        {
            text = reader.ReadToEnd();
        }

        double maxScore = double.MinValue;
        int start = 0;
        int end = 0;


        bool validSnippetFound = false;
        // we will scan the text to get the section (substring) of it which is most similar to the query
        for (int i = 0; i < text.Length; i++)
        {
            // short circuit for long documents
            if (validSnippetFound && i >= 5_000) break;
            
            // detect the beginning of a new word
            if (char.IsLetterOrDigit(text[i]))
            {
                string nextWord = Utils.GetNextWord(i, text).ToLower();
                currSnippet.Add(nextWord, i);
                i = i + nextWord.Length - 1;

                // we check if the snippet has all words that need to be included
                // and if it has at least one centric query term
                if (!currSnippet.ValidSnippet(query)) continue;
                
                var snippetVector = currSnippet.GetVector(content);
                var newScore = query.queryVector.DotProduct(snippetVector);
                newScore /= (query.VectorNorm*snippetVector.Norm());

                if (newScore > 0 && newScore > maxScore && currSnippet.AtCapacity())
                {
                    validSnippetFound = true;
                    maxScore = newScore;
                    start = currSnippet.GetStart();
                    end = currSnippet.GetEnd();
                }
            }
        }


        // this only happens if the doc was too small for the currSnippet to be at capacity at any time
        // or terms of the query appear at the very end of the document, in either case, simply printing
        // the current snippet is what we want
        if (maxScore == double.MinValue)
        {
            start = currSnippet.GetStart();
            end = currSnippet.GetEnd();
        }

        return text.Substring(start, (end - start + 1));   
    }
}

public class SnippetBuilder
{
    public int Capacity {get; private set;}
    public int Count { get {return wordList.Count;} }
    private LinkedList<(int start, string word)> wordList;
    private Vector<string> Vector;

    public SnippetBuilder(int Capacity = 30)
    {
        wordList = new LinkedList<(int start, string word)>();
        this.Capacity = Capacity;
        Vector = new Vector<string>();
    }

    public bool AtCapacity()
    {
        return Count == Capacity;
    }

    // this will append a word to the linked list (an abstract snippet), removing the first one
    // if doing so exceedes the maximum length (capacity) specified in the instantiation of this
    public void Add(string word, int start)
    {
        wordList.AddLast((start, word));
        Vector[word]++;
        if (Count > Capacity)
        {
            var firstWord = wordList.First.Value.word;
            Vector[firstWord]--;
            wordList.RemoveFirst();
        }
    }

    // this will build a vector for tf-idf comparison with the words held in the linked list
    // that represents the snippet
    public Vector<string> GetVector(Corpus content)
    {
        var answ = new Vector<string>(this.Vector);
        foreach(var word in answ.Dimensions)
        {
            answ[word] /= (double)this.Count;
            answ[word] *= content.GetIdf(word);
        }

        return answ;   
    }

    public bool ValidSnippet(Query query)
    {
        foreach(var necessaryWord in query.Include)
        {
            if (this.Vector.Dimension_Is_0(necessaryWord)) return false;
        }
        foreach(var absencence_necessaryWord in query.Exclude)
        {
            if (this.Vector.Dimension_Not_0(absencence_necessaryWord)) return false;
        }
        
        // if there are no query terms in the center-ish of the snippet, then it is
        // best to wait until there are
        int pos = 0;
        if (Count/4 >= (3*Count)/4) return true;
        foreach(var word_pos in wordList)
        {
            pos++;
            if (query.queryVector.Dimension_Not_0(word_pos.word) && pos > Count/4 && pos < (3*Count)/4) return true;
        }

        return false;
    }

    public int GetStart()
    {
        return wordList.First.Value.start;
    }

    public int GetEnd()
    {
        return wordList.Last.Value.start + wordList.Last.Value.word.Length;
    }
}
