using System.Collections.Generic;

public static class ParenthesesValidator
{
    public static bool IsValid(string s)
    {
        var stack = new Stack<char>();
        var pairs = new Dictionary<char, char> { { ')', '(' }, { '}', '{' }, { ']', '[' } };
        foreach (char c in s)
        {
            if (pairs.ContainsValue(c))
                stack.Push(c);
            else if (pairs.ContainsKey(c))
            {
                if (stack.Count == 0 || stack.Pop() != pairs[c]) return false;
            }
        }
        return stack.Count == 0;
    }
}