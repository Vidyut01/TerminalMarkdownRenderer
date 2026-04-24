using Spectre.Console;

namespace MdRenderer;

class Renderer
{
    public static void Render(List<Block> blocks)
    {
        foreach (var block in blocks)
        {
            RenderBlock(block);
        }
    }

    private static void RenderBlock(Block block)
    {
        switch (block.Type) {
            case BlockType.Heading:
                RenderHeading(block);
                break;
            case BlockType.Paragraph:
                AnsiConsole.WriteLine(block.Content);
                break;
            case BlockType.CodeBlock:
                RenderCodeBlock(block);
                break;
            case BlockType.ListItem:
                AnsiConsole.MarkupLine($"  [yellow]•[/] {block.Content}");
                break;
            case BlockType.Blockquote:
                AnsiConsole.MarkupLine($"[grey]│[/] [italic]{block.Content}[/]");
                break;
            case BlockType.Blank:
                AnsiConsole.WriteLine();
                break;
        }
    }

    private static void RenderHeading(Block block)
    {
        var (color, prefix) = block.Level switch
        {
            1 => ("bold magenta", ""),
            2 => ("bold blue", "  "),
            3 => ("bold cyan", "    "),
            _ => ("bold white", "      ")
        };

        AnsiConsole.MarkupLine($"{prefix}[{color}]{block.Content}[/]");
        
        if (block.Level == 1) 
        {
            AnsiConsole.MarkupLine($"[magenta]{new string('─', block.Content.Length)}[/]");
        }
    }

    private static void RenderCodeBlock(Block block) {
        var panel = new Panel(block.Content) {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Grey),
            Padding = new Padding(1, 0)
        };
        AnsiConsole.Write(panel);
    }
}