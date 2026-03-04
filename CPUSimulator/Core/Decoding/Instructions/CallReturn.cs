namespace CPUSimulator.Core.Decoding.Instructions
{
    using CPUSimulator.Core;
    using CPUSimulator.Core.Decoding;
    using CPUSimulator.Core.Memory;

    public static class CallReturn
    {
        public static IEnumerable<Microcode> Call(Instruction instruction)
        {
            MicrocodeBuilder builder = new();

            if (instruction.IsImm1)
            {
                yield return builder.SetMC(ControlUnitFlag.Call).SetALUFunction(ALUFunction.PassY).SetIORAM(RAMIOFlags.ZRegisterWriteToRamData | RAMIOFlags.RamDataWriteToROM_MCOP).Build(instruction.Immediate1);
            }
            if (instruction.IsRegisterAddress1)
            {
                yield return builder.SetMC(ControlUnitFlag.Call).SetALUFunction(ALUFunction.PassX).SetXBus(instruction.RegisterName1).SetIORAM(RAMIOFlags.ZRegisterWriteToRamData | RAMIOFlags.RamDataWriteToROM_MCOP).Build();
            }
        }

        public static IEnumerable<Microcode> Return()
        {
            MicrocodeBuilder builder = new();
            yield return builder.SetMC(ControlUnitFlag.Return).Build();
        }
    }
}