namespace CPUSimulator.Core
{
    [Flags]
    public enum ALUFlag : int
    {
        NoneFlag = 0,
        CarryFlag = 2,// Greater 0
        SignFlag = 4, // Less 0
        ZeroFlag = 8, // Equals 0
        OverflowFlag = 16,
        ParityFlag = 32,
        InterruptEnableFlag = 64,
        AdjustFlag = 128,
        BCDFlag = 256,
    }
}