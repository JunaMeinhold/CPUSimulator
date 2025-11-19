namespace CPUSimulator.Core.Assembly
{
    using CPUSimulator.Core.Assembly.Lexical;
    using CPUSimulator.Core.Decoding;
    using Hexa.NET.Utilities;
    using System.Text;

    public unsafe class Assembler
    {
        private readonly Dictionary<StringSpan, Label> labels = new(StringSpanComparer.Instance);
        private readonly Dictionary<ulong, string> symbols = [];
        private readonly List<(int index, ulong offset, StringSpan label)> references = [];
        private UnsafeList<Section> sections = [];
        private Section* current;

        public ulong BaseAddress = 16384;

        public Dictionary<StringSpan, Label> Labels => labels;

        public Dictionary<ulong, string> Symbols => symbols;

        public unsafe AssemblyResult Assemble(string code)
        {
            labels.Clear();
            symbols.Clear();
            references.Clear();

            foreach (Section section in sections)
            {
                section.Data.Release();
                section.Instructions.Release();
            }
            sections.Clear();

            var count = Encoding.UTF8.GetByteCount(code);
            var text = AllocT<byte>(count + 1);
            Encoding.UTF8.GetBytes(code, new Span<byte>(text, count));
            text[count] = 0;

            SourceText sourceText = new()
            {
                Text = text,
                Length = (uint)count
            };

            TokenStream stream = new(&sourceText, LexerFlags.None);
            ulong instructionOffset = 0;

            while (stream.CanAdvance)
            {
                Parse(ref stream, ref instructionOffset);
            }

            var textSection = sections.FirstOrDefault(x => x.Type == SectionType.Text);
            if (textSection.Instructions.Count > 0)
            {
                var instructions = textSection.Instructions;
                foreach (var (index, offset, labelName) in references)
                {
                    var instruction = instructions.GetPointer(index);
                    var label = labels[labelName];
                    switch (label.Section)
                    {
                        case SectionType.None:
                            break;

                        case SectionType.Text:
                            if (IsRelative(instruction->OpCode))
                            {
                                var relative = (long)label.Offset - (long)offset;
                                instruction->Immediate = *(ulong*)&relative;
                            }
                            else
                            {
                                instruction->Immediate = label.Offset + BaseAddress;
                            }

                            break;

                        case SectionType.Data:
                            instruction->Immediate = label.Offset + BaseAddress;
                            break;

                        case SectionType.RoData:
                            break;

                        case SectionType.Bss:
                            break;
                    }
                }
            }

            Free(text);

            return new AssemblyResult(sections);
        }

        private bool IsRelative(OpCode code)
        {
            return code switch
            {
                OpCode.CALL => true,
                OpCode.JMP => true,
                OpCode.JG => true,
                OpCode.JGE => true,
                OpCode.JL => true,
                OpCode.JLE => true,
                OpCode.JNE => true,
                OpCode.JNZ => true,
                OpCode.JE => true,
                _ => false,
            };
        }

        private void Parse(ref TokenStream stream, ref ulong offset)
        {
            if (stream.TryKeyword(out var keyword))
            {
                if (keyword == Keyword.Section)
                {
                    ParseSection(ref stream);
                    return;
                }

                if (keyword == Keyword.Global)
                {
                    ParseGlobal(ref stream);
                    return;
                }

                if (TryParseSpecialInstruction(ref stream, keyword))
                {
                    return;
                }

                if (keyword.IsOpCode())
                {
                    ParseInstruction(ref stream, keyword.ToOpCode(), ref offset);
                    return;
                }

                throw new InvalidOperationException("Unexpected keyword.");
            }
            else if (TryParseLabel(ref stream, offset))
            {
                return;
            }

            throw new InvalidOperationException("Unexpected token.");
        }

        private void ParseGlobal(ref TokenStream stream)
        {
            throw new NotImplementedException();
        }

        private bool TryParseLabel(ref TokenStream stream, ulong offset)
        {
            if (!stream.TryIdentifier(out var identifier))
            {
                return false;
            }
            stream.ExpectDelimiter(':');
            labels.Add(identifier, new Label(identifier, offset, current->Type));
            return true;
        }

        private void ParseInstruction(ref TokenStream stream, OpCode opCode, ref ulong offset)
        {
            if (current->Type != SectionType.Text)
            {
                throw new InvalidOperationException("Instructions are only allowed in .text section");
            }

            Instruction instruction = default;
            int instructionIndex = current->Instructions.Count;

            instruction.OpCode = opCode;

            if (opCode.IsBinary())
            {
                (instruction.Immediate, instruction.OperandSource1) = ParseOperand(ref stream, ref instruction, true, instructionIndex, offset);
                stream.ExpectDelimiter(',');
                (instruction.Immediate, instruction.OperandSource2) = ParseOperand(ref stream, ref instruction, false, instructionIndex, offset);
            }
            else if (opCode.IsUnary())
            {
                (instruction.Immediate, instruction.OperandSource1) = ParseOperand(ref stream, ref instruction, true, instructionIndex, offset);
            }
            else if (!opCode.HasNoOperands())
            {
                throw new NotImplementedException("Missing op code handling");
            }

            offset += (uint)instruction.Size();
            current->Instructions.Add(instruction);
        }

        private bool TryParseSpecialInstruction(ref TokenStream stream, Keyword keyword)
        {
            switch (keyword)
            {
                case Keyword.Org:
                    ParseOrgDirective(ref stream);
                    return true;

                case Keyword.Db:
                    ParseDataDirective(ref stream, 1);
                    return true;

                case Keyword.Dw:
                    ParseDataDirective(ref stream, 2);
                    return true;

                case Keyword.Dd:
                    ParseDataDirective(ref stream, 4);
                    return true;

                case Keyword.Dq:
                    ParseDataDirective(ref stream, 8);
                    return true;

                case Keyword.Resb:
                    ReserveSpace(ref stream, 1);
                    return true;

                case Keyword.Resw:
                    ReserveSpace(ref stream, 2);
                    return true;

                case Keyword.Resd:
                    ReserveSpace(ref stream, 4);
                    return true;

                case Keyword.Resq:
                    ReserveSpace(ref stream, 8);
                    return true;

                case Keyword.Dbs:
                    ParseStringDirective(ref stream);
                    return true;

                case Keyword.Align:
                    AlignCurrentSection(ref stream);
                    return true;

                default:
                    return false;
            }
        }

        private void ParseOrgDirective(ref TokenStream stream)
        {
            var token = stream.ExpectNumber();
            ulong baseAddress = token.Number.U64;
            current->BaseAddress = baseAddress;
        }

        private void AlignCurrentSection(ref TokenStream stream)
        {
            var token = stream.ExpectNumber();
            int alignment = (int)token.Number.U64;

            if (alignment <= 0)
            {
                throw new InvalidOperationException("Invalid alignment value.");
            }

            int oldSize = current->Data.Count;
            int padding = (alignment - oldSize % alignment) % alignment;
            int newSize = current->Data.Count + padding;
            current->Data.Resize(newSize);
            Memset(current->Data.Data + oldSize, 0, padding);
        }

        private void ParseStringDirective(ref TokenStream stream)
        {
            var token = stream.ExpectLiteral();

            if (token.Length == 0)
            {
                current->Data.Add(0);
                return;
            }

            byte* src = token.Text;
            uint length = token.Length;

            int size = current->Data.Count;
            current->Data.Resize(size + (int)length + 1);
            Memcpy(src, current->Data.Data + size, length);
            current->Data.Data[size + length] = 0; // Null terminator
        }

        private void ReserveSpace(ref TokenStream stream, int size)
        {
            var token = stream.ExpectNumber();
            uint count = (uint)token.Number.U64;

            int additional = (int)count * size;
            int oldSize = current->Data.Count;
            int newSize = current->Data.Count + additional;
            current->Data.Resize(newSize);
            Memset(current->Data.Data + oldSize, 0, additional);
        }

        private void ParseDataDirective(ref TokenStream stream, int size)
        {
            while (!stream.TryDelimiter(','))
            {
                var token = stream.ExpectNumber();
                ulong value = token.Number.U64;

                for (int i = 0; i < size; i++)
                {
                    current->Data.Add((byte)((value >> (8 * i)) & 0xFF));
                }
            }
        }

        private void ParseSection(ref TokenStream stream)
        {
            if (!stream.TryKeyword(out var keyword))
            {
                var token = stream.Current;
                throw new InvalidOperationException($"Expected section name at line {token.Line}, column {token.Column}.");
            }

            SectionType sectionType = keyword switch
            {
                Keyword.Text => SectionType.Text,
                Keyword.Data => SectionType.Data,
                Keyword.Rodata => SectionType.RoData,
                Keyword.Bss => SectionType.Bss,
                _ => throw new InvalidOperationException($"Invalid section keyword.")
            };

            foreach (Section section1 in sections)
            {
                if (section1.Type == sectionType)
                {
                    throw new InvalidOperationException("Duplicate section.");
                }
            }

            Section section = new()
            {
                Type = sectionType,
                BaseAddress = ulong.MaxValue,
            };

            int index = sections.Count;
            sections.Add(section);
            current = sections.GetPointer(index);
        }

        private unsafe (ulong value, OperandSource flag) ParseOperand(ref TokenStream stream, ref Instruction instruction, bool firstOperator, int instructionIndex, ulong offset)
        {
            if (stream.TryDelimiter('['))
            {
                if (stream.TryRegister(out var registerAddress))
                {
                    instruction.Flags |= firstOperator ? InstructionFlags.Source1AsAddress : InstructionFlags.Source2AsAddress;
                    stream.ExpectDelimiter(']');
                    return (0, Instruction.Convert(registerAddress));
                }

                if (stream.TryIdentifier(out var identifier))
                {
                    references.Add((instructionIndex, offset, identifier));
                    stream.ExpectDelimiter(']');
                    return (0, OperandSource.ImmAddress);
                }

                if (stream.TryNumber(out var number))
                {
                    stream.ExpectDelimiter(']');
                    return (number.Number.U64, OperandSource.ImmAddress);
                }

                var current = stream.Current;
                throw new AssemblyException($"Unexpected token in address expression at line {current.Line}, column {current.Column}.");
            }

            if (stream.TryRegister(out var register))
            {
                return (0, Instruction.Convert(register));
            }

            if (stream.TryIdentifier(out var identifierToken))
            {
                references.Add((instructionIndex, offset, identifierToken));
                return (0, OperandSource.Imm64);
            }

            if (stream.TryNumber(out var numberToken))
            {
                return (numberToken.Number.U64, ConvertType(numberToken.NumberType));
            }

            var token = stream.Current;
            throw new AssemblyException($"Expected operand at line {token.Line}, column {token.Column}.");
        }

        private static unsafe OperandSource ConvertType(NumberType type)
        {
            return type switch
            {
                NumberType.U8 or NumberType.I8 => OperandSource.Imm8,
                NumberType.U16 or NumberType.I16 => OperandSource.Imm16,
                NumberType.U32 or NumberType.I32 or NumberType.F32 => OperandSource.Imm32,
                NumberType.U64 or NumberType.I64 or NumberType.F64 => OperandSource.Imm64,
                _ => throw new InvalidOperationException("Invalid number type."),
            };
        }
    }
}