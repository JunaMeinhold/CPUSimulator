namespace CPUSimulator.Core.Assembly
{
    using CPUSimulator.Core.Decoding;
    using Hexa.NET.Utilities;
    using Microsoft.CodeAnalysis;
    using System.Globalization;
    using System.Text;

    public unsafe class Assembler
    {
        private readonly Dictionary<string, Label> labels = [];
        private readonly Dictionary<ulong, string> symbols = [];
        private readonly List<(int index, ulong offset, string label)> references = [];
        private UnsafeList<Section> sections = [];
        private Section* current;

        public ulong BaseAddress = 16384;

        public Dictionary<string, Label> Labels => labels;

        public Dictionary<ulong, string> Symbols => symbols;

        public unsafe AssemblyResult Assemble(string code)
        {
            labels.Clear();
            symbols.Clear();
            references.Clear();
            ReadOnlySpan<char> span = code;

            foreach (Section section in sections)
            {
                section.Data.Release();
                section.Instructions.Release();
            }
            sections.Clear();

            int lineIndex = 0;
            ulong instructionOffset = 0;
            while (span.Length > 0)
            {
                bool crlf = false;
                int idx = span.IndexOf('\n');
                if (idx == -1) idx = span.Length;
                if (idx > 0 && span[idx - 1] == '\r')
                {
                    idx--; crlf = true;
                }

                var line = span[..idx];
                Parse(line, lineIndex, ref instructionOffset);
                lineIndex++;
                if (crlf) idx += 2;
                else idx++;

                if (idx >= span.Length) break;
                span = span[idx..];
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

        private void Parse(ReadOnlySpan<char> line, int lineIndex, ref ulong offset)
        {
            // Pre process
            line = line.Trim();
            if (line.IsWhiteSpace()) return;
            int commentIdx = line.IndexOf(';');
            if (commentIdx != -1)
            {
                line = line[..commentIdx]; // trim out comment.
            }

            line = line.Trim();
            if (line.IsWhiteSpace()) return;

            if (line.StartsWith("section", StringComparison.OrdinalIgnoreCase))
            {
                ParseSection(line);
                return;
            }

            int end = line.IndexOf(' ');
            if (end == -1) end = line.Length;

            if (ParseLabel(ref line, offset, ref end))
            {
                return;
            }

            symbols.Add(offset, $"line: {lineIndex}, {line}");

            if (TryParseSpecialInstruction(ref line, end))
            {
                return;
            }

            ParseInstruction(ref line, ref offset, end);
        }

        private bool ParseLabel(ref ReadOnlySpan<char> line, ulong offset, ref int end)
        {
            var part = line[..end];
            if (part.EndsWith(':')) // label
            {
                var label = part[..^1].ToString();
                labels.Add(label, new Label(label, offset, current->Type));

                if (end == line.Length) return true;
                line = line[(end + 1)..].Trim();

                if (line.IsWhiteSpace()) return true;

                end = line.IndexOf(' ');
                if (end == -1) end = line.Length;
            }
            return false;
        }

        private void ParseInstruction(ref ReadOnlySpan<char> line, ref ulong offset, int end)
        {
            if (current->Type != SectionType.Text)
            {
                throw new InvalidOperationException("Instructions are only allowed in .text section");
            }

            Instruction instruction = default;
            int instructionIndex = current->Instructions.Count;

            instruction.OpCode = ParseOpCode(ref line, end);

            if (line.Length > 0)
            {
                (instruction.Immediate, instruction.OperandSource1) = ParseOperand(ref line, ref instruction, true, instructionIndex, offset);
            }

            if (line.Length > 0)
            {
                (instruction.Immediate, instruction.OperandSource2) = ParseOperand(ref line, ref instruction, false, instructionIndex, offset);
            }

            offset += (uint)instruction.Size();

            current->Instructions.Add(instruction);
        }

        private bool TryParseSpecialInstruction(ref ReadOnlySpan<char> line, int end)
        {
            var part = line[..end];
            if (!Enum.TryParse(part, true, out SpecialInstruction instruction))
            {
                return false;
            }

            if (end != line.Length) end++;
            line = line[end..].TrimStart();

            end = line.IndexOf(' ');
            if (end == -1) end = line.Length;
            part = line[..end];

            switch (instruction)
            {
                case SpecialInstruction.ORG:
                    ulong baseAddress;
                    if (part.StartsWith("0x"))
                    {
                        if (!ulong.TryParse(part[2..], NumberStyles.HexNumber, CultureInfo.CurrentCulture, out baseAddress))
                        {
                            throw new InvalidOperationException("Invalid ORG value.");
                        }
                    }
                    else if (!ulong.TryParse(part, out baseAddress))
                    {
                        throw new InvalidOperationException("Invalid ORG value.");
                    }
                    current->BaseAddress = baseAddress;
                    break;

                case SpecialInstruction.DB:
                    ParseDataDirective(ref line, 1);
                    break;

                case SpecialInstruction.DW:
                    ParseDataDirective(ref line, 2);
                    break;

                case SpecialInstruction.DD:
                    ParseDataDirective(ref line, 4);
                    break;

                case SpecialInstruction.DQ:
                    ParseDataDirective(ref line, 8);
                    break;

                case SpecialInstruction.RESB:
                    ReserveSpace(ref line, 1);
                    break;

                case SpecialInstruction.RESW:
                    ReserveSpace(ref line, 2);
                    break;

                case SpecialInstruction.RESD:
                    ReserveSpace(ref line, 4);
                    break;

                case SpecialInstruction.RESQ:
                    ReserveSpace(ref line, 8);
                    break;

                case SpecialInstruction.DBS:
                    ParseStringDirective(ref line);
                    break;

                case SpecialInstruction.ALIGN:
                    AlignCurrentSection(ref line);
                    break;
            }

            return true;
        }

        private void AlignCurrentSection(ref ReadOnlySpan<char> line)
        {
            if (!int.TryParse(line.Trim(), CultureInfo.CurrentCulture, out int alignment) || alignment <= 0)
            {
                throw new InvalidOperationException("Invalid alignment value.");
            }

            int oldSize = current->Data.Count;
            int padding = (alignment - oldSize % alignment) % alignment;
            int newSize = current->Data.Count + padding;
            current->Data.Resize(newSize);
            Memset(current->Data.Data + oldSize, 0, padding);
        }

        private void ParseStringDirective(ref ReadOnlySpan<char> line)
        {
            if (!line.StartsWith("\"") || !line.EndsWith("\""))
            {
                throw new InvalidOperationException("String must be enclosed in double quotes.");
            }

            var str = line[1..^1];

            if (str.IsEmpty)
            {
                current->Data.Add(0);
                return;
            }

            fixed (char* pStr = str)
            {
                int byteCount = Encoding.UTF8.GetByteCount(pStr, str.Length);
                int size = current->Data.Count;
                current->Data.Resize(size + byteCount + 1);
                Encoding.UTF8.GetBytes(pStr, str.Length, current->Data.Data + size, byteCount);
                current->Data.Data[size + byteCount] = 0;
            }
        }

        private void ReserveSpace(ref ReadOnlySpan<char> line, int size)
        {
            if (!uint.TryParse(line, CultureInfo.CurrentCulture, out uint count))
            {
                throw new InvalidOperationException("Invalid value for RES directive.");
            }

            int additional = (int)count * size;
            int oldSize = current->Data.Count;
            int newSize = current->Data.Count + additional;
            current->Data.Resize(newSize);
            Memset(current->Data.Data + oldSize, 0, additional);
        }

        private void ParseDataDirective(ref ReadOnlySpan<char> line, int size)
        {
            while (!line.IsEmpty)
            {
                int idx = line.IndexOf(',');
                if (idx == -1) idx = line.Length;
                ReadOnlySpan<char> part = line[..idx].Trim();

                ulong value;
                if (part.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    if (!ulong.TryParse(part.Slice(2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out value))
                    {
                        throw new InvalidOperationException("Invalid hexadecimal value.");
                    }
                }
                else
                {
                    if (!ulong.TryParse(part, NumberStyles.Integer, CultureInfo.CurrentCulture, out value))
                    {
                        throw new InvalidOperationException("Invalid integer value.");
                    }
                }

                for (int i = 0; i < size; i++)
                {
                    current->Data.Add((byte)((value >> (8 * i)) & 0xFF));
                }

                if (idx < line.Length) idx++;
                line = line[idx..];
            }
        }

        private ReadOnlySpan<char> ParseSection(ReadOnlySpan<char> line)
        {
            line = line["section".Length..].Trim();

            SectionType sectionType = line switch
            {
                ".text" => SectionType.Text,
                ".data" => SectionType.Data,
                ".rodata" => SectionType.RoData,
                ".bss" => SectionType.Bss,
                _ => throw new InvalidOperationException("Invalid section.")
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

            return line;
        }

        private unsafe (ulong value, OperandSource flag) ParseOperand(ref ReadOnlySpan<char> line, ref Instruction instruction, bool firstOperator, int instructionIndex, ulong offset)
        {
            long signedValue;
            ulong value = 0;
            OperandSource flag = 0;

            var end = line.IndexOf(',');
            if (end == -1) end = line.Length;

            var part = line[..end];

            if (end != line.Length) end++;
            line = line[end..].TrimStart();

            RegisterAddress registerAddress;

            if (part.StartsWith('[') && part.EndsWith(']'))
            {
                part = part[1..];
                part = part[..(part.Length - 1)];

                if (!part.All(char.IsDigit) && Enum.TryParse(part, true, out registerAddress) && Enum.IsDefined(registerAddress))
                {
                    instruction.Flags |= firstOperator ? InstructionFlags.Source1AsAddress : InstructionFlags.Source2AsAddress;
                    return (0, Instruction.Convert(registerAddress));
                }

                if (part.StartsWith("0x"))
                {
                    value = ulong.Parse(part[2..], NumberStyles.HexNumber);
                    return (value, OperandSource.ImmAddress);
                }

                if (char.IsLetter(part[0])) // label ref
                {
                    var name = part.ToString();
                    references.Add((instructionIndex, offset, name));
                    return (value, OperandSource.ImmAddress);
                }
            }

            if (!part.All(char.IsDigit) && Enum.TryParse(part, true, out registerAddress) && Enum.IsDefined(registerAddress))
            {
                return (0, Instruction.Convert(registerAddress));
            }

            if (part.StartsWith("0x"))
            {
                value = ulong.Parse(part[2..], NumberStyles.HexNumber);
                flag = Classify(value);
                return (value, flag);
            }

            if (char.IsLetter(part[0])) // label ref
            {
                flag = OperandSource.Imm64;
                var name = part.ToString();
                references.Add((instructionIndex, offset, name));
            }
            else if (part.EndsWith("f"))
            {
            }
            else if (part.StartsWith('-'))
            {
                signedValue = long.Parse(part);
                value = *(ulong*)&signedValue;
                flag = Classify(signedValue);
            }
            else
            {
                value = ulong.Parse(part);
                flag = Classify(value);
            }

            return (value, flag);
        }

        private static unsafe OperandSource Classify(ulong value)
        {
            if (value <= byte.MaxValue)
            {
                return OperandSource.Imm8;
            }
            else if (value <= ushort.MaxValue)
            {
                return OperandSource.Imm16;
            }
            else if (value <= uint.MaxValue)
            {
                return OperandSource.Imm32;
            }
            else if (value <= ulong.MaxValue)
            {
                return OperandSource.Imm64;
            }

            return 0;
        }

        private static unsafe OperandSource Classify(long value)
        {
            if (value <= sbyte.MaxValue && value >= sbyte.MinValue)
            {
                return OperandSource.Imm8;
            }
            else if (value <= short.MaxValue && value >= short.MinValue)
            {
                return OperandSource.Imm16;
            }
            else if (value <= int.MaxValue && value >= int.MinValue)
            {
                return OperandSource.Imm32;
            }
            else if (value <= long.MaxValue && value >= long.MinValue)
            {
                return OperandSource.Imm64;
            }

            return 0;
        }

        private static OpCode ParseOpCode(ref ReadOnlySpan<char> line, int end)
        {
            if (!Enum.TryParse<OpCode>(line[..end], true, out var opCode))
            {
                // handle error.
                throw new ArgumentException("Invalid op code");
            }
            if (end != line.Length) end++;
            line = line[end..].TrimStart();
            return opCode;
        }
    }
}