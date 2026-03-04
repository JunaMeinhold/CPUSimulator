namespace CPUSimulator.Core.Decoding
{
    public enum InstructionFlags : byte
    {
        None = 0,
        Source1AsAddress = 1,
        Source2AsAddress = 2,
        Scale2 = 4,
        Scale4 = 8,
        Scale8 = Scale2 | Scale4,
        Index = 16,
        Displacement = 32,
        Width16 = 64,
        Width32 = 128,
        Width64 = Width16 | Width32
    }
}