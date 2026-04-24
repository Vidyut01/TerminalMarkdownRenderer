namespace MdRenderer;

class Parser
{
    private bool _inCodeBlock = false;
    private readonly List<string> _codeLines = [];

    public List<Block> Parse(string content)
    {
        List<Block> blocks = [];

        foreach(var line in content.Split('\n'))
        {
            Block? block = ParseLine(line);
            if (block != null)
                blocks.Add(block);
        }

        return blocks;
    }

    private Block? ParseLine(string line)
    {
        if (line.StartsWith("```"))
        {
            if (_inCodeBlock)
            {
                _inCodeBlock = false;
                return new Block(BlockType.CodeBlock, string.Join('\n', _codeLines));
            }

            _inCodeBlock = true;
            _codeLines.Clear();
            return null;
        }

        if (_inCodeBlock)
        {
            _codeLines.Add(line);
            return null;
        }
        
        if (string.IsNullOrWhiteSpace(line))
            return new Block(BlockType.Blank, "");
        
        if (line.StartsWith('#'))
        {
            int level = line.TakeWhile(c => c == '#').Count();
            if (level <= 6 && line[level] == ' ')
                return new Block(BlockType.Heading, line[level..].Trim(), level);
            return new Block(BlockType.Paragraph, line);
        }

        if (line.StartsWith("- ") || line.StartsWith("* "))
            return new Block(BlockType.ListItem, line[2..].Trim());

        if (line.StartsWith("> "))
            return new Block(BlockType.Blockquote, line[2..].Trim());

        return new Block(BlockType.Paragraph, line);
    }
}
