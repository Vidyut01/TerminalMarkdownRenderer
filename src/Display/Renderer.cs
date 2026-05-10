using System.Text;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using MdTable = Markdig.Extensions.Tables.Table;
using MdTableRow = Markdig.Extensions.Tables.TableRow;
using MdTableCell = Markdig.Extensions.Tables.TableCell;
using Spectre.Console;
using SpectreTable = Spectre.Console.Table;
using MdRenderer.Utilities;

namespace MdRenderer;

class Renderer
{
    private const int LeftMargin = 2;
    private readonly string _margin = new(' ', LeftMargin);

    private IAnsiConsole _console = null!;
    private StringWriter _writer = null!;
    private string _source = null!;

    public List<string> Render(MarkdownDocument doc, string source)
    {
        _source = source;
        _writer = new StringWriter();
        _console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.Yes,
            ColorSystem = ColorSystemSupport.TrueColor,
            Out = new AnsiConsoleOutput(_writer),
        });

        foreach (var block in doc)
            RenderBlock(block);

        return _writer.ToString().Split('\n').ToList();
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
            case FencedCodeBlock c:
                RenderCodeBlock(c);
                break;
            case CodeBlock c:
                RenderCodeBlock(c);
                break;
            case ListBlock l:
                RenderList(l, 0);
                break;
            case QuoteBlock q:
                RenderQuote(q, 0);
                break;
            case MdTable t:
                RenderTable(t);
                break;
            case ThematicBreakBlock:
                RenderRule();
                break;
            default:
                RenderUnsupportedMarkdown(block);
                break;
        }
    }

    private void RenderHeading(HeadingBlock heading)
    {
        string text = RenderInlines(heading.Inline);

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
            _console.MarkupLine($"{_margin}[underline {color}]{text}[/]");
        else
            _console.MarkupLine($"{_margin}[{color}]{text}[/]");

        _console.WriteLine();
    }

    private void RenderParagraph(ParagraphBlock paragraph)
    {
        string text = RenderInlines(paragraph.Inline);
        foreach (var segment in text.Split('\n'))
            _console.Write(new Padder(new Markup(segment), new Padding(LeftMargin, 0, 0, 0)));
        _console.WriteLine();
    }

    private void RenderCodeBlock(CodeBlock code)
    {
        var lines = code.Lines.Lines
            .Select(l => l.ToString())
            .SkipLast(1)
            .ToList();

        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[^1]))
            lines.RemoveAt(lines.Count - 1);

        string content = string.Join('\n', lines);

        var panel = new Panel(StringUtilities.Escape(content))
        {
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("grey dim"),
            Padding = new Padding(1, 0)
        };

        if (code is FencedCodeBlock fenced && !string.IsNullOrEmpty(fenced.Info))
            panel.Header = new PanelHeader($"[grey]{fenced.Info}[/]");

        _console.WriteLine();
        _console.Write(new Padder(panel).PadLeft(LeftMargin));
        _console.WriteLine();
    }

    private void RenderList(ListBlock list, int level)
    {
        string indent = new(' ', level * 2);
        int index = 1;
        foreach (var item in list.Cast<ListItemBlock>())
        {
            string bullet = list.IsOrdered ? $"{index++}." : "•";
            string color = list.IsOrdered ? "blue" : "yellow";

            string text = item.FirstOrDefault() is ParagraphBlock p
                ? RenderInlines(p.Inline) : "";

            _console.MarkupLine($"{_margin}{indent}[{color}]{bullet}[/] {text}");

            foreach (var child in item.Skip(1))
            {
                if (child is ListBlock nested)
                    RenderList(nested, level + 1);
                else
                    RenderBlock(child);
            }

            if (list.IsLoose && index <= list.Count) _console.WriteLine();
        }
        if (level == 0) _console.WriteLine();
    }

    private void RenderQuote(QuoteBlock quote, int level)
    {
        string prefix = string.Concat(Enumerable.Repeat("[magenta dim]│[/] ", level + 1));
        if (level == 0) _console.WriteLine();
        foreach (var child in quote)
        {
            if (child is ParagraphBlock p)
            {
                string text = RenderInlines(p.Inline);
                _console.MarkupLine($"{_margin}{prefix}[italic grey]{text}[/]");
            }
            else if (child is QuoteBlock nested)
                RenderQuote(nested, level + 1);
            else
                RenderBlock(child);
        }
        if (level == 0) _console.WriteLine();
    }

    private void RenderRule()
    {
        _console.WriteLine();
        _console.Write(new Rule() { Style = Style.Parse("grey dim") });
        _console.WriteLine();
    }

    private void RenderTable(MdTable mdTable)
    {
        var table = new SpectreTable()
            .BorderColor(Color.Grey)
            .Border(TableBorder.Rounded);

        foreach (var row in mdTable.Cast<MdTableRow>())
        {
            var cells = row
                .Cast<MdTableCell>()
                .Select(cell => cell.FirstOrDefault() is ParagraphBlock p ? RenderInlines(p.Inline) : "")
                .ToArray();

            if (row.IsHeader)
            {
                foreach (var cell in cells)
                    table.AddColumn(new TableColumn($"[bold]{cell}[/]"));
            }
            else
            {
                table.AddRow(cells);
            }
        }

        _console.WriteLine();
        _console.Write(new Padder(table).PadLeft(LeftMargin));
        _console.WriteLine();
    }

    private static string RenderInlines(ContainerInline? inlines)
    {
        if (inlines == null) return "";

        var sb = new StringBuilder();

        foreach (var inline in inlines)
        {
            switch (inline)
            {
                case LiteralInline literal:
                    sb.Append(StringUtilities.Escape(literal.Content.ToString()));
                    break;

                case EmphasisInline emphasis:
                    string inner = RenderInlines(emphasis);

                    string? tag = null;
                    if (emphasis.DelimiterChar != '*' && emphasis.DelimiterChar != '_')
                    {
                        if (emphasis.DelimiterChar == '~' && emphasis.DelimiterCount == 2)
                            tag = "strikethrough";
                        else if (emphasis.DelimiterChar == '+' && emphasis.DelimiterCount == 2)
                            tag = "underline";
                    }
                    else
                        tag = emphasis.DelimiterCount == 2 ? "bold" : "italic";
                    
                    if (tag is null)
                    {
                        string delimiters = new(emphasis.DelimiterChar, emphasis.DelimiterCount);
                        sb.Append($"{delimiters}{inner}{delimiters}");
                        break;
                    }

                    sb.Append($"[{tag}]{inner}[/]");
                    break;

                case CodeInline code:
                    sb.Append($"[bold yellow on grey19]{StringUtilities.Escape(code.Content)}[/]");
                    break;

                case LinkInline link:
                    string label = RenderInlines(link);
                    if (link.IsImage)
                    {
                        sb.Append($"[grey][[image: {label}]][/]");
                        break;
                    }
                    sb.Append($"[link={link.Url ?? ""}][blue underline]{label}[/][/]");
                    sb.Append($" [grey]({StringUtilities.Escape(link.Url ?? "")})[/]");
                    break;
                
                case AutolinkInline autolink:
                    string autolinkUrl = StringUtilities.Escape(autolink.Url ?? "");
                    sb.Append($"[link={autolink.Url ?? ""}][blue underline]{autolinkUrl}[/][/]");
                    break;

                case LineBreakInline lb:
                    sb.Append(lb.IsHard ? '\n' : ' ');
                    break;
            }
        }

        return sb.ToString();
    }

    private void RenderUnsupportedMarkdown(Block block)
    {
        string raw = _source[block.Span.Start..(block.Span.End + 1)];
        foreach (var line in raw.Split('\n'))
            _console.MarkupLine($"{_margin}[grey]{StringUtilities.Escape(line)}[/]");
        _console.WriteLine();
    }
}
