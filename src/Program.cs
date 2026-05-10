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
    return await Main.RunAsync(path);
}
catch (Exception err)
{
    Console.Error.WriteLine($"Error: {err.Message}");
    return 1;
}
