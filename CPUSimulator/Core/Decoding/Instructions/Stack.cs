namespace CPUSimulator.Core.Decoding.Instructions
{
    using CPUSimulator.Core;
    using CPUSimulator.Core.Memory;

    public static class StackInstr
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
        public static bool Push(MicrocodeQueue queue, in Instruction instruction)
        {
            MicrocodeBuilder builder = new();
            if (instruction.IsImm1)
            {
                queue.Enqueue(builder.SetALUFunction(ALUFunction.Substraction).SetXBus(RegisterAddress.RSP).SetZBus(RegisterAddress.RSP).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).Build(instruction.OperandSource1.GetImmSize()));
                queue.Enqueue(builder.SetMC(ControlUnitFlag.Step).SetALUFunction(ALUFunction.PassY).SetIORAM(RAMIOFlags.ZRegisterWriteToRamData).SetRAMMode(RAMMode.Write).SetRAMBusWidth(instruction.OperandSource1).Build(instruction.Immediate1));
            }

            if (instruction.IsRegister1)
            {
                queue.Enqueue(builder.SetALUFunction(ALUFunction.Substraction).SetXBus(RegisterAddress.RSP).SetZBus(RegisterAddress.RSP).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).Build(RegisterHelper.GetRegisterSize(instruction.RegisterName1)));
                queue.Enqueue(builder.SetMC(ControlUnitFlag.Step).SetALUFunction(ALUFunction.PassX).SetXBus(instruction.RegisterName1).SetIORAM(RAMIOFlags.ZRegisterWriteToRamData).SetRAMMode(RAMMode.Write).SetRAMBusWidth(instruction.RegisterName1).Build());
            }

            if (instruction.IsRegisterAddress1)
            {
                var width = instruction.Width;
                queue.Enqueue(builder.SetALUFunction(ALUFunction.PassX).SetXBus(instruction.RegisterName1).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).SetRAMMode(RAMMode.Read).SetRAMBusWidth(width).Build());
                queue.Enqueue(builder.SetALUFunction(ALUFunction.Substraction).SetXBus(RegisterAddress.RSP).SetZBus(RegisterAddress.RSP).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).Build(width.GetRAMBusWidthSize()));
                queue.Enqueue(builder.SetMC(ControlUnitFlag.Step).SetALUFunction(ALUFunction.NoOperation).SetRAMMode(RAMMode.Write).SetRAMBusWidth(width).Build());
            }

            if (instruction.IsImm1Address)
            {
                var width = instruction.Width;
                queue.Enqueue(builder.SetALUFunction(ALUFunction.PassY).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).SetRAMMode(RAMMode.Read).SetRAMBusWidth(width).Build(instruction.Immediate1));
                queue.Enqueue(builder.SetALUFunction(ALUFunction.Substraction).SetXBus(RegisterAddress.RSP).SetZBus(RegisterAddress.RSP).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).Build(width.GetRAMBusWidthSize()));
                queue.Enqueue(builder.SetMC(ControlUnitFlag.Step).SetALUFunction(ALUFunction.NoOperation).SetRAMMode(RAMMode.Write).SetRAMBusWidth(width).Build());
            }
            return true;
        }

        public static bool Pop(MicrocodeQueue queue, in Instruction instruction)
        {
            MicrocodeBuilder builder = new();
            if (instruction.IsRegister1)
            {
                queue.Enqueue(builder.SetALUFunction(ALUFunction.PassX).SetXBus(RegisterAddress.RSP).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).SetRAMMode(RAMMode.Read).SetRAMBusWidth(instruction.RegisterName1).Build());
                queue.Enqueue(builder.SetALUFunction(ALUFunction.NoOperation).SetIORAM(RAMIOFlags.RamDataWriteToZRegister).SetZBus(instruction.RegisterName1).Build());
                queue.Enqueue(builder.SetMC(ControlUnitFlag.Step).SetALUFunction(ALUFunction.Addition).SetXBus(RegisterAddress.RSP).SetZBus(RegisterAddress.RSP).Build(RegisterHelper.GetRegisterSize(instruction.RegisterName1)));
            }

            if (instruction.IsImm1Address)
            {
                var width = instruction.Width;
                queue.Enqueue(builder.SetALUFunction(ALUFunction.PassX).SetXBus(RegisterAddress.RSP).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).SetRAMMode(RAMMode.Read).SetRAMBusWidth(width).Build());
                queue.Enqueue(builder.SetALUFunction(ALUFunction.PassY).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).SetRAMMode(RAMMode.Write).SetRAMBusWidth(width).Build(instruction.Immediate1));
                queue.Enqueue(builder.SetMC(ControlUnitFlag.Step).SetALUFunction(ALUFunction.Addition).SetXBus(RegisterAddress.RSP).SetZBus(RegisterAddress.RSP).Build(width.GetRAMBusWidthSize()));
            }
            return true;
        }
    }
}