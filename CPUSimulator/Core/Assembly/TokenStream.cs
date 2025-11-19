namespace CPUSimulator.Core.Assembly
{
    using CPUSimulator.Core.Assembly.Lexical;

    public unsafe struct TokenStream
    {
        private LexerState state;
        private LexerFlags flags;
        private Token current;
        private Token last;

        public TokenStream(SourceText* source, LexerFlags flags = LexerFlags.None)
        {
            state = new LexerState(source);
            this.flags = flags;
            current = Lexer.Step(ref state, flags);
            last = default;
        }

        public readonly Token Current => current;

        public readonly Token Last => last;

        public readonly bool CanAdvance => !current.IsEof;

        public LexerFlags Flags { readonly get => flags; set => flags = value; }

        private bool TryAdvance()
        {
            last = current;
            current = Lexer.Step(ref state, flags);
            return !current.IsEof;
        }

        private void Advance()
        {
            if (!CanAdvance)
            {
                throw new AssemblyException($"Unexpected end of file at line {last.Line}, column {last.Column}.");
            }
            TryAdvance();
        }

        public void ExpectDelimiter(char delimiter)
        {
            if (current.IsDelimiterOf(delimiter))
            {
                Advance();
                return;
            }
            throw new AssemblyException($"Expected delimiter '{delimiter}' at line {current.Line}, column {current.Column}.");
        }

        public void ExpectKeyword(Keyword keyword)
        {
            if (current.IsKeywordOf(keyword))
            {
                Advance();
                return;
            }
            throw new AssemblyException($"Expected keyword '{keyword}' at line {current.Line}, column {current.Column}.");
        }

        public Token ExpectIdentifier()
        {
            if (current.IsIdentifier)
            {
                Token identifier = current;
                Advance();
                return identifier;
            }
            throw new AssemblyException($"Expected identifier at line {current.Line}, column {current.Column}.");
        }

        public Token ExpectNumber()
        {
            if (current.IsNumber)
            {
                Token number = current;
                Advance();
                return number;
            }
            throw new AssemblyException($"Expected number at line {current.Line}, column {current.Column}.");
        }

        public Token ExpectLiteral()
        {
            if (current.IsLiteral)
            {
                Token literal = current;
                Advance();
                return literal;
            }
            throw new AssemblyException($"Expected literal at line {current.Line}, column {current.Column}.");
        }

        public bool TryKeyword(Keyword keyword)
        {
            if (current.IsKeywordOf(keyword))
            {
                Advance();
                return true;
            }
            return false;
        }

        public bool TryKeyword(out Keyword keyword)
        {
            if (current.IsKeyword)
            {
                keyword = (Keyword)current.Value;
                Advance();
                return true;
            }
            keyword = default;
            return false;
        }

        public bool TryDelimiter(char delimiter)
        {
            if (current.IsDelimiterOf(delimiter))
            {
                Advance();
                return true;
            }
            return false;
        }

        public bool TryOperator(Operator op)
        {
            if (current.IsOperatorOf(op))
            {
                Advance();
                return true;
            }
            return false;
        }

        public bool TryIdentifier(out Token identifier)
        {
            if (current.IsIdentifier)
            {
                identifier = current;
                Advance();
                return true;
            }
            identifier = default;
            return false;
        }

        public bool TryNumber(out Token number)
        {
            if (current.IsNumber)
            {
                number = current;
                Advance();
                return true;
            }
            number = default;
            return false;
        }

        public bool TryLiteral(out Token literal)
        {
            if (current.IsLiteral)
            {
                literal = current;
                Advance();
                return true;
            }
            literal = default;
            return false;
        }

        public bool TryRegister(out RegisterAddress register)
        {
            if (current.IsKeyword)
            {
                var kw = (Keyword)current.Value;
                if (!kw.IsRegister())
                {
                    register = default;
                    return false;
                }
                register = kw.ToRegisterAddress();
                Advance();
                return true;
            }
            register = default;
            return false;
        }
    }
}