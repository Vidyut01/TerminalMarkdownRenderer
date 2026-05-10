using Markdig;
using Markdig.Syntax;

namespace MdRenderer;

public class Main
{
    public async Task<int> RunAsync(string filePath)
    {
        using var reader = new StreamReader(filePath);
        string content = reader.ReadToEnd();

        var pipeline = new MarkdownPipelineBuilder().UseEmphasisExtras().Build();
        var doc = Markdown.Parse(content, pipeline);
        var renderer = new MdRenderer.Renderer();
        var lines = renderer.Render(doc);

        var pager = new MdRenderer.Pager(lines, filePath);
        pager.Run();
        
        return 0;
    }
}
