namespace CPUSimulator.Core.Decoding
{
    public enum InstructionFlags : byte
    {
        None = 0,
        Source1AsAddress = 1,
        Source2AsAddress = 2,
        ImmIsOperandDestination = 4,
    }
}