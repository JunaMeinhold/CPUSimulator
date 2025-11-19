namespace CPUSimulator.Core.Assembly.Lexical
{
    using CPUSimulator.Core.Assembly;

    public enum LexerFlags
    {
        None = 0,
        IncludeComments = 1 << 0,
        IncludeWhitespaces = 1 << 1,
    }

    public unsafe static class Lexer
    {
        private static readonly RadixTree<Keyword> keywords = new();
        private static readonly RadixTree<Operator> operators = new();
        private static readonly HashSet<byte> delimiters = [(byte)'(', (byte)')', (byte)'{', (byte)'}', (byte)'[', (byte)']', (byte)';', (byte)',', (byte)'.'];
        private static readonly HashSet<byte> lineTerms = [(byte)'\n', (byte)'\r'];

        static Lexer()
        {
            keywords.Insert("mov"u8, Keyword.Mov);
            keywords.Insert("add"u8, Keyword.Add);
            keywords.Insert("sub"u8, Keyword.Sub);
            keywords.Insert("mul"u8, Keyword.Mul);
            keywords.Insert("div"u8, Keyword.Div);

            operators.Insert("+"u8, Operator.Add);
            operators.Insert("-"u8, Operator.Subtract);
            operators.Insert("*"u8, Operator.Multiply);
            operators.Insert("/"u8, Operator.Divide);
        }

        public static Token Step(ref LexerState state, LexerFlags flags)
        {
            var source = state.Source;
            uint idx = state.Index;
            byte* pCur = state.Current;
            byte* pEnd = source->End;

            if (pCur == pEnd)
            {
                return state.MakeEOF();
            }

            byte c = *pCur;
            if (char.IsWhiteSpace((char)c))
            {
                uint skip = SkipWhitespaces(pCur, pEnd, false, out uint columns, out uint lines);
                if (skip != 0)
                {
                    if ((flags & LexerFlags.IncludeWhitespaces) != 0)
                    {
                        var token = state.MakeToken(TokenType.Whitespace, 0, pCur, pCur + skip);
                        state.Advance(skip, columns, lines);
                        return token;
                    }
                    else
                    {
                        state.Advance(skip, columns, lines);
                    }
                }
            }

            if (c == '/' && pCur + 1 != pEnd)
            {
                var cNext = pCur[1];
                if (cNext == '/')
                {
                    var ix = IndexOfAny(pCur + 2, pEnd, lineTerms);
                    if (ix != uint.MaxValue)
                    {
                        var len = ix + 2;
                        if ((flags & LexerFlags.IncludeComments) != 0)
                        {
                            return state.MakeComment(pCur, pCur + len, len, 0);
                        }
                        else
                        {
                            state.Advance(len, len, 0);
                        }
                    }
                    else
                    {
                        if ((flags & LexerFlags.IncludeComments) != 0)
                        {
                            return state.MakeComment(pCur, pEnd, (uint)(pEnd - pCur), 0);
                        }
                        else
                        {
                            state.Advance((uint)(pEnd - pCur), (uint)(pEnd - pCur), 0);
                        }
                    }
                }
                else if (cNext == '*')
                {
                    var ix = LookAhead(pCur + 2, pEnd, "*/"u8, out uint columns, out uint lines);
                    if (ix != uint.MaxValue)
                    {
                        ix += 2;
                        if ((flags & LexerFlags.IncludeComments) != 0)
                        {
                            return state.MakeComment(pCur, pCur + ix, columns, lines);
                        }
                        else
                        {
                            state.Advance(ix, columns, lines);
                        }
                    }
                    else
                    {
                        throw new LexerException("Unterminated comment.", source, idx, pCur);
                    }
                }
            }

            nuint matchLength;
            if (keywords.TryLookupLongestMatch(pCur, pEnd, out var keyword, out matchLength) && keyword != Keyword.Unknown)
            {
                uint boundary = FindWordBoundary(pCur + matchLength, pEnd);
                if (boundary == 0)
                {
                    return state.MakeToken(keyword, pCur, pCur + matchLength); // Keyword.
                }
                else
                {
                    return state.MakeToken(TokenType.Identifier, 0, pCur, pEnd);
                }
            }

            bool sign = c == '-';
            if (char.IsDigit((char)c) || (sign && pCur + 1 != pEnd && char.IsDigit((char)pCur[1])))
            {
                return ParseNumber(ref state, pCur + (sign ? 1 : 0), pEnd, sign);
            }

            if (operators.TryLookupLongestMatch(pCur, pEnd, out var op, out matchLength) && op != Operator.Unknown)
            {
                return state.MakeToken(op, pCur, pCur + matchLength); // Operator.
            }

            if (delimiters.Contains(c))
            {
                return state.MakeToken(TokenType.Delimiter, c, pCur, pCur + 1);
            }

            state.Advance(1, 1, 0);
            return default;
        }

        private static Token ParseNumber(ref LexerState state, byte* text, byte* end, bool hasSign)
        {
            byte* start = text;
            bool isHex = false;
            bool isBinary = false;
            if (text + 1 != end)
            {
                if (text[0] == '0' && (text[1] == 'x' || text[1] == 'X'))
                {
                    isHex = true;
                    text += 2;
                }
                else if (text[0] == '0' && (text[1] == 'b' || text[1] == 'B'))
                {
                    isBinary = true;
                    text += 2;
                }
            }

            ulong value = 0;
            ulong upper = 0;
            uint fractDigits = 0;
            bool isFloat = false;
            bool isDouble = false;
            bool isLong = false;
            bool isLongLong = false;
            bool isUnsigned = false;
            while (text != end)
            {
                ulong c = *text;
                if (isHex)
                {
                    if (char.IsDigit((char)c))
                    {
                        value = (value << 4) | c - (byte)'0';
                    }
                    else if (c >= 'a' && c <= 'f')
                    {
                        value = (value << 4) | c - (byte)'a' + 10;
                    }
                    else if (c >= 'A' && c <= 'F')
                    {
                        value = (value << 4) | c - (byte)'A' + 10;
                    }
                    else
                    {
                        throw new LexerException("Invalid hexadecimal digit.", state.Source, state.Index + (uint)(text - start), text);
                    }
                }
                else if (isBinary)
                {
                    if (c == '0' || c == '1')
                    {
                        value = (value << 1) | c - (byte)'0';
                    }
                    else
                    {
                        throw new LexerException("Invalid binary digit.", state.Source, state.Index + (uint)(text - start), text);
                    }
                }
                else
                {
                    if (char.IsDigit((char)c))
                    {
                        value = value * 10 + (c - (byte)'0');
                    }
                    else if (c == '.')
                    { 
                        isDouble = true;
                        ++text;
                        upper = value;
                        value = 0;
                        continue;
                    }
                    else if (c == '_')
                    {
                        ++text;
                        continue;
                    }
                    else if (c == 'f' || c == 'F' )
                    {
                        isFloat = true;
                        isDouble = false;
                        ++text;
                        break;
                    }
                    else if (c == 'd' || c == 'D')
                    {
                        isFloat = false;
                        isDouble = true;
                        ++text;
                        break;
                    }
                    else if (c == 'l' || c == 'L')
                    {
                        ++text;
                        if (isLong)
                        {
                            isLongLong = true;
                            isLong = false;
                            break;
                        }
                        isLong = true;
                        
                        continue;
                    }
                    else if (c == 'u' || c == 'U')
                    {
                        isUnsigned = true;
                        ++text;
                        continue;
                    }
                    else
                    {
                        throw new LexerException("Invalid decimal digit.", state.Source, state.Index + (uint)(text - start), text);
                    }
                }
                ++text;
                if (isDouble)
                {
                    ++fractDigits;
                }
            }

            if (hasSign && isUnsigned)
            {
                throw new LexerException();
            }

            sbyte multiplier = hasSign ? (sbyte)-1 : (sbyte)1;

            Number number = default;
            NumberType numberType = NumberType.I32;
            if (isFloat)
            {
                numberType = NumberType.F32;
                float f = multiplier * (upper + value / MathF.Pow(10, fractDigits));
                number = new() { F32 = f };
            }
            else if (isDouble)
            {
                numberType = NumberType.F64;
                double d = multiplier * (upper + value / Math.Pow(10, fractDigits));
                number = new() { F64 = d };
            }
            else if (isLongLong)
            {
                numberType = isUnsigned ? NumberType.U64 : NumberType.I64;
                number = isUnsigned ? new() { U64 = value } : new() { I64 = multiplier * (long)value };
            }
            else if (isLong)
            {
                numberType = isUnsigned ? NumberType.U32 : NumberType.I32;
                number = isUnsigned ? new() { U32 = (uint)value } : new() { I32 = multiplier * (int)value };
            }
            else
            {
                if (value > uint.MaxValue)
                {
                    numberType = isUnsigned ? NumberType.U64 : NumberType.I64;
                    number = isUnsigned ? new() { U64 = value } : new() { I64 = multiplier * (long)value };
                }
                else if (value > ushort.MaxValue)
                {
                    numberType = isUnsigned ? NumberType.U32 : NumberType.I32;
                    number = isUnsigned ? new() { U32 = (uint)value } : new() { I32 = multiplier * (int)value };
                }
                else if (value > byte.MaxValue)
                {
                    numberType = isUnsigned ? NumberType.U16 : NumberType.I16;
                    number = isUnsigned ? new() { U16 = (ushort)value } : new() { I16 = (short)(multiplier * (short)value) };
                }
                else
                {
                    numberType = isUnsigned ? NumberType.U8 : NumberType.I8;
                    number = isUnsigned ? new() { U8 = (byte)value } : new() { I8 = (sbyte)(multiplier * (sbyte)value) };
                }
            }

            return state.MakeNumber(start, text, numberType, number);
        }

        private static uint SkipWhitespaces(byte* start, byte* end, bool stopAtLine, out uint columns, out uint lines)
        {
            columns = 0;
            lines = 0;
            var cur = start;
            while (cur != end)
            {
                var c = *cur;
                bool cr = c == '\r';
                if (cr || c == '\n')
                {
                    if (stopAtLine && cur != start)
                    {
                        break;
                    }
                    uint width = 1;
                    if (cr && cur + 1 != end && cur[1] == '\n')
                    {
                        width = 2;
                    }

                    cur += width;
                    ++lines;
                    columns = 0;
                    if (stopAtLine)
                    {
                        break;
                    }
                }
                else if (c == ' ')
                {
                    ++columns;
                    ++cur;
                }
                else
                {
                    break;
                }
            }

            return (uint)(cur - start);
        }

        private static uint LookAhead(byte* text, byte* end, ReadOnlySpan<byte> needle, out uint columns, out uint lines)
        {
            byte* start = text;
            int ix = 0;
            columns = 0;
            lines = 0;
            while (text != end)
            {
                var c = *text;
                bool cr = c == '\r';
                if (cr || c == '\n') { 
                    uint width = 1;
                    if (cr && text + 1 != end && text[1] == '\n')
                    {
                        width = 2;
                    }
                    text += width;
                    ++lines;
                    columns = 0;
                    ix = 0;
                    continue;
                }
                if (c == needle[ix])
                {
                    ++ix;
                    if (ix == needle.Length)
                    {
                        return (uint)(text + 1 - start);
                    }
                }
                else
                {
                    ix = 0;
                    continue;
                }

                ++text;
                ++columns;
            }

            return uint.MaxValue;
        }

        private static uint FindWordBoundary(byte* text, byte* end)
        {
            byte* start = text;
            while (text != end)
            {
                var c = *text;
                if (!char.IsLetterOrDigit((char)c) && c != '_')
                {
                    break;
                }
                ++text;
            }

            return (uint)(text - start);
        }

        private static uint IndexOf(byte* text, byte* end, byte c)
        {
            byte* start = text;
            while (text != end)
            {
                if (*text == c)
                {
                    return (uint)(text - start);
                }
                ++text;
            }
            return uint.MaxValue;
        }

        private static uint IndexOfAny<TSet>(byte* text, byte* end, TSet chars) where TSet : ISet<byte>
        {
            byte* start = text;
            while (text != end)
            {
                if (chars.Contains(*text))
                {
                    return (uint)(text - start);
                }
                ++text;
            }
            return uint.MaxValue;
        }
    }
}
