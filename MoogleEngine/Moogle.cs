namespace MoogleEngine;
public static class Moogle
{
    static bool messages = true;
    static Corpus content = new Corpus(Path.Combine("..", "Content"), messages);
    public static SearchResult Query(string query) 
    {
        if (messages) System.Console.WriteLine("Query Sent");
        int timer = Environment.TickCount;

        Query query1 = new Query(query, content);

        var results = content.GetClosestDocuments(query1);
        if (results.Length < 10 && query1.synonymsFound)
        {
            if (messages) System.Console.WriteLine("Synonym query computed");
            
            Query SynonymQuery = new Query(query, content, true);
            
            var synResult = content.GetClosestDocuments(SynonymQuery, ExtendedSearch: true);

            results = Merge(results, synResult);
        }

        var sr = new SearchResult(results, query1.suggestion);

        if (messages) System.Console.WriteLine($"Query Time: {(Environment.TickCount - timer)/1000.0}s");

        return sr;
    }

    private static SearchItem[] Merge(SearchItem[] priority1, SearchItem[] priority2, int targetSize = 10)
    {
        int totalCount = priority1.Length + priority2.Length;
        bool[] repeated = new bool[priority2.Length];

        for(int i = 0; i < priority1.Length; i++)
        {
            for(int j = 0; j < priority2.Length; j++)
            {
                if (!repeated[j] && priority1[i].Title == priority2[j].Title)
                {
                    totalCount--;
                    if (priority2[j].Score > priority1[i].Score)
                    {
                        priority1[i] = priority2[i];
                    }
                    repeated[j] = true;
                }
            }
        }

        
        int it_2 = 0;
        while(it_2 < priority2.Length && repeated[it_2]) it_2++;
        if (it_2 == priority2.Length) return (SearchItem[])priority1.Clone();


        int it_1 = 0;
        var answ = new SearchItem[Math.Min(totalCount, targetSize)];
        int it_a = 0;

        while(it_a < answ.Length && it_1 < priority1.Length && it_2 < priority2.Length)
        {
            if (priority1[it_1].Score >= priority2[it_2].Score)
            {
                answ[it_a++] = priority1[it_1++];
            }
            else
            {
                answ[it_a++] = priority2[it_2++];
            }

            while(it_2 < priority2.Length && repeated[it_2]) it_2++;
        }

        while(it_a < answ.Length && it_1 < priority1.Length)
        {
            answ[it_a++] = priority1[it_1++];
        }
        while(it_a < answ.Length && it_2 < priority2.Length)
        {
            answ[it_a++] = priority2[it_2++];
            while(it_2 < priority2.Length && repeated[it_2]) it_2++;
        }

        return answ;
    }
}