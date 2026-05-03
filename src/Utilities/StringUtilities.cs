namespace MdRenderer.Utilities;

public static class StringUtilities
{
    /// <summary>
    /// Escapes square brackets for Spectre.Console markup parsing.
    /// </summary>
    public static string Escape(string text) =>
        text.Replace("[", "[[").Replace("]", "]]");
}
