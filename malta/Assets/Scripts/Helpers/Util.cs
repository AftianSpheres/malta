using UnityEngine;
using System.Collections.Generic;

public static class Util
{
    private readonly static char[] vowelsArray = { 'a', 'e', 'i', 'o', 'u', 'A', 'E', 'I', 'O', 'U' };
    private static List<string> _substrings;

    public static string[] GetLinesFrom (TextAsset a, int lnCount)
    {
        return a.text.Split(new string[] { "\r\n", "\n" }, lnCount, System.StringSplitOptions.None);
    }

    public static string[] GetLinesFrom (TextAsset a)
    {
        return a.text.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.None);
    }

    public static string[] GetLinesFrom (string s)
    {
        return s.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.None);
    }

    public static bool isVowel (char c)
    {
        bool r = false;
        for (int i = 0; i < vowelsArray.Length; i++)
        {
            if (c == vowelsArray[i])
            {
                r = true;
                break;
            }
        }
        return r;
    }

    public static string[] SplitIntoSubstringsOfLength (string s, int lenMax)
    {
        if (_substrings == null) _substrings = new List<string>();
        string[] words = s.Split(new string[] { "\r\n", "\n", " " }, System.StringSplitOptions.None);
        _substrings.Clear();
        string ln = string.Empty;
        for (int w = 0; w < words.Length; w++)
        {
            if (words[w].Length > lenMax) throw new System.Exception("SplitIntoSubstringsOfLength doesn't handle words longer than lenMax elegantly atm! To-do: make it do that");
            if (ln.Length + words[w].Length + 1 > lenMax)
            {
                _substrings.Add(ln + System.Environment.NewLine);
                ln = string.Empty;
            }
            if (ln == string.Empty) ln = words[w];
            else ln += " " + words[w];
        }
        _substrings.Add(ln);
        return _substrings.ToArray();
    }
}