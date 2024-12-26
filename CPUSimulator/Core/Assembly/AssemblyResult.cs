namespace CPUSimulator.Core.Assembly
{
    using CPUSimulator.Core.Decoding;

    public struct AssemblyResult
    {
        public Instruction[] Instructions;

        public AssemblyResult(Instruction[] instructions)
        {
            Instructions = instructions;
        }
    }
}