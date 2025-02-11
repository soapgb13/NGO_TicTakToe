public static class LongestCommonPrefixFinder
{
    public static string LongestCommonPrefix(string[] strs)
    {
        if (strs.Length == 0) return "";
        string prefix = strs[0];
        foreach (string str in strs)
        {
            int j = 0;
            while (j < prefix.Length && j < str.Length && prefix[j] == str[j])
                j++;
            prefix = prefix.Substring(0, j);
            if (prefix == "") break;
        }
        return prefix;
    }
}