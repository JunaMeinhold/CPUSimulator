namespace CPUSimulator.Core.Decoding
{
    using System;
    using System.Buffers.Binary;
    using System.Runtime.CompilerServices;

    public struct Instruction
    {
        public OpCode OpCode;
        public OperandSource OperandSource1;
        public OperandSource OperandSource2;
        public ulong Immediate;
        public InstructionFlags Flags;

        public readonly bool IsRegister1 => OperandSource1 >= OperandSource.RAX && OperandSource1 < OperandSource.RegisterCount && !IsRegisterAddress1;

        public readonly bool IsRegisterAddress1 => (Flags & InstructionFlags.Source1AsAddress) != 0;

        public readonly bool IsImm1 => OperandSource1 == OperandSource.Imm8 || OperandSource1 == OperandSource.Imm16 || OperandSource1 == OperandSource.Imm32 || OperandSource1 == OperandSource.Imm64;

        public readonly bool IsRegister2 => OperandSource2 >= OperandSource.RAX && OperandSource2 < OperandSource.RegisterCount && !IsRegisterAddress2;

        public readonly bool IsRegisterAddress2 => (Flags & InstructionFlags.Source2AsAddress) != 0;

        public readonly bool IsImm2 => OperandSource2 == OperandSource.Imm8 || OperandSource2 == OperandSource.Imm16 || OperandSource2 == OperandSource.Imm32 || OperandSource2 == OperandSource.Imm64;

        public readonly bool IsImm1Address => OperandSource1 == OperandSource.ImmAddress;

        public readonly bool IsImm2Address => OperandSource2 == OperandSource.ImmAddress;

        public readonly RegisterAddress RegisterName1 => Convert(OperandSource1);

        public readonly RegisterAddress RegisterName2 => Convert(OperandSource2);

        public static Instruction ReadFrom(ReadOnlySpan<byte> buffer)
        {
            Unsafe.SkipInit(out Instruction instruction);
            instruction.Read(buffer);
            return instruction;
        }

        public readonly int Size()
        {
            int size = 4;
            if (OperandSource1 >= OperandSource.Imm8 && OperandSource1 <= OperandSource.ImmAddress)
            {
                size += SizeOp(OperandSource1);
            }
            else if (OperandSource2 >= OperandSource.Imm8 && OperandSource2 <= OperandSource.ImmAddress)
            {
                size += SizeOp(OperandSource2);
            }

            return size;
        }

        public static int SizeOp(OperandSource flag)
        {
            return flag switch
            {
                OperandSource.Imm8 => 1,
                OperandSource.Imm16 => 2,
                OperandSource.Imm32 => 4,
                OperandSource.Imm64 or OperandSource.ImmAddress => 8,
                _ => 0,
            };
        }

        public readonly int Write(Span<byte> buffer)
        {
            buffer[0] = (byte)OpCode;
            buffer[1] = (byte)OperandSource1;
            buffer[2] = (byte)OperandSource2;
            buffer[3] = (byte)Flags;
            int idx = 4;

            if (OperandSource1 >= OperandSource.Imm8 && OperandSource1 <= OperandSource.ImmAddress)
            {
                idx += WriteOperator(buffer[idx..], OperandSource1, Immediate);
            }
            else if (OperandSource2 >= OperandSource.Imm8 && OperandSource2 <= OperandSource.ImmAddress)
            {
                idx += WriteOperator(buffer[idx..], OperandSource2, Immediate);
            }
            else if ((Flags & InstructionFlags.ImmIsOperandDestination) != 0)
            {
                idx += WriteOperator(buffer[idx..], OperandSource.Imm8, Immediate);
            }

            return idx;
        }

        private static int WriteOperator(Span<byte> buffer, OperandSource flag, ulong value)
        {
            int idx = 0;
            switch (flag)
            {
                case OperandSource.Imm8:
                    buffer[0] = (byte)value; idx++;
                    break;

                case OperandSource.Imm16:
                    BinaryPrimitives.WriteUInt16LittleEndian(buffer, (ushort)value); idx += 2;
                    break;

                case OperandSource.Imm32:
                    BinaryPrimitives.WriteUInt32LittleEndian(buffer, (uint)value); idx += 4;
                    break;

                case OperandSource.Imm64:
                case OperandSource.ImmAddress:
                    BinaryPrimitives.WriteUInt64LittleEndian(buffer, value); idx += 8;
                    break;
            }

            return idx;
        }

        public int Read(ReadOnlySpan<byte> buffer)
        {
            OpCode = (OpCode)buffer[0];
            OperandSource1 = (OperandSource)buffer[1];
            OperandSource2 = (OperandSource)buffer[2];
            Flags = (InstructionFlags)buffer[3];
            int idx = 4;

            if (OperandSource1 >= OperandSource.Imm8 && OperandSource1 <= OperandSource.ImmAddress)
            {
                idx += ReadOperand(buffer[idx..], OperandSource1, out Immediate);
            }
            else if (OperandSource2 >= OperandSource.Imm8 && OperandSource2 <= OperandSource.ImmAddress)
            {
                idx += ReadOperand(buffer[idx..], OperandSource2, out Immediate);
            }
            else if ((Flags & InstructionFlags.ImmIsOperandDestination) != 0)
            {
                idx += ReadOperand(buffer[idx..], OperandSource.Imm8, out Immediate);
            }

            return idx;
        }

        private static int ReadOperand(ReadOnlySpan<byte> buffer, OperandSource flag, out ulong value)
        {
            value = 0;
            int idx = 0;
            switch (flag)
            {
                case OperandSource.Imm8:
                    value = buffer[0]; idx++;
                    break;

                case OperandSource.Imm16:
                    value = BinaryPrimitives.ReadUInt16LittleEndian(buffer); idx += 2;
                    break;

                case OperandSource.Imm32:
                    value = BinaryPrimitives.ReadUInt32LittleEndian(buffer); idx += 4;
                    break;

                case OperandSource.Imm64:
                case OperandSource.ImmAddress:
                    value = BinaryPrimitives.ReadUInt64LittleEndian(buffer); idx += 8;
                    break;
            }

            return idx;
        }

        public static RegisterAddress Convert(OperandSource flag)
        {
            return (RegisterAddress)(flag - OperandSource.RAX + 1);
        }

        public static OperandSource Convert(RegisterAddress address)
        {
            return (OperandSource)((int)address - 1 + (int)OperandSource.RAX);
        }
    }
}