namespace CPUSimulator.Core.Assembly.Lexical
{
    using System;

    public unsafe struct SourceText
    {
        public byte* Text;
        public uint Length;

        public readonly byte* End => Text + Length;
    }

    public unsafe struct LexerState
    {
        public SourceText* Source;
        public uint Index;
        public uint Line;
        public uint Column;

        public LexerState(SourceText* source) 
        {
            Source = source;
        }

        public byte* Current => Source->Text + Index;

        public void Advance(uint skip, uint columns, uint lines)
        {
            Index += skip;
            Line += lines;
            if (lines > 0)
            {
                Column = columns;
            }
            else
            {
                Column += columns;
            }
        }

        public Token MakeToken(TokenType type, long value, byte* start, byte* end)
        {
            uint len = (uint)(end - start);
            Token token = new()
            {
                Text = start,
                Length = len,
                Line = Line,
                Column = Column,
                Type = type,
                Value = value
            };
            Advance(len, len, 0);
            return token;
        }

        public Token MakeToken(Keyword keyword, byte* start, byte* end)
        {
            return MakeToken(TokenType.Keyword, (long)keyword, start, end);
        }

        public Token MakeToken(Operator op, byte* start, byte* end)
        {
            return MakeToken(TokenType.Operator, (long)op, start, end);
        }

        public Token MakeComment(byte* start, byte* end, uint columns, uint lines)
        {
            Token token = new()
            {
                Text = start,
                Length = (uint)(end - start),
                Line = Line,
                Column = Column,
                Type = TokenType.Comment
            };
            Advance((uint)(end - start), columns, lines);
            return token;
        }

        public Token MakeNumber(byte* start, byte* end, NumberType type, Number value)
        {
            uint len = (uint)(end - start); 
            Token token = new()
            {
                Text = start,
                Length = len,
                Line = Line,
                Column = Column,
                Type = TokenType.Number,
                NumberType = type,
                Number = value
            };
            Advance(len, len, 0);
            return token;
        }

        public Token MakeEOF()
        {
            return new()
            {
                Type = TokenType.Eof,
                Length = 0,
                Text = Source->End,
                Column = Column,
                Line = Line,
            };
        }
    }
}
