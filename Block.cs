namespace MdRenderer;

enum BlockType
{
    Heading,
    ListItem,
    CodeBlock,
    Blockquote,
    Blank,
    Paragraph
};

record Block(BlockType Type, string Content, int Level = 0);
