namespace MoogleEngine;

public static class Utils
{
    public static char[] splitChars = GetSplitChars();

    private static char[] GetSplitChars()
    {
        int ctr = 0;
        for (int i = char.MinValue; i <= char.MaxValue; i++)
        {
            if ( !char.IsLetterOrDigit((char)i) )
            {
                ctr++;
            }
        }
        var newSplitChars = new char[ctr];
        int char_it = 0;
        for (int i = char.MinValue; i <= char.MaxValue; i++)
        {
            if ( !char.IsLetterOrDigit((char)i) )
            {
                newSplitChars[char_it++] = (char)i;
            }
        }

        return newSplitChars;
        
    }

    public static string GetNextWord(int index, string query)
    {
        string answ = "";
        answ += query[index];
        for (int i = index + 1; i < query.Length; i++)
        {
            if (!char.IsLetterOrDigit(query[i])) break;
            else answ += query[i];
        }

        return answ;
    }

    public static bool DoubleEquality(double a, double b) => Math.Abs(a - b) < 1E-15;
    
}

