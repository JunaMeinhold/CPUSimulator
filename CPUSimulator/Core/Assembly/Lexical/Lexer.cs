namespace CPUSimulator.Core.Assembly.Lexical
{
    using CPUSimulator.Core.Assembly;

    public enum LexerFlags
    {
        None = 0,
        IncludeComments = 1 << 0,
        IncludeWhitespaces = 1 << 1,
    }

    public static unsafe class Lexer
    {
        private static readonly RadixTree<Keyword> keywords = new();
        private static readonly RadixTree<Operator> operators = new();
        private static readonly HashSet<byte> delimiters = [(byte)'(', (byte)')', (byte)'{', (byte)'}', (byte)'[', (byte)']', (byte)':', (byte)';', (byte)',', (byte)'.'];
        private static readonly HashSet<byte> lineTerms = [(byte)'\n', (byte)'\r'];

        static Lexer()
        {
            // Directives
            keywords.Insert("section"u8, Keyword.Section);
            keywords.Insert("global"u8, Keyword.Global);
            keywords.Insert("org"u8, Keyword.Org);

            // Section names
            keywords.Insert(".text"u8, Keyword.Text);
            keywords.Insert(".data"u8, Keyword.Data);
            keywords.Insert(".rodata"u8, Keyword.Rodata);
            keywords.Insert(".bss"u8, Keyword.Bss);

            // Data directives
            keywords.Insert("db"u8, Keyword.Db);
            keywords.Insert("dw"u8, Keyword.Dw);
            keywords.Insert("dd"u8, Keyword.Dd);
            keywords.Insert("dq"u8, Keyword.Dq);
            keywords.Insert("dbs"u8, Keyword.Dbs);

            // Reserve directives
            keywords.Insert("resb"u8, Keyword.Resb);
            keywords.Insert("resw"u8, Keyword.Resw);
            keywords.Insert("resd"u8, Keyword.Resd);
            keywords.Insert("resq"u8, Keyword.Resq);

            // Other directives
            keywords.Insert("align"u8, Keyword.Align);

            // Opcodes
            keywords.Insert("add"u8, Keyword.ADD);
            keywords.Insert("and"u8, Keyword.AND);
            keywords.Insert("call"u8, Keyword.CALL);
            keywords.Insert("cli"u8, Keyword.CLI);
            keywords.Insert("cmp"u8, Keyword.CMP);
            keywords.Insert("cmps"u8, Keyword.CMPS);
            keywords.Insert("cmpsb"u8, Keyword.CMPSB);
            keywords.Insert("cmpsw"u8, Keyword.CMPSW);
            keywords.Insert("dec"u8, Keyword.DEC);
            keywords.Insert("div"u8, Keyword.DIV);
            keywords.Insert("hlt"u8, Keyword.HLT);
            keywords.Insert("inc"u8, Keyword.INC);
            keywords.Insert("int"u8, Keyword.INT);
            keywords.Insert("iret"u8, Keyword.IRET);
            keywords.Insert("jae"u8, Keyword.JAE);
            keywords.Insert("jb"u8, Keyword.JB);
            keywords.Insert("jbe"u8, Keyword.JBE);
            keywords.Insert("jc"u8, Keyword.JC);
            keywords.Insert("jcxz"u8, Keyword.JCXZ);
            keywords.Insert("je"u8, Keyword.JE);
            keywords.Insert("jg"u8, Keyword.JG);
            keywords.Insert("jge"u8, Keyword.JGE);
            keywords.Insert("jl"u8, Keyword.JL);
            keywords.Insert("jle"u8, Keyword.JLE);
            keywords.Insert("jmp"u8, Keyword.JMP);
            keywords.Insert("jne"u8, Keyword.JNE);
            keywords.Insert("jnz"u8, Keyword.JNZ);
            keywords.Insert("loop"u8, Keyword.LOOP);
            keywords.Insert("loopne"u8, Keyword.LOOPNE);
            keywords.Insert("mov"u8, Keyword.MOV);
            keywords.Insert("mul"u8, Keyword.MUL);
            keywords.Insert("neg"u8, Keyword.NEG);
            keywords.Insert("nop"u8, Keyword.NOP);
            keywords.Insert("not"u8, Keyword.NOT);
            keywords.Insert("or"u8, Keyword.OR);
            keywords.Insert("pop"u8, Keyword.POP);
            keywords.Insert("push"u8, Keyword.PUSH);
            keywords.Insert("ret"u8, Keyword.RET);
            keywords.Insert("sti"u8, Keyword.STI);
            keywords.Insert("sub"u8, Keyword.SUB);
            keywords.Insert("xor"u8, Keyword.XOR);

            // Registers - RAX family
            keywords.Insert("rax"u8, Keyword.RAX);
            keywords.Insert("eax"u8, Keyword.EAX);
            keywords.Insert("ax"u8, Keyword.AX);
            keywords.Insert("al"u8, Keyword.AL);
            keywords.Insert("ah"u8, Keyword.AH);

            // RBX family
            keywords.Insert("rbx"u8, Keyword.RBX);
            keywords.Insert("ebx"u8, Keyword.EBX);
            keywords.Insert("bx"u8, Keyword.BX);
            keywords.Insert("bl"u8, Keyword.BL);
            keywords.Insert("bh"u8, Keyword.BH);

            // RCX family
            keywords.Insert("rcx"u8, Keyword.RCX);
            keywords.Insert("ecx"u8, Keyword.ECX);
            keywords.Insert("cx"u8, Keyword.CX);
            keywords.Insert("cl"u8, Keyword.CL);
            keywords.Insert("ch"u8, Keyword.CH);

            // RDX family
            keywords.Insert("rdx"u8, Keyword.RDX);
            keywords.Insert("edx"u8, Keyword.EDX);
            keywords.Insert("dx"u8, Keyword.DX);
            keywords.Insert("dl"u8, Keyword.DL);
            keywords.Insert("dh"u8, Keyword.DH);

            // Other registers
            keywords.Insert("rsp"u8, Keyword.RSP);
            keywords.Insert("rbp"u8, Keyword.RBP);
            keywords.Insert("rdi"u8, Keyword.RDI);
            keywords.Insert("rsi"u8, Keyword.RSI);

            // R8 family
            keywords.Insert("r8"u8, Keyword.R8);
            keywords.Insert("r8d"u8, Keyword.R8D);
            keywords.Insert("r8w"u8, Keyword.R8W);
            keywords.Insert("r8b"u8, Keyword.R8B);

            // R9 family
            keywords.Insert("r9"u8, Keyword.R9);
            keywords.Insert("r9d"u8, Keyword.R9D);
            keywords.Insert("r9w"u8, Keyword.R9W);
            keywords.Insert("r9b"u8, Keyword.R9B);

            // R10 family
            keywords.Insert("r10"u8, Keyword.R10);
            keywords.Insert("r10d"u8, Keyword.R10D);
            keywords.Insert("r10w"u8, Keyword.R10W);
            keywords.Insert("r10b"u8, Keyword.R10B);

            // R11 family
            keywords.Insert("r11"u8, Keyword.R11);
            keywords.Insert("r11d"u8, Keyword.R11D);
            keywords.Insert("r11w"u8, Keyword.R11W);
            keywords.Insert("r11b"u8, Keyword.R11B);

            // R12 family
            keywords.Insert("r12"u8, Keyword.R12);
            keywords.Insert("r12d"u8, Keyword.R12D);
            keywords.Insert("r12w"u8, Keyword.R12W);
            keywords.Insert("r12b"u8, Keyword.R12B);

            // R13 family
            keywords.Insert("r13"u8, Keyword.R13);
            keywords.Insert("r13d"u8, Keyword.R13D);
            keywords.Insert("r13w"u8, Keyword.R13W);
            keywords.Insert("r13b"u8, Keyword.R13B);

            // R14 family
            keywords.Insert("r14"u8, Keyword.R14);
            keywords.Insert("r14d"u8, Keyword.R14D);
            keywords.Insert("r14w"u8, Keyword.R14W);
            keywords.Insert("r14b"u8, Keyword.R14B);

            // R15 family
            keywords.Insert("r15"u8, Keyword.R15);
            keywords.Insert("r15d"u8, Keyword.R15D);
            keywords.Insert("r15w"u8, Keyword.R15W);
            keywords.Insert("r15b"u8, Keyword.R15B);

            // XMM registers
            keywords.Insert("xmm0"u8, Keyword.XMM0);
            keywords.Insert("xmm1"u8, Keyword.XMM1);
            keywords.Insert("xmm2"u8, Keyword.XMM2);
            keywords.Insert("xmm3"u8, Keyword.XMM3);
            keywords.Insert("xmm4"u8, Keyword.XMM4);
            keywords.Insert("xmm5"u8, Keyword.XMM5);
            keywords.Insert("xmm6"u8, Keyword.XMM6);
            keywords.Insert("xmm7"u8, Keyword.XMM7);
            keywords.Insert("xmm8"u8, Keyword.XMM8);
            keywords.Insert("xmm9"u8, Keyword.XMM9);
            keywords.Insert("xmm10"u8, Keyword.XMM10);
            keywords.Insert("xmm11"u8, Keyword.XMM11);
            keywords.Insert("xmm12"u8, Keyword.XMM12);
            keywords.Insert("xmm13"u8, Keyword.XMM13);
            keywords.Insert("xmm14"u8, Keyword.XMM14);
            keywords.Insert("xmm15"u8, Keyword.XMM15);

            operators.Insert("+"u8, Operator.Add);
            operators.Insert("-"u8, Operator.Subtract);
            operators.Insert("*"u8, Operator.Multiply);
            operators.Insert("/"u8, Operator.Divide);
        }

        public static Token Step(ref LexerState state, LexerFlags flags)
        {
        start:
            var source = state.Source;
            uint idx = state.Index;
            byte* pCur = state.Current;
            byte* pEnd = source->End;

            if (pCur == pEnd)
            {
                return state.MakeEOF();
            }

            byte c = *pCur;
            if (IsWhiteSpace((char)c))
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
                        goto start;
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
                            goto start;
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
                            goto start;
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
                            goto start;
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
                    return state.MakeToken(TokenType.Identifier, 0, pCur, pCur + matchLength + boundary);
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

            if (char.IsLetter((char)c) || c == '_')
            {
                uint boundary = FindWordBoundary(pCur, pEnd);
                return state.MakeToken(TokenType.Identifier, 0, pCur, pCur + boundary);
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
                    else if (c == 'f' || c == 'F')
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
                        break;
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
                throw new LexerException("Invalid sign for unsigned number.", state.Source, state.Index + (uint)(text - start), text);
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
                else if (c == ' ' || c == '\t' || c == '\f' || c == '\v')
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
                if (cr || c == '\n')
                {
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

        private static bool IsWhiteSpace(uint c)
        {
            if (c <= 0x20)
            {
                return c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == '\f' || c == '\v';
            }

            return (c >= 0x2000 && c <= 0x200A) || // Range for common space characters
                (c == 0x202F) ||                // Narrow no-break space
                (c == 0x3000) ||                // Ideographic space
                (c == 0x205F);                  // Medium Mathematical Space
        }
    }
}