namespace CPUSimulator.Core.Decoding
{
    using CPUSimulator.Core;
    using System.Collections.Generic;

    public struct DecodeResult
    {
        public List<ParseBlock> Blocks;

        public Microcode[] Microcodes;

        public DecodeResult(List<ParseBlock> blocks, Microcode[] microcodes)
        {
            Blocks = blocks;
            Microcodes = microcodes;
        }

        public static implicit operator Microcode[](DecodeResult result) => result.Microcodes;
    }
}