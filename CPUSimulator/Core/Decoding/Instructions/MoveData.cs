namespace CPUSimulator.Core.Decoding.Instructions
{
    using CPUSimulator.Core;
    using CPUSimulator.Core.Decoding;
    using CPUSimulator.Core.Memory;

    public static class MoveData
    {
        public static IEnumerable<Microcode> MOV(Instruction instruction)
        {
            MicrocodeBuilder builder = new();

            if (instruction.IsRegister1 && instruction.IsImm2)
            {
                yield return builder.SetMC(ControlUnitFlag.Step).SetALUFunction(ALUFunction.PassY).SetZBus(instruction.RegisterName1).Build(instruction.Immediate2);
            }

            if (instruction.IsRegister1 && instruction.IsRegister2)
            {
                yield return builder.SetMC(ControlUnitFlag.Step).SetALUFunction(ALUFunction.PassX).SetXBus(instruction.RegisterName2).SetZBus(instruction.RegisterName1).Build();
            }

            if (instruction.IsImm1 && instruction.IsImm2)
            {
                yield return builder.SetALUFunction(ALUFunction.PassY).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).Build(instruction.Immediate1);
                yield return builder.SetMC(ControlUnitFlag.Step).SetALUFunction(ALUFunction.PassY).SetIORAM(RAMIOFlags.ZRegisterWriteToRamData).SetRAMMode(RAMMode.Write).SetRAMBusWidth(instruction.OperandSource2).Build(instruction.Immediate2);
            }

            if (instruction.IsRegisterAddress1 && instruction.IsImm2)
            {
                yield return builder.SetALUFunction(ALUFunction.PassX).SetXBus(instruction.RegisterName1).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).Build();
                yield return builder.SetMC(ControlUnitFlag.Step).SetALUFunction(ALUFunction.PassY).SetIORAM(RAMIOFlags.ZRegisterWriteToRamData).SetRAMMode(RAMMode.Write).SetRAMBusWidth(instruction.OperandSource2).Build(instruction.Immediate2);
            }

            if (instruction.IsRegister1 && instruction.IsImm2Address)
            {
                yield return builder.SetALUFunction(ALUFunction.PassY).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).SetRAMMode(RAMMode.Read).SetRAMBusWidth(instruction.RegisterName1).Build(instruction.Immediate2);
                yield return builder.SetMC(ControlUnitFlag.Step).SetALUFunction(ALUFunction.NoOperation).SetIORAM(RAMIOFlags.RamDataWriteToZRegister).SetZBus(instruction.RegisterName1).Build();
            }

            if (instruction.IsImm1Address && instruction.IsRegister2)
            {
                yield return builder.SetALUFunction(ALUFunction.PassY).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).Build(instruction.Immediate1);
                yield return builder.SetMC(ControlUnitFlag.Step).SetALUFunction(ALUFunction.PassX).SetXBus(instruction.RegisterName2).SetIORAM(RAMIOFlags.ZRegisterWriteToRamData).SetRAMMode(RAMMode.Write).SetRAMBusWidth(instruction.RegisterName2).Build();
            }

            if (instruction.IsRegisterAddress1 && instruction.IsRegister2)
            {
                yield return builder.SetALUFunction(ALUFunction.PassX).SetXBus(instruction.RegisterName1).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).Build();
                yield return builder.SetMC(ControlUnitFlag.Step).SetALUFunction(ALUFunction.PassX).SetXBus(instruction.RegisterName2).SetIORAM(RAMIOFlags.ZRegisterWriteToRamData).SetRAMMode(RAMMode.Write).SetRAMBusWidth(instruction.RegisterName2).Build();
            }

            if (instruction.IsRegister1 && instruction.IsRegisterAddress2)
            {
                yield return builder.SetALUFunction(ALUFunction.PassX).SetXBus(instruction.RegisterName1).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).Build();
                yield return builder.SetMC(ControlUnitFlag.Step).SetALUFunction(ALUFunction.NoOperation).SetIORAM(RAMIOFlags.RamDataWriteToZRegister).SetZBus(instruction.RegisterName1).SetRAMMode(RAMMode.Read).SetRAMBusWidth(instruction.RegisterName1).Build();
            }

            if (instruction.IsImm1Address && instruction.IsImm2)
            {
                yield return builder.SetALUFunction(ALUFunction.PassY).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).Build(instruction.Immediate1);
                yield return builder.SetMC(ControlUnitFlag.Step).SetALUFunction(ALUFunction.PassY).SetIORAM(RAMIOFlags.ZRegisterWriteToRamData).SetRAMMode(RAMMode.Write).SetRAMBusWidth(instruction.Width).Build(instruction.Immediate2);
            }
        }
    }
}