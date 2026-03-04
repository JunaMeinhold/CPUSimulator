namespace CPUSimulator.Core.Decoding.Instructions
{
    using CPUSimulator.Core;
    using CPUSimulator.Core.Memory;

    public static class Stack
    {
        /*
         
        start:
            mov rsp, 16384 // Stack Base address
			mov rax, 10
			mov ebx, 20
			push rax
			push ebx
			mov rax, 0
			mov ebx, 0
			pop ebx
			pop rax
            hlt

         */
        public static IEnumerable<Microcode> Push(Instruction instruction)
        {
            MicrocodeBuilder builder = new();
            if (instruction.IsImm1)
            {
                yield return builder.SetALUFunction(ALUFunction.Substraction).SetXBus(RegisterAddress.RSP).SetZBus(RegisterAddress.RSP).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).Build(instruction.OperandSource1.GetImmSize());
                yield return builder.SetMC(ControlUnitFlag.Step).SetALUFunction(ALUFunction.PassY).SetIORAM(RAMIOFlags.ZRegisterWriteToRamData).SetRAMMode(RAMMode.Write).SetRAMBusWidth(instruction.OperandSource1).Build(instruction.Immediate1);
            }

            if (instruction.IsRegister1)
            {
                yield return builder.SetALUFunction(ALUFunction.Substraction).SetXBus(RegisterAddress.RSP).SetZBus(RegisterAddress.RSP).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).Build(RegisterHelper.GetRegisterSize(instruction.RegisterName1));
                yield return builder.SetMC(ControlUnitFlag.Step).SetALUFunction(ALUFunction.PassX).SetXBus(instruction.RegisterName1).SetIORAM(RAMIOFlags.ZRegisterWriteToRamData).SetRAMMode(RAMMode.Write).SetRAMBusWidth(instruction.RegisterName1).Build();
            }

            if (instruction.IsRegisterAddress1)
            {
                var width = instruction.Width;
                yield return builder.SetALUFunction(ALUFunction.PassX).SetXBus(instruction.RegisterName1).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).SetRAMMode(RAMMode.Read).SetRAMBusWidth(width).Build();
                yield return builder.SetALUFunction(ALUFunction.Substraction).SetXBus(RegisterAddress.RSP).SetZBus(RegisterAddress.RSP).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).Build(width.GetRAMBusWidthSize());
                yield return builder.SetMC(ControlUnitFlag.Step).SetALUFunction(ALUFunction.NoOperation).SetRAMMode(RAMMode.Write).SetRAMBusWidth(width).Build();
            }

            if (instruction.IsImm1Address)
            {
                var width = instruction.Width;
                yield return builder.SetALUFunction(ALUFunction.PassY).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).SetRAMMode(RAMMode.Read).SetRAMBusWidth(width).Build(instruction.Immediate1);
                yield return builder.SetALUFunction(ALUFunction.Substraction).SetXBus(RegisterAddress.RSP).SetZBus(RegisterAddress.RSP).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).Build(width.GetRAMBusWidthSize());
                yield return builder.SetMC(ControlUnitFlag.Step).SetALUFunction(ALUFunction.NoOperation).SetRAMMode(RAMMode.Write).SetRAMBusWidth(width).Build();
            }
        }

        public static IEnumerable<Microcode> Pop(Instruction instruction)
        {
            MicrocodeBuilder builder = new();
            if (instruction.IsRegister1)
            {
                yield return builder.SetALUFunction(ALUFunction.PassX).SetXBus(RegisterAddress.RSP).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).SetRAMMode(RAMMode.Read).SetRAMBusWidth(instruction.RegisterName1).Build();
                yield return builder.SetALUFunction(ALUFunction.NoOperation).SetIORAM(RAMIOFlags.RamDataWriteToZRegister).SetZBus(instruction.RegisterName1).Build();
                yield return builder.SetMC(ControlUnitFlag.Step).SetALUFunction(ALUFunction.Addition).SetXBus(RegisterAddress.RSP).SetZBus(RegisterAddress.RSP).Build(RegisterHelper.GetRegisterSize(instruction.RegisterName1));
            }

            if (instruction.IsImm1Address)
            {
                var width = instruction.Width;
                yield return builder.SetALUFunction(ALUFunction.PassX).SetXBus(RegisterAddress.RSP).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).SetRAMMode(RAMMode.Read).SetRAMBusWidth(width).Build();
                yield return builder.SetALUFunction(ALUFunction.PassY).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).SetRAMMode(RAMMode.Write).SetRAMBusWidth(width).Build(instruction.Immediate1);
                yield return builder.SetMC(ControlUnitFlag.Step).SetALUFunction(ALUFunction.Addition).SetXBus(RegisterAddress.RSP).SetZBus(RegisterAddress.RSP).Build(width.GetRAMBusWidthSize());
            }
        }
    }
}