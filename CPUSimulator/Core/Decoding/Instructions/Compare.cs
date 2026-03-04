namespace CPUSimulator.Core.Decoding.Instructions
{
    using CPUSimulator.Core;
    using CPUSimulator.Core.Decoding;

    public static class Compare
    {
        public static IEnumerable<Microcode> CMP(Instruction instruction)
        {
            MicrocodeBuilder builder = new();

            if (instruction.IsRegister1 && instruction.IsImm2)
            {
                yield return builder.SetMC(ControlUnitFlag.Step).SetCC(true).SetALUFunction(ALUFunction.Compare).SetXBus(instruction.RegisterName1).Build(instruction.Immediate2);
            }

            if (instruction.IsRegister1 && instruction.IsRegister2)
            {
                yield return builder.SetMC(ControlUnitFlag.Step).SetCC(true).SetALUFunction(ALUFunction.Compare).SetXBus(instruction.RegisterName1).SetYBus(instruction.RegisterName2).Build();
            }
        }
    }
}