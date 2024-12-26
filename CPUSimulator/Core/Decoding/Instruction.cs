namespace CPUSimulator.Core.Decoding
{
    using System.Buffers.Binary;
    using System.Runtime.CompilerServices;

    public struct Instruction
    {
        public OpCode OpCode;
        public OperandFlag OperandFlag1;
        public ulong Operand1;
        public OperandFlag OperandFlag2;
        public ulong Operand2;

        public readonly bool IsRegister1 => OperandFlag1 == OperandFlag.Register;

        public readonly bool IsRegisterAddress1 => OperandFlag1 == OperandFlag.RegisterAddress;

        public readonly bool IsAddress1 => OperandFlag1 == OperandFlag.Address;

        public readonly bool IsInterm1 => OperandFlag1 == OperandFlag.Interm8 || OperandFlag1 == OperandFlag.Interm16 || OperandFlag1 == OperandFlag.Interm32 || OperandFlag1 == OperandFlag.Interm64;

        public readonly bool IsRegister2 => OperandFlag2 == OperandFlag.Register;

        public readonly bool IsRegisterAddress2 => OperandFlag2 == OperandFlag.RegisterAddress;

        public readonly bool IsAddress2 => OperandFlag2 == OperandFlag.Address;

        public readonly bool IsInterm2 => OperandFlag2 == OperandFlag.Interm8 || OperandFlag2 == OperandFlag.Interm16 || OperandFlag2 == OperandFlag.Interm32 || OperandFlag2 == OperandFlag.Interm64;

        public readonly RegisterAddress RegisterName1 => (RegisterAddress)Operand1;

        public readonly RegisterAddress RegisterName2 => (RegisterAddress)Operand2;

        public static Instruction ReadFrom(ReadOnlySpan<byte> buffer)
        {
            Unsafe.SkipInit(out Instruction instruction);
            instruction.Read(buffer);
            return instruction;
        }

        public readonly int Write(Span<byte> buffer)
        {
            int start = buffer.Length;
            BinaryPrimitives.WriteUInt16LittleEndian(buffer, (ushort)OpCode);
            buffer[2] = (byte)OperandFlag1;
            buffer = buffer[3..];
            Write(ref buffer, OperandFlag1, Operand1);
            buffer[0] = (byte)OperandFlag2;
            buffer = buffer[1..];
            Write(ref buffer, OperandFlag2, Operand2);
            return buffer.Length - start;
        }

        private static void Write(ref Span<byte> buffer, OperandFlag flag, ulong value)
        {
            int idx = 0;
            switch (flag)
            {
                case OperandFlag.Interm8:
                case OperandFlag.Register:
                    buffer[0] = (byte)value; idx++;
                    break;

                case OperandFlag.Interm16:
                    BinaryPrimitives.WriteUInt16LittleEndian(buffer, (ushort)value); idx += 2;
                    break;

                case OperandFlag.Interm32:
                    BinaryPrimitives.WriteUInt32LittleEndian(buffer, (uint)value); idx += 4;
                    break;

                case OperandFlag.Address:
                case OperandFlag.Interm64:
                    BinaryPrimitives.WriteUInt64LittleEndian(buffer, value); idx += 8;
                    break;
            }

            buffer = buffer[idx..];
        }

        public int Read(ReadOnlySpan<byte> buffer)
        {
            int start = buffer.Length;
            OpCode = (OpCode)BinaryPrimitives.ReadInt16LittleEndian(buffer);
            OperandFlag1 = (OperandFlag)buffer[2];
            buffer = buffer[3..];
            Operand1 = Read(ref buffer, OperandFlag1);
            OperandFlag2 = (OperandFlag)buffer[0];
            buffer = buffer[1..];
            Operand2 = Read(ref buffer, OperandFlag2);
            return buffer.Length - start;
        }

        private static ulong Read(ref ReadOnlySpan<byte> buffer, OperandFlag flag)
        {
            ulong value = 0;
            int idx = 0;
            switch (flag)
            {
                case OperandFlag.Interm8:
                case OperandFlag.Register:
                    value = buffer[0]; idx++;
                    break;

                case OperandFlag.Interm16:
                    value = BinaryPrimitives.ReadUInt16LittleEndian(buffer); idx += 2;
                    break;

                case OperandFlag.Interm32:
                    value = BinaryPrimitives.ReadUInt32LittleEndian(buffer); idx += 4;
                    break;

                case OperandFlag.Address:
                case OperandFlag.Interm64:
                    value = BinaryPrimitives.ReadUInt64LittleEndian(buffer); idx += 8;
                    break;
            }

            buffer = buffer[idx..];

            return value;
        }
    }
}