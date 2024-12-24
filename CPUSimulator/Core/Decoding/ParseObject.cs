namespace CPUSimulator.Core.Decoding
{
    using CPUSimulator.Core;

    public struct ParseObject
    {
        public ParseObject(string value)
        {
            Source = value;
            Number64 = null;
            Number32 = null;
            Number16 = null;
            Number8 = null;
            Type = NumberType.None;
            Instruction = null;
            AddressValue = null;
            Register = null!;
            if (Enum.TryParse<OpCode>(Source, false, out var instruction))
            {
                Instruction = instruction;
                return;
            }
            if (Source.StartsWith("0x"))
            {
                AddressValue = Convert.ToInt32(Source, 16);
                return;
            }
            if (Source.StartsWith("R") & Source.EndsWith("X") & Source.Length == 3)
            {
                Register = Source;
                return;
            }
            if (Source.StartsWith("byte:"))
            {
                if (byte.TryParse(Source.Replace("byte:", ""), out var num))
                {
                    Type = NumberType.Byte;
                    Number8 = num;
                    return;
                }
            }
            if (Source.StartsWith("short:"))
            {
                if (short.TryParse(Source.Replace("short:", ""), out var num))
                {
                    Type = NumberType.Int16;
                    Number16 = num;
                    return;
                }
            }
            if (Source.StartsWith("int:"))
            {
                if (int.TryParse(Source.Replace("int:", ""), out var num))
                {
                    Type = NumberType.Int32;
                    Number32 = num;
                    return;
                }
            }
            if (Source.StartsWith("long:"))
            {
                if (long.TryParse(Source.Replace("long:", ""), out var num))
                {
                    Type = NumberType.Int64;
                    Number64 = num;
                    return;
                }
            }
        }

        public string Source;

        public string Register;

        public long? Number64;

        public int? Number32;

        public short? Number16;

        public byte? Number8;

        public NumberType? Type;

        public int? AddressValue;

        public OpCode? Instruction;

        public readonly bool IsInstruction => Instruction is not null;

        public readonly bool IsAddress => AddressValue is not null;

        public readonly bool IsRegister => Register is not null;

        public readonly string RegisterToBinary()
        {
            return Register switch
            {
                nameof(Processor.RAX) => "10000000",
                nameof(Processor.RBX) => "01000000",
                nameof(Processor.RCX) => "00100000",
                nameof(Processor.RDX) => "00010000",
                nameof(Processor.REX) => "00001000",
                nameof(Processor.RFX) => "00000100",
                nameof(Processor.RGX) => "00000010",
                nameof(Processor.RHX) => "00000001",
                _ => "00000000",
            };
        }

        public readonly byte RegisterToBinaryValue()
        {
            return Register switch
            {
                nameof(Processor.RAX) => 0b_00000001,
                nameof(Processor.RBX) => 0b_00000010,
                nameof(Processor.RCX) => 0b_00000100,
                nameof(Processor.RDX) => 0b_00001000,
                nameof(Processor.REX) => 0b_00010000,
                nameof(Processor.RFX) => 0b_00100000,
                nameof(Processor.RGX) => 0b_01000000,
                nameof(Processor.RHX) => 0b_10000000,
                _ => 0b_00000000,
            };
        }

        public readonly int RegisterToByteWidth()
        {
            return Register switch
            {
                nameof(Processor.RAX) => 8,
                nameof(Processor.RBX) => 8,
                nameof(Processor.RCX) => 4,
                nameof(Processor.RDX) => 4,
                nameof(Processor.REX) => 2,
                nameof(Processor.RFX) => 2,
                nameof(Processor.RGX) => 1,
                nameof(Processor.RHX) => 1,
                _ => 0,
            };
        }
    }
}