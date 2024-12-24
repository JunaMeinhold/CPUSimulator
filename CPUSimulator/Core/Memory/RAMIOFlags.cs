namespace CPUSimulator.Core.Memory
{
    public enum RAMIOFlags
    {
        None = 0,
        ZRegisterWriteToRamAddress = 1,
        ZRegisterWriteToRamData = 2,

        RamAddressWriteToZRegister = 4,
        RamDataWriteToZRegister = 8,

        RamDataWriteToYRegister = 16,

        RamDataWriteToROM_MCOP = 32,
    }
}