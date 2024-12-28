namespace CPUSimulator.Core
{
    using CPUSimulator.Core.Decoding;
    using CPUSimulator.Core.Memory;

    public struct MicrocodeBuilder
    {
        private ulong _value;

        public MicrocodeBuilder SetMC(ControlUnitFlag flag)
        {
            SetMC((int)flag);
            return this;
        }

        public MicrocodeBuilder SetMC(int mc)
        {
            _value &= ~(MicrocodeFieldPositions.MC_MASK << MicrocodeFieldPositions.MC_SHIFT); // Clear current value
            _value |= ((ulong)mc & MicrocodeFieldPositions.MC_MASK) << MicrocodeFieldPositions.MC_SHIFT; // Set new value
            return this;
        }

        public MicrocodeBuilder SetCC(bool cc)
        {
            _value &= ~(MicrocodeFieldPositions.CC_MASK << MicrocodeFieldPositions.CC_SHIFT);
            _value |= (cc ? 1UL : 0UL) << MicrocodeFieldPositions.CC_SHIFT;
            return this;
        }

        public MicrocodeBuilder SetALUMode(ALUMode mode)
        {
            _value &= ~(MicrocodeFieldPositions.ALU_MODE_MASK << MicrocodeFieldPositions.ALU_MODE_SHIFT);
            _value |= ((ulong)mode & MicrocodeFieldPositions.ALU_MODE_MASK) << MicrocodeFieldPositions.ALU_MODE_SHIFT;
            return this;
        }

        public MicrocodeBuilder SetALUFunction(ALUFunction alufc)
        {
            _value &= ~(MicrocodeFieldPositions.ALU_FC_MASK << MicrocodeFieldPositions.ALU_FC_SHIFT);
            _value |= ((ulong)alufc & MicrocodeFieldPositions.ALU_FC_MASK) << MicrocodeFieldPositions.ALU_FC_SHIFT;
            return this;
        }

        public static byte RegisterToBinaryValue(RegisterAddress registerName)
        {
            return (byte)registerName;
        }

        public static RAMBusWidth RegisterToByteWidth(RegisterAddress registerName)
        {
            return RegisterHelper.GetRegisterSize(registerName) switch
            {
                8 => RAMBusWidth.Bits64,
                4 => RAMBusWidth.Bits32,
                2 => RAMBusWidth.Bits16,
                1 => RAMBusWidth.Bits8,
                _ => 0,
            };
        }

        public MicrocodeBuilder SetXBus(RegisterAddress registerName)
        {
            SetXBus(RegisterToBinaryValue(registerName));
            return this;
        }

        public MicrocodeBuilder SetXBus(byte xbus)
        {
            _value &= ~(MicrocodeFieldPositions.X_BUS_MASK << MicrocodeFieldPositions.X_BUS_SHIFT);
            _value |= (xbus & MicrocodeFieldPositions.X_BUS_MASK) << MicrocodeFieldPositions.X_BUS_SHIFT;
            return this;
        }

        public MicrocodeBuilder SetYBus(RegisterAddress registerName)
        {
            SetYBus(RegisterToBinaryValue(registerName));
            return this;
        }

        public MicrocodeBuilder SetYBus(byte ybus)
        {
            _value &= ~(MicrocodeFieldPositions.Y_BUS_MASK << MicrocodeFieldPositions.Y_BUS_SHIFT);
            _value |= (ybus & MicrocodeFieldPositions.Y_BUS_MASK) << MicrocodeFieldPositions.Y_BUS_SHIFT;
            return this;
        }

        public MicrocodeBuilder SetZBus(RegisterAddress registerName)
        {
            SetZBus(RegisterToBinaryValue(registerName));
            return this;
        }

        public MicrocodeBuilder SetZBus(byte zbus)
        {
            _value &= ~(MicrocodeFieldPositions.Z_BUS_MASK << MicrocodeFieldPositions.Z_BUS_SHIFT);
            _value |= (zbus & MicrocodeFieldPositions.Z_BUS_MASK) << MicrocodeFieldPositions.Z_BUS_SHIFT;
            return this;
        }

        public MicrocodeBuilder SetIORAM(RAMIOFlags flags)
        {
            SetIORAM((byte)flags);
            return this;
        }

        public MicrocodeBuilder SetIORAM(byte ioram)
        {
            _value &= ~(MicrocodeFieldPositions.IO_RAM_MASK << MicrocodeFieldPositions.IO_RAM_SHIFT);
            _value |= (ioram & MicrocodeFieldPositions.IO_RAM_MASK) << MicrocodeFieldPositions.IO_RAM_SHIFT;
            return this;
        }

        public MicrocodeBuilder SetRAMMode(RAMMode mode)
        {
            int mod = mode switch
            {
                RAMMode.Read => 1,
                RAMMode.Write => 2,
                _ => 0
            };
            SetRAMMode(mod);
            return this;
        }

        public MicrocodeBuilder SetRAMMode(int mode)
        {
            _value &= ~(MicrocodeFieldPositions.RAM_MODE_MASK << MicrocodeFieldPositions.RAM_MODE_SHIFT);
            _value |= ((ulong)mode & MicrocodeFieldPositions.RAM_MODE_MASK) << MicrocodeFieldPositions.RAM_MODE_SHIFT;
            return this;
        }

        public MicrocodeBuilder SetRAMBusWidth(RegisterAddress register)
        {
            SetRAMBusWidth(RegisterToByteWidth(register));
            return this;
        }

        public MicrocodeBuilder SetRAMBusWidth(OperandSource OperandSource)
        {
            switch (OperandSource)
            {
                case OperandSource.Imm8:
                    SetRAMBusWidth(RAMBusWidth.Bits8);
                    break;

                case OperandSource.Imm16:
                    SetRAMBusWidth(RAMBusWidth.Bits16);
                    break;

                case OperandSource.Imm32:
                    SetRAMBusWidth(RAMBusWidth.Bits32);
                    break;

                case OperandSource.Imm64:
                    SetRAMBusWidth(RAMBusWidth.Bits64);
                    break;
            }
            return this;
        }

        public MicrocodeBuilder SetRAMBusWidth(RAMBusWidth width)
        {
            SetRAMBusWidth((int)width);
            return this;
        }

        public MicrocodeBuilder SetRAMBusWidth(int width)
        {
            _value &= ~(MicrocodeFieldPositions.RAM_BUS_WIDTH_MASK << MicrocodeFieldPositions.RAM_BUS_WIDTH_SHIFT);
            _value |= ((ulong)width & MicrocodeFieldPositions.RAM_BUS_WIDTH_MASK) << MicrocodeFieldPositions.RAM_BUS_WIDTH_SHIFT;
            return this;
        }

        public readonly Microcode Build()
        {
            return new Microcode(_value);
        }

        public readonly Microcode Build(byte constant)
        {
            return new Microcode(_value, constant);
        }

        public readonly Microcode Build(short constant)
        {
            return new Microcode(_value, constant);
        }

        public readonly Microcode Build(int constant)
        {
            return new Microcode(_value, constant);
        }

        public readonly Microcode Build(long constant)
        {
            return new Microcode(_value, constant);
        }

        public readonly Microcode Build(ulong constant)
        {
            return new Microcode(_value, constant);
        }
    }
}