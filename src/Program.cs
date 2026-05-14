using System.Runtime.InteropServices;
using Markdig;
using MdRenderer;

if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: mdrenderer <file.md>");
    return 1;
}

string path = args[0];

if (Directory.Exists(path))
{
    Console.Error.WriteLine($"mdrender: {path}: Is a directory");
    return 1;
}

try
{
    Console.CancelKeyPress += (_, e) => e.Cancel = true;
    using var _ = PosixSignalRegistration.Create(PosixSignal.SIGTERM, _ =>
    {
        Writer.CloseAlternateBuffer();
        Environment.Exit(0);
    });

    using var reader = new StreamReader(path);
    string content = reader.ReadToEnd();

    var pipeline = new MarkdownPipelineBuilder().UseEmphasisExtras().UsePipeTables().Build();
    var doc = Markdown.Parse(content, pipeline);
    var renderer = new Renderer();
    var lines = renderer.Render(doc, content);

    var pager = new Pager(lines, path);
    pager.Run();

    return 0;
}
catch (Exception err)
{
    Console.Error.WriteLine($"Error: {err.Message}");
    return 1;
}
