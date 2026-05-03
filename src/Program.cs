using MdRenderer;

if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: mdrenderer <file.md>");
    return 1;
}

try
{
    var app = new Main();
    return await app.RunAsync(args[0]);
}
catch (Exception err)
{
    Console.Error.WriteLine($"Error: {err.Message}");
    return 1;
}
