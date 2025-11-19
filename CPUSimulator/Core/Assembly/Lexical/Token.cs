namespace CPUSimulator.Core.Assembly.Lexical
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct Token
    {
        [FieldOffset(0)]
        public byte* Text;
        [FieldOffset(8)]
        public uint Length;
        [FieldOffset(12)]
        public uint Line;
        [FieldOffset(16)]
        public uint Column;
        [FieldOffset(20)]
        public TokenType Type;
        [FieldOffset(21)]
        public NumberType NumberType;
        [FieldOffset(22)]
        public TokenFlags Flags;
        [FieldOffset(24)]
        public long Value;
        [FieldOffset(24)]
        public Number Number;

        public readonly bool IsEof => Type == TokenType.Eof;

        public readonly bool IsKeyword => Type == TokenType.Keyword;

        public readonly bool IsIdentifier => Type == TokenType.Identifier;

        public readonly bool IsNumber => Type == TokenType.Number;

        public readonly bool IsLiteral => Type == TokenType.Literal;

        public readonly bool IsDelimiter => Type == TokenType.Delimiter;

        public readonly bool IsOperator => Type == TokenType.Operator;

        public readonly bool IsComment => Type == TokenType.Comment;

        public readonly bool IsWhitespace => Type == TokenType.Whitespace;

        public readonly bool IsNewLine => Type == TokenType.NewLine;

        public readonly bool IsKeywordOf(Keyword keyword)
        {
            return IsKeyword && Value == (long)keyword;
        }

        public readonly bool IsDelimiterOf(char delimiter)
        {
            return IsDelimiter && Value == delimiter;
        }

        public readonly bool IsOperatorOf(Operator op)
        {
            return IsOperator && Value == (long)op;
        }

        public readonly bool IsOperatorOfAny(ReadOnlySpan<Operator> operators)
        {
            if (!IsOperator) return false;
            foreach (var op in operators)
            {
                if ((long)op == Value) return true;
            }
            return false;
        }

        public readonly bool IsOperatorOfAny<TSet>(in TSet set) where TSet : IReadOnlySet<Operator>
        {
            return IsKeyword && set.Contains((Operator)Value);
        }

        public readonly bool IsDelimiterOfAny(ReadOnlySpan<byte> span)
        {
            return IsDelimiter && span.Contains((byte)Value);
        }

        public readonly bool IsDelimiterOfAny<TSet>(in TSet set) where TSet : IReadOnlySet<byte>
        {
            return IsKeyword && set.Contains((byte)Value);
        }

        public readonly bool IsKeywordOfAny(ReadOnlySpan<Keyword> keywords)
        {
            if (!IsKeyword) return false;
            foreach (var word in keywords)
            {
                if ((long)word == Value) return true;
            }
            return false;
        }

        public readonly bool IsKeywordOfAny<TSet>(in TSet set) where TSet : IReadOnlySet<Keyword>
        {
            return IsKeyword && set.Contains((Keyword)Value);
        }

        public readonly Span<byte> AsSpan()
        {
            return new(Text, (int)Length);
        }

        public override readonly string ToString()
        {
            return Encoding.UTF8.GetString(Text, (int)Length);
        }
    }
}
