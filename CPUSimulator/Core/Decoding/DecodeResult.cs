namespace CPUSimulator.Core.Decoding
{
    using CPUSimulator.Core;

    public struct DecodeResult
    {
        public Microcode[] Microcodes;

        public DecodeResult(Microcode[] microcodes)
        {
            Microcodes = microcodes;
        }

        public static implicit operator Microcode[](DecodeResult result) => result.Microcodes;
    }
}