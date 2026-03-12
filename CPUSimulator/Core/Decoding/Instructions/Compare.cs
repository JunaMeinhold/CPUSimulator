namespace CPUSimulator.Core.Decoding.Instructions
{
    using CPUSimulator.Core;
    using CPUSimulator.Core.Decoding;
    using CPUSimulator.Core.Memory;

    public static class Compare
    {
        public static IEnumerable<Microcode> CMP(Instruction instruction)
        {
            MicrocodeBuilder builder = new();

            if (instruction.IsRegister1 && instruction.IsImm2)
            {
                yield return builder.SetMC(ControlUnitFlag.Step).SetCC(true).SetALUFunction(ALUFunction.Compare).SetXBus(instruction.RegisterName1).Build(instruction.Immediate2);
            }
            else if (instruction.IsRegister1 && instruction.IsRegister2)
            {
                yield return builder.SetMC(ControlUnitFlag.Step).SetCC(true).SetALUFunction(ALUFunction.Compare).SetXBus(instruction.RegisterName1).SetYBus(instruction.RegisterName2).Build();
            }
        }

        public static IEnumerable<Microcode> TEST(Instruction instruction)
        {
            MicrocodeBuilder builder = new();
            if (instruction.IsRegister1 && instruction.IsRegister2)
            {
                yield return builder.SetMC(ControlUnitFlag.Step).SetCC(true).SetALUFunction(ALUFunction.And).SetXBus(instruction.RegisterName1).SetYBus(instruction.RegisterName1).Build();
            }
            else if (instruction.IsRegister1 && instruction.IsImm2)
            {
                yield return builder.SetMC(ControlUnitFlag.Step).SetCC(true).SetALUFunction(ALUFunction.And).SetXBus(instruction.RegisterName1).Build(instruction.Immediate2);
            }
            else if (instruction.IsImm1Address && instruction.IsRegister2)
            {
                yield return builder.SetALUFunction(ALUFunction.PassY).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).SetRAMBusWidth(instruction.Width).SetRAMMode(RAMMode.Read).Build(instruction.Immediate1);
                yield return builder.SetMC(ControlUnitFlag.Step).SetCC(true).SetALUFunction(ALUFunction.And).SetIORAM(RAMIOFlags.RamDataWriteToYRegister).SetXBus(instruction.RegisterName2).Build();
            }
            else if (instruction.IsImm1Address && instruction.IsImm2)
            {
                yield return builder.SetALUFunction(ALUFunction.PassY).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).SetRAMBusWidth(instruction.Width).SetRAMMode(RAMMode.Read).Build(instruction.Immediate1);
                yield return builder.SetMC(ControlUnitFlag.Step).SetCC(true).SetALUFunction(ALUFunction.And).SetIORAM(RAMIOFlags.RamDataWriteToYRegister).Build(instruction.Immediate2);
            }
        }
    }
}