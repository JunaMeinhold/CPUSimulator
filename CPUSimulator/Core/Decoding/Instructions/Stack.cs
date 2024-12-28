namespace CPUSimulator.Core.Decoding.Instructions
{
    using CPUSimulator.Core;
    using CPUSimulator.Core.Memory;

    public static class Stack
    {
        public static IEnumerable<Microcode> Push(Instruction instruction)
        {
            MicrocodeBuilder builder = new();
            if (instruction.IsImm1)
            {
                yield return builder.SetALUFunction(ALUFunction.Substraction).SetXBus(RegisterAddress.RSP).SetZBus(RegisterAddress.RSP).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).Build(8);
                yield return builder.SetMC(ControlUnitFlag.Step).SetALUFunction(ALUFunction.PassY).SetIORAM(RAMIOFlags.ZRegisterWriteToRamData).SetRAMMode(RAMMode.Write).SetRAMBusWidth(instruction.OperandSource1).Build(instruction.Immediate);
            }

            if (instruction.IsRegister1)
            {
                yield return builder.SetALUFunction(ALUFunction.Substraction).SetXBus(RegisterAddress.RSP).SetZBus(RegisterAddress.RSP).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).Build(8);
                yield return builder.SetMC(ControlUnitFlag.Step).SetALUFunction(ALUFunction.PassX).SetXBus(instruction.RegisterName1).SetIORAM(RAMIOFlags.ZRegisterWriteToRamData).SetRAMMode(RAMMode.Write).SetRAMBusWidth(instruction.RegisterName1).Build();
            }

            if (instruction.IsRegisterAddress1)
            {
                yield return builder.SetALUFunction(ALUFunction.PassX).SetXBus(instruction.RegisterName1).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).Build();
                yield return builder.SetALUFunction(ALUFunction.NoOperation).SetRAMMode(RAMMode.Read).SetRAMBusWidth(RAMBusWidth.Bits64).Build();
                yield return builder.SetALUFunction(ALUFunction.Substraction).SetXBus(RegisterAddress.RSP).SetZBus(RegisterAddress.RSP).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).Build(8);
                yield return builder.SetMC(ControlUnitFlag.Step).SetALUFunction(ALUFunction.NoOperation).SetRAMMode(RAMMode.Write).SetRAMBusWidth(RAMBusWidth.Bits64).Build();
            }
        }

        public static IEnumerable<Microcode> Pop(Instruction instruction)
        {
            MicrocodeBuilder builder = new();
            if (instruction.IsRegister1)
            {
                yield return builder.SetALUFunction(ALUFunction.PassX).SetXBus(RegisterAddress.RSP).SetZBus(RegisterAddress.RSP).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).Build();
                yield return builder.SetALUFunction(ALUFunction.NoOperation).SetIORAM(RAMIOFlags.RamDataWriteToZRegister).SetZBus(instruction.RegisterName1).SetRAMMode(RAMMode.Read).SetRAMBusWidth(RAMBusWidth.Bits64).Build();
                yield return builder.SetMC(ControlUnitFlag.Step).SetALUFunction(ALUFunction.Addition).SetXBus(RegisterAddress.RSP).SetZBus(RegisterAddress.RSP).Build(8);
            }
        }
    }

    public static class Halt
    {
        public static IEnumerable<Microcode> HALT()
        {
            MicrocodeBuilder builder = new();
            yield return builder.SetMC(ControlUnitFlag.Halt).Build();
        }
    }

    public static class Interrupts
    {
        public static IEnumerable<Microcode> ClearInterruptFlag()
        {
            MicrocodeBuilder builder = new();
            yield return builder.SetMC(ControlUnitFlag.InterruptClearFlag).Build();
        }

        public static IEnumerable<Microcode> SetInterruptFlag()
        {
            MicrocodeBuilder builder = new();
            yield return builder.SetMC(ControlUnitFlag.InterruptSetFlag).Build();
        }
    }
}