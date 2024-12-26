namespace CPUSimulator.Core.Assembly
{
    using CPUSimulator.Core.Decoding;

    public static class Assembler
    {
        public static AssemblyResult Assemble(string code)
        {
            ReadOnlySpan<char> span = code;
            List<Instruction> instructions = [];

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
                Instruction instruction = Parse(line);
                instructions.Add(instruction);

                if (crlf) idx += 2;
                else idx++;

                if (idx >= span.Length) break;
                span = span[idx..];
            }

            return new AssemblyResult([.. instructions]);
        }

        private static bool All<T>(this ReadOnlySpan<T> span, Func<T, bool> cond)
        {
            foreach (T t in span)
            {
                if (!cond(t)) return false;
            }
            return true;
        }

        private static Instruction Parse(ReadOnlySpan<char> line)
        {
            Instruction instruction = default;
            line = line.Trim();
            instruction.OpCode = ParseOpCode(ref line);

            if (line.Length > 0)
            {
                (instruction.Operand1, instruction.OperandFlag1) = ParseOperand(ref line);
            }

            if (line.Length > 0)
            {
                (instruction.Operand2, instruction.OperandFlag2) = ParseOperand(ref line);
            }

            return instruction;
        }

        private static unsafe (ulong value, OperandFlag flag) ParseOperand(ref ReadOnlySpan<char> line)
        {
            long signedValue;
            ulong value = 0;
            OperandFlag flag = 0;

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
                if (Enum.TryParse(part, true, out registerAddress))
                {
                    return ((ulong)registerAddress, OperandFlag.RegisterAddress);
                }
            }

            if (!part.All(char.IsDigit) && Enum.TryParse(part, true, out registerAddress) && Enum.IsDefined(registerAddress))
            {
                return ((ulong)registerAddress, OperandFlag.Register);
            }

            if (part.StartsWith("0x"))
            {
                return (ulong.Parse(part[2..], System.Globalization.NumberStyles.HexNumber), OperandFlag.Address);
            }

            int typeSeparator = part.IndexOf(':');
            ReadOnlySpan<char> valuePart;
            if (typeSeparator == -1)
            {
                valuePart = part;
                if (valuePart.EndsWith("f"))
                {
                }
                else if (valuePart.StartsWith('-'))
                {
                    signedValue = long.Parse(valuePart);
                    if (signedValue <= sbyte.MaxValue && signedValue >= sbyte.MinValue)
                    {
                        value = *(ulong*)&signedValue;
                        flag = OperandFlag.Interm8;
                    }
                }
                else
                {
                    value = ulong.Parse(valuePart);
                    if (value <= byte.MaxValue)
                    {
                        flag = OperandFlag.Interm8;
                    }
                    else if (value <= ushort.MaxValue)
                    {
                        flag = OperandFlag.Interm16;
                    }
                    else if (value <= uint.MaxValue)
                    {
                        flag = OperandFlag.Interm32;
                    }
                    else if (value <= ulong.MaxValue)
                    {
                        flag = OperandFlag.Interm64;
                    }
                }
                return (value, flag);
            }

            valuePart = part[..typeSeparator].Trim();
            var type = part[(typeSeparator + 1)..].Trim();

            switch (type)
            {
                case "u8":
                    value = byte.Parse(valuePart);  // Unsigned 8-bit
                    flag = OperandFlag.Interm8;
                    break;

                case "i8":
                    signedValue = sbyte.Parse(valuePart);  // Signed 8-bit
                    value = *(ulong*)&signedValue;
                    flag = OperandFlag.Interm8;
                    break;

                case "u16":
                    value = ushort.Parse(valuePart);  // Unsigned 16-bit
                    flag = OperandFlag.Interm16;
                    break;

                case "i16":
                    signedValue = short.Parse(valuePart);  // Signed 16-bit
                    value = *(ulong*)&signedValue;
                    flag = OperandFlag.Interm16;
                    break;

                case "u32":
                    value = uint.Parse(valuePart);  // Unsigned 32-bit
                    flag = OperandFlag.Interm32;
                    break;

                case "i32":
                    signedValue = int.Parse(valuePart);  // Signed 32-bit
                    value = *(ulong*)&signedValue;
                    flag = OperandFlag.Interm32;
                    break;

                case "u64":
                    value = ulong.Parse(valuePart);  // Unsigned 64-bit
                    flag = OperandFlag.Interm64;
                    break;

                case "i64":
                    signedValue = long.Parse(valuePart);  // Signed 64-bit
                    value = *(ulong*)&signedValue;
                    flag = OperandFlag.Interm64;
                    break;

                case "f":
                    var f = float.Parse(valuePart);  // Floating-point (32-bit)
                    value = *(ulong*)&f;
                    flag = OperandFlag.Interm32;
                    break;

                case "d":
                    var d = double.Parse(valuePart);  // Double-precision floating-point (64-bit)
                    value = *(ulong*)&d;
                    flag = OperandFlag.Interm64;
                    break;
            }

            return (value, flag);
        }

        private static OpCode ParseOpCode(ref ReadOnlySpan<char> line)
        {
            var end = line.IndexOf(' ');
            if (end == -1) end = line.Length;
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