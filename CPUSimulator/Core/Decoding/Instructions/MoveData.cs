namespace CPUSimulator.Core.Decoding.Instructions
{
    using CPUSimulator.Core;
    using CPUSimulator.Core.Decoding;
    using CPUSimulator.Core.Memory;

    public static class MoveData
    {
        public static void MOV(ref DecodeBlock block, Instruction instruction, int next)
        {
            MicrocodeBuilder builder = new MicrocodeBuilder().SetNextAddress(next);

            if (instruction.IsRegister1 && instruction.IsInterm2)
            {
                block.Add(builder.SetALUFunction(ALUFunction.PassY).SetZBus(instruction.RegisterName1).Build(instruction.Operand2));
            }

            if (instruction.IsRegister1 && instruction.IsRegister2)
            {
                block.Add(builder.SetALUFunction(ALUFunction.PassX).SetXBus(instruction.RegisterName1).SetZBus(instruction.RegisterName2).Build());
            }

            if (instruction.IsAddress1 && instruction.IsInterm2)
            {
                block.Add(builder.SetMC(MemoryControlFlag.Step).SetNextAddress(0).SetALUFunction(ALUFunction.PassY).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).Build(instruction.Operand1));
                block.Add(builder.SetMC(0).SetNextAddress(next).SetALUFunction(ALUFunction.PassY).SetIORAM(RAMIOFlags.ZRegisterWriteToRamData).SetRAMMode(RAMMode.Write).SetRAMBusWidth(instruction.OperandFlag2).Build(instruction.Operand2));
            }

            if (instruction.IsRegisterAddress1 && instruction.IsInterm2)
            {
                block.Add(builder.SetMC(MemoryControlFlag.Step).SetNextAddress(0).SetALUFunction(ALUFunction.PassX).SetXBus(instruction.RegisterName1).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).Build());
                block.Add(builder.SetMC(0).SetNextAddress(next).SetALUFunction(ALUFunction.PassY).SetIORAM(RAMIOFlags.ZRegisterWriteToRamData).SetRAMMode(RAMMode.Write).SetRAMBusWidth(instruction.OperandFlag2).Build(instruction.Operand2));
            }

            if (instruction.IsRegister1 && instruction.IsAddress2)
            {
                block.Add(builder.SetMC(MemoryControlFlag.Step).SetNextAddress(0).SetALUFunction(ALUFunction.PassY).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).Build(instruction.Operand2));
                block.Add(builder.SetMC(0).SetNextAddress(next).SetALUFunction(ALUFunction.NoOperation).SetIORAM(RAMIOFlags.RamDataWriteToZRegister).SetZBus(instruction.RegisterName1).SetRAMMode(RAMMode.Read).SetRAMBusWidth(instruction.RegisterName1).Build());
            }

            if (instruction.IsAddress1 && instruction.IsRegister2)
            {
                block.Add(builder.SetMC(MemoryControlFlag.Step).SetNextAddress(0).SetALUFunction(ALUFunction.PassY).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).Build(instruction.Operand1));
                block.Add(builder.SetMC(0).SetNextAddress(next).SetALUFunction(ALUFunction.PassX).SetXBus(instruction.RegisterName2).SetIORAM(RAMIOFlags.ZRegisterWriteToRamData).SetRAMMode(RAMMode.Write).SetRAMBusWidth(instruction.RegisterName2).Build());
            }

            if (instruction.IsRegisterAddress1 && instruction.IsRegister2)
            {
                block.Add(builder.SetMC(MemoryControlFlag.Step).SetNextAddress(0).SetALUFunction(ALUFunction.PassX).SetXBus(instruction.RegisterName1).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).Build());
                block.Add(builder.SetMC(0).SetNextAddress(next).SetALUFunction(ALUFunction.PassX).SetXBus(instruction.RegisterName2).SetIORAM(RAMIOFlags.ZRegisterWriteToRamData).SetRAMMode(RAMMode.Write).SetRAMBusWidth(instruction.RegisterName2).Build());
            }

            if (instruction.IsRegister1 && instruction.IsRegisterAddress2)
            {
                block.Add(builder.SetMC(MemoryControlFlag.Step).SetNextAddress(0).SetALUFunction(ALUFunction.PassX).SetXBus(instruction.RegisterName1).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).Build());
                block.Add(builder.SetMC(0).SetNextAddress(next).SetALUFunction(ALUFunction.NoOperation).SetIORAM(RAMIOFlags.RamDataWriteToZRegister).SetZBus(instruction.RegisterName1).SetRAMMode(RAMMode.Read).SetRAMBusWidth(instruction.RegisterName1).Build());
            }
        }
    }
}