using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Spectre.Console;

using MdRenderer.Utilities;

namespace MdRenderer;

class Renderer
{
    private const int MaxWidth = 80;
    private const int LeftMargin = 2;
    private readonly string _margin = new(' ', LeftMargin);

    public void Render(MarkdownDocument doc)
    {
        foreach (var block in doc)
            RenderBlock(block);
    }

    private void RenderBlock(Block block)
    {
        switch (block)
        {
            case HeadingBlock h:
                RenderHeading(h);
                break;
            case ParagraphBlock p:
                RenderParagraph(p);
                break;
            case FencedCodeBlock code:
                RenderCodeBlock(code);
                break;
            case ListBlock list:
                RenderList(list);
                break;
            case QuoteBlock quote:
                RenderQuote(quote);
                break;
            case ThematicBreakBlock:
                RenderRule();
                break;
        }
    }

    private void RenderHeading(HeadingBlock heading)
    {
        string text = RenderInlines(heading.Inline);
        int length = heading.Inline!.Span.Length;

        var (color, underline) = heading.Level switch
        {
            1 => ("bold magenta", true),
            2 => ("bold blue",    true),
            3 => ("bold cyan",    true),
            4 => ("bold green",   true),
            5 => ("bold yellow",  false),
            _ => ("bold white",   false)
        };

        if (underline)
            AnsiConsole.MarkupLine($"{_margin}[underline {color}]{text}[/]");
        else
            AnsiConsole.MarkupLine($"{_margin}[{color}]{text}[/]");


        AnsiConsole.WriteLine();
    }

    private void RenderParagraph(ParagraphBlock paragraph)
    {
        string text = RenderInlines(paragraph.Inline);
        foreach (var line in Wrap(text, MaxWidth))
            AnsiConsole.MarkupLine($"{_margin}{line}");
        AnsiConsole.WriteLine();
    }

    private void RenderCodeBlock(FencedCodeBlock code)
    {
        var lines = code.Lines.Lines
            .Select(l => l.ToString())
            .SkipLast(1)
            .ToList();
        
        // Remove trailing empty lines
        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[^1]))
            lines.RemoveAt(lines.Count - 1);
        
        string content = string.Join('\n', lines);

        var panel = new Panel(StringUtilities.Escape(content))
        {
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("grey dim"),
            Padding = new Padding(1, 0)
        };

        AnsiConsole.WriteLine();
        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    private void RenderList(ListBlock list)
    {
        int index = 1;
        foreach (var item in list.Cast<ListItemBlock>())
        {
            string bullet = list.IsOrdered ? $"{index++}." : "•";
            string color = list.IsOrdered ? "blue" : "yellow";

            // grab the first paragraph inside the list item
            string text = item.FirstOrDefault() is ParagraphBlock p
                ? RenderInlines(p.Inline)
                : "";

            AnsiConsole.MarkupLine($"{_margin}  [{color}]{bullet}[/] {text}");

            // handle nested blocks inside the list item
            foreach (var child in item.Skip(1))
                RenderBlock(child);
        }
        AnsiConsole.WriteLine();
    }

    private void RenderQuote(QuoteBlock quote)
    {
        AnsiConsole.WriteLine();
        foreach (var child in quote)
        {
            if (child is ParagraphBlock p)
            {
                string text = RenderInlines(p.Inline);
                foreach (var line in Wrap(text, MaxWidth - 4))
                    AnsiConsole.MarkupLine($"{_margin}[magenta dim]│[/] [italic grey]{line}[/]");
            }
        }
        AnsiConsole.WriteLine();
    }

    private void RenderRule()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule() { Style = Style.Parse("grey dim") });
        AnsiConsole.WriteLine();
    }

    private static string RenderInlines(ContainerInline? inlines)
    {
        if (inlines == null) return "";

        var sb = new System.Text.StringBuilder();

        foreach (var inline in inlines)
        {
            switch (inline)
            {
                case LiteralInline literal:
                    sb.Append(StringUtilities.Escape(literal.Content.ToString()));
                    break;

                case EmphasisInline emphasis:
                    string inner = RenderInlines(emphasis);
                    string tag = emphasis.DelimiterCount == 2 ? "bold" : "italic";
                    sb.Append($"[{tag}]{inner}[/]");
                    break;

                case CodeInline code:
                    sb.Append($"[bold yellow on grey19]{StringUtilities.Escape(code.Content)}[/]");
                    break;

                case LinkInline link:
                    string label = RenderInlines(link);
                    sb.Append($"[blue underline]{label}[/]");
                    sb.Append($" [grey]({StringUtilities.Escape(link.Url ?? "")})[/]");
                    break;

                case LineBreakInline:
                    sb.Append('\n');
                    break;
            }
        }

        return sb.ToString();
    }

    private static IEnumerable<string> Wrap(string text, int width)
    {
        if (text.Length <= width)
        {
            yield return text;
            yield break;
        }

        var words = text.Split(' ');
        var line = new System.Text.StringBuilder();

        foreach (var word in words)
        {
            if (line.Length + word.Length + 1 > width && line.Length > 0)
            {
                yield return line.ToString().TrimEnd();
                line.Clear();
            }
            line.Append(word + " ");
        }

        if (line.Length > 0)
            yield return line.ToString().TrimEnd();
    }
}