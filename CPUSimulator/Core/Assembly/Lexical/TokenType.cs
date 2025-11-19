namespace CPUSimulator.Core.Assembly.Lexical
{
    public enum TokenType : byte
    {
        Unknown,
        Eof,
        Keyword,
        Identifier,
        Number,
        Delimiter,
        Operator,
        Literal,
        Comment,
        Whitespace,
        NewLine,
    }
}
