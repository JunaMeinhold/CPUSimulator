namespace CPUSimulator.Core.Decoding
{
    public enum InstructionFlags : byte
    {
        None = 0,
        Source1AsAddress = 1,
        Source2AsAddress = 2,
        ImmIsOperandDestination = 4,
        Scale2 = 8,
        Scale4 = 16,
        Scale8 = Scale2 | Scale4,
        Index = 32,
        Displacement = 64,
    }
}