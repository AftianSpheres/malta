using UnityEngine;

public static class Util
{
    private readonly static char[] vowelsArray = { 'a', 'e', 'i', 'o', 'u', 'A', 'E', 'I', 'O', 'U' };

    public static string[] GetLinesFrom (TextAsset a, int lnCount)
    {
        return a.text.Split(new string[] { "\r\n", "\n" }, lnCount, System.StringSplitOptions.None);
    }

    public static string[] GetLinesFrom (TextAsset a)
    {
        return a.text.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.None);
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
}