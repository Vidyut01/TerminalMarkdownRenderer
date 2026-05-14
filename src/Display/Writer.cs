namespace MdRenderer;

class Writer
{
    public static void OpenAlternateBuffer()
    {
        Console.Write("\x1b[?1049h");
        Console.Clear();
    }

    public static void CloseAlternateBuffer()
    {
        Console.Write("\x1b[?1049l");
    }

    public static void WriteStatusBar(int offset, int total, int viewportHeight, string filePath)
    {
        const int RightWrapperMaxWidth = 6;

        int width = Console.WindowWidth;
        int percent = total <= viewportHeight ? 100
            : (int)(100.0 * offset / Math.Max(1, total - viewportHeight));

        string left = " [q] Quit  [↑/↓] Scroll  [PgUp/PgDn] Page  [Home/End] Jump ";
        
        if (left.Length + filePath.Length + RightWrapperMaxWidth > width) filePath = Path.GetFileName(filePath);
        string right = percent switch {100 => $" END {filePath} ", _ => $" {percent}% {filePath} "};
         

        string status;

        if (left.Length + right.Length <= width)
        {
            int padding = width - left.Length - right.Length;
            status = left + new string(' ', padding) + right;
        }
        else if (right.Length <= width)
        {
            status = new string(' ', width - right.Length) + right;
        }
        else
        {
            status = right[..width];
        }

        Console.BackgroundColor = ConsoleColor.Gray;
        Console.ForegroundColor = ConsoleColor.Black;
        Console.Write(status);
        Console.ResetColor();
    }
}