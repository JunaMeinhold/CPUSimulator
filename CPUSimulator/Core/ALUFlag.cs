namespace CPUSimulator.Core
{
    [Flags]
    public enum ALUFlag : ulong
    {
        NoneFlag = 0,
        CarryFlag = 2,
        SignFlag = 4,
        ZeroFlag = 8,
        OverflowFlag = 16,
        ParityFlag = 32,
        InterruptEnableFlag = 64,
        AdjustFlag = 128,
        BCDFlag = 256,
        ALUFlagsMask = CarryFlag | SignFlag | ZeroFlag | OverflowFlag | ParityFlag
    }
}