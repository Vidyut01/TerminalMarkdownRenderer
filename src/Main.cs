using Markdig;
using Markdig.Syntax;

namespace MdRenderer;

public class Main
{
    public async Task<int> RunAsync(string filePath)
    {
        string content = await File.ReadAllTextAsync(filePath);
        MarkdownDocument doc = Markdown.Parse(content);
        
        var renderer = new Renderer();
        renderer.Render(doc);
        
        return 0;
    }
}
