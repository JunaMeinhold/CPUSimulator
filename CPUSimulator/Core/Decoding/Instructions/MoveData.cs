namespace CPUSimulator.Core.Decoding.Instructions
{
    using CPUSimulator.Core;
    using CPUSimulator.Core.Decoding;
    using CPUSimulator.Core.Memory;

    public static class MoveData
    {
        public static bool MOV(MicrocodeQueue queue, in Instruction instruction)
        {
            MicrocodeBuilder builder = new();

            if (instruction.IsRegister1 && instruction.IsImm2)
            {
                queue.Enqueue(builder.SetMC(ControlUnitFlag.Step).SetALUFunction(ALUFunction.PassY).SetZBus(instruction.RegisterName1).Build(instruction.Immediate2));
                return true;
            }

            if (instruction.IsRegister1 && instruction.IsRegister2)
            {
                queue.Enqueue(builder.SetMC(ControlUnitFlag.Step).SetALUFunction(ALUFunction.PassX).SetXBus(instruction.RegisterName2).SetZBus(instruction.RegisterName1).Build());
                return true;
            }

            if (instruction.IsImm1 && instruction.IsImm2)
            {
                queue.Enqueue(builder.SetALUFunction(ALUFunction.PassY).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).Build(instruction.Immediate1));
                queue.Enqueue(builder.SetMC(ControlUnitFlag.Step).SetALUFunction(ALUFunction.PassY).SetIORAM(RAMIOFlags.ZRegisterWriteToRamData).SetRAMMode(RAMMode.Write).SetRAMBusWidth(instruction.OperandSource2).Build(instruction.Immediate2));
                return true;
            }

            if (instruction.IsRegisterAddress1 && instruction.IsImm2)
            {
                queue.Enqueue(builder.SetALUFunction(ALUFunction.PassX).SetXBus(instruction.RegisterName1).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).Build());
                queue.Enqueue(builder.SetMC(ControlUnitFlag.Step).SetALUFunction(ALUFunction.PassY).SetIORAM(RAMIOFlags.ZRegisterWriteToRamData).SetRAMMode(RAMMode.Write).SetRAMBusWidth(instruction.OperandSource2).Build(instruction.Immediate2));
                return true;
            }

            if (instruction.IsRegister1 && instruction.IsImm2Address)
            {
                queue.Enqueue(builder.SetALUFunction(ALUFunction.PassY).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).SetRAMMode(RAMMode.Read).SetRAMBusWidth(instruction.RegisterName1).Build(instruction.Immediate2));
                queue.Enqueue(builder.SetMC(ControlUnitFlag.Step).SetALUFunction(ALUFunction.NoOperation).SetIORAM(RAMIOFlags.RamDataWriteToZRegister).SetZBus(instruction.RegisterName1).Build());
                return true;
            }

            if (instruction.IsImm1Address && instruction.IsRegister2)
            {
                queue.Enqueue(builder.SetALUFunction(ALUFunction.PassY).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).Build(instruction.Immediate1));
                queue.Enqueue(builder.SetMC(ControlUnitFlag.Step).SetALUFunction(ALUFunction.PassX).SetXBus(instruction.RegisterName2).SetIORAM(RAMIOFlags.ZRegisterWriteToRamData).SetRAMMode(RAMMode.Write).SetRAMBusWidth(instruction.RegisterName2).Build());
                return true;
            }

            if (instruction.IsRegisterAddress1 && instruction.IsRegister2)
            {
                queue.Enqueue(builder.SetALUFunction(ALUFunction.PassX).SetXBus(instruction.RegisterName1).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).Build());
                queue.Enqueue(builder.SetMC(ControlUnitFlag.Step).SetALUFunction(ALUFunction.PassX).SetXBus(instruction.RegisterName2).SetIORAM(RAMIOFlags.ZRegisterWriteToRamData).SetRAMMode(RAMMode.Write).SetRAMBusWidth(instruction.RegisterName2).Build());
                return true;
            }

            if (instruction.IsRegister1 && instruction.IsRegisterAddress2)
            {
                queue.Enqueue(builder.SetALUFunction(ALUFunction.PassX).SetXBus(instruction.RegisterName1).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).Build());
                queue.Enqueue(builder.SetMC(ControlUnitFlag.Step).SetALUFunction(ALUFunction.NoOperation).SetIORAM(RAMIOFlags.RamDataWriteToZRegister).SetZBus(instruction.RegisterName1).SetRAMMode(RAMMode.Read).SetRAMBusWidth(instruction.RegisterName1).Build());
                return true;
            }

            if (instruction.IsImm1Address && instruction.IsImm2)
            {
                queue.Enqueue(builder.SetALUFunction(ALUFunction.PassY).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).Build(instruction.Immediate1));
                queue.Enqueue(builder.SetMC(ControlUnitFlag.Step).SetALUFunction(ALUFunction.PassY).SetIORAM(RAMIOFlags.ZRegisterWriteToRamData).SetRAMMode(RAMMode.Write).SetRAMBusWidth(instruction.Width).Build(instruction.Immediate2));
                return true;
            }

            return false;
        }
    }
}