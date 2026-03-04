namespace CPUSimulator.Core
{
    public unsafe struct Microcode
    {
        public ulong Code;
        public ulong Intermediate;

        public Microcode(ulong code)
        {
            Code = code;
            Intermediate = 0;
        }

        public Microcode(ulong code, byte value)
        {
            Code = code;
            Intermediate = value;
        }

        public Microcode(ulong code, short value)
        {
            Code = code;
            Intermediate = *(ushort*)&value;
        }

        public Microcode(ulong code, int value)
        {
            Code = code;
            Intermediate = *(uint*)&value;
        }

        public Microcode(ulong code, long value)
        {
            Code = code;
            Intermediate = *(ulong*)&value;
        }

        public Microcode(ulong code, ulong value)
        {
            Code = code;
            Intermediate = value;
        }

        public ControlUnitFlag MC
        {
            get => (ControlUnitFlag)GetValue(MicrocodeFieldPositions.MC_SHIFT, MicrocodeFieldPositions.MC_MASK);
            set => SetValue((ulong)value, MicrocodeFieldPositions.MC_SHIFT, MicrocodeFieldPositions.MC_MASK);
        }

        public bool CC
        {
            get => GetValue(MicrocodeFieldPositions.CC_SHIFT, MicrocodeFieldPositions.CC_MASK) != 0;
            set => SetValue(value ? 1ul : 0ul, MicrocodeFieldPositions.CC_SHIFT, MicrocodeFieldPositions.CC_MASK);
        }

        public ALUMode ALUMode
        {
            get => (ALUMode)GetValue(MicrocodeFieldPositions.ALU_MODE_SHIFT, MicrocodeFieldPositions.ALU_MODE_MASK);
            set => SetValue((ulong)value, MicrocodeFieldPositions.ALU_MODE_SHIFT, MicrocodeFieldPositions.ALU_MODE_MASK);
        }

        public ALUFunction ALUFunction
        {
            get => (ALUFunction)GetValue(MicrocodeFieldPositions.ALU_FC_SHIFT, MicrocodeFieldPositions.ALU_FC_MASK);
            set => SetValue((ulong)value, MicrocodeFieldPositions.ALU_FC_SHIFT, MicrocodeFieldPositions.ALU_FC_MASK);
        }

        public ulong GetValue(int offset, ulong mask)
        {
            return Code >> offset & mask;
        }

        public void SetValue(ulong value, int shift, ulong mask)
        {
            Code &= ~(mask << shift);
            Code |= (value & mask) << shift;
        }
    }
}