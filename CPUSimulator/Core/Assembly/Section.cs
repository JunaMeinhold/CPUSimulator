namespace CPUSimulator.Core.Assembly
{
    using CPUSimulator.Core.Decoding;
    using Hexa.NET.Utilities;

    public struct Section
    {
        public SectionType Type;
        public ulong BaseAddress;
        public UnsafeList<byte> Data;
        public UnsafeList<Instruction> Instructions;
    }
}