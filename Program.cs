using MdRenderer;

if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: mdrenderer <file.md>");
    return 1;
}

string path = args[0];

// if (!File.Exists(path))
// {
//     Console.Error.WriteLine($"File not found: {path}");
//     return 1;
// }

try
{
    using var reader = new StreamReader(path);
    string content = await reader.ReadToEndAsync();
    // string content = File.ReadAllText(path);
    // Console.WriteLine(content);
    var parser = new Parser();
    var blocks = parser.Parse(content);

    // foreach (var block in blocks) {
    //     Console.WriteLine($"[{block.Type}] {block.Content}");
    // }

    Renderer.Render(blocks);
}
catch (Exception err)
{
    Console.Error.WriteLine($"Error: {err.Message}");
    return 1;
}



return 0;