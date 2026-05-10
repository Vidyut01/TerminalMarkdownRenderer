using System.Text;

namespace MdRenderer.Utilities;

public static class StringUtilities
{
    /// <summary>
    /// Escapes square brackets for Spectre.Console markup parsing.
    /// </summary>
    public static string Escape(string text) =>
        text.Replace("[", "[[").Replace("]", "]]");
    
    public static IEnumerable<string> Wrap(string text, int width)
    {
        if (VisualLength(text) <= width)
        {
            yield return text;
            yield break;
        }

        var words = text.Split(' ');
        var line = new StringBuilder();
        int lineLen = 0;

        foreach (var word in words)
        {
            int wordLen = VisualLength(word);
            if (lineLen + wordLen + 1 > width && lineLen > 0)
            {
                yield return line.ToString().TrimEnd();
                line.Clear();
                lineLen = 0;
            }
            line.Append(word + " ");
            lineLen += wordLen + 1;
        }

        if (line.Length > 0)
            yield return line.ToString().TrimEnd();
    }

    private static int VisualLength(string text)
    {
        int len = 0;
        bool inTag = false;
        foreach (char c in text)
        {
            if (c == '[') inTag = true;
            else if (c == ']') inTag = false;
            else if (!inTag) len++;
        }
        return len;
    }
}
