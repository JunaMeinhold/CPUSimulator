namespace CPUSimulator.Core.Decoding
{
    using CPUSimulator.Core;
    using System.Runtime.CompilerServices;

    public ref struct DecodeBlock
    {
        public byte Index;
        public MicrocodeBlock Block;

        [InlineArray(4)]
        public struct MicrocodeBlock
        {
            public Microcode Microcode;
        }

        public void Add(Microcode microcode)
        {
            Block[Index++] = microcode;
        }
    }
}