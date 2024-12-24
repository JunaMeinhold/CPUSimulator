namespace CPUSimulator.Core
{
    using CPUSimulator.Core.Decoding;
    using CPUSimulator.Core.Memory;

    public struct MicrocodeBuilder
    {
        private ulong _value;

        public MicrocodeBuilder SetMC(int mc)
        {
            _value &= ~(MicrocodeFieldPositions.MC_MASK << MicrocodeFieldPositions.MC_SHIFT); // Clear current value
            _value |= ((ulong)mc & MicrocodeFieldPositions.MC_MASK) << MicrocodeFieldPositions.MC_SHIFT; // Set new value
            return this;
        }

        public MicrocodeBuilder SetNextAddress(int mcnext)
        {
            _value &= ~(MicrocodeFieldPositions.MCNEXT_MASK << MicrocodeFieldPositions.MCNEXT_SHIFT);
            _value |= ((ulong)mcnext & MicrocodeFieldPositions.MCNEXT_MASK) << MicrocodeFieldPositions.MCNEXT_SHIFT;
            return this;
        }

        public MicrocodeBuilder SetCC(bool cc)
        {
            _value &= ~(MicrocodeFieldPositions.CC_MASK << MicrocodeFieldPositions.CC_SHIFT);
            _value |= (cc ? 1UL : 0UL) << MicrocodeFieldPositions.CC_SHIFT;
            return this;
        }

        public MicrocodeBuilder SetALUFunction(ALUFunction alufc)
        {
            _value &= ~(MicrocodeFieldPositions.ALU_FC_MASK << MicrocodeFieldPositions.ALU_FC_SHIFT);
            _value |= ((ulong)alufc & MicrocodeFieldPositions.ALU_FC_MASK) << MicrocodeFieldPositions.ALU_FC_SHIFT;
            return this;
        }

        public MicrocodeBuilder SetXBus(ParseObject parseObject)
        {
            SetXBus(parseObject.RegisterToBinaryValue());
            return this;
        }

        public MicrocodeBuilder SetXBus(byte xbus)
        {
            _value &= ~(MicrocodeFieldPositions.X_BUS_MASK << MicrocodeFieldPositions.X_BUS_SHIFT);
            _value |= (xbus & MicrocodeFieldPositions.X_BUS_MASK) << MicrocodeFieldPositions.X_BUS_SHIFT;
            return this;
        }

        public MicrocodeBuilder SetYBus(ParseObject parseObject)
        {
            SetYBus(parseObject.RegisterToBinaryValue());
            return this;
        }

        public MicrocodeBuilder SetYBus(byte ybus)
        {
            _value &= ~(MicrocodeFieldPositions.Y_BUS_MASK << MicrocodeFieldPositions.Y_BUS_SHIFT);
            _value |= (ybus & MicrocodeFieldPositions.Y_BUS_MASK) << MicrocodeFieldPositions.Y_BUS_SHIFT;
            return this;
        }

        public MicrocodeBuilder SetZBus(ParseObject parseObject)
        {
            SetZBus(parseObject.RegisterToBinaryValue());
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

        public MicrocodeBuilder SetRAMBusWidth(ParseObject register)
        {
            SetRAMBusWidth(register.RegisterToByteWidth());
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
    }
}