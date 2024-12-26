namespace CPUSimulator.Core.Decoding
{
    public enum OperandFlag : byte
    {
        Register = 0,
        RegisterAddress = 1,
        Address = 2,
        Interm8 = 3,
        Interm16 = 4,
        Interm32 = 5,
        Interm64 = 6,
    }
}