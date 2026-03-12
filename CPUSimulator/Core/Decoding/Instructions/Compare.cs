namespace CPUSimulator.Core.Decoding.Instructions
{
    using CPUSimulator.Core;
    using CPUSimulator.Core.Decoding;
    using CPUSimulator.Core.Memory;

    public static class Compare
    {
        public static bool CMP(MicrocodeQueue queue, in Instruction instruction)
        {
            MicrocodeBuilder builder = new();

            if (instruction.IsRegister1 && instruction.IsImm2)
            {
                queue.Enqueue(builder.SetMC(ControlUnitFlag.Step).SetCC(true).SetALUFunction(ALUFunction.Compare).SetXBus(instruction.RegisterName1).Build(instruction.Immediate2));
                return true;
            }
            else if (instruction.IsRegister1 && instruction.IsRegister2)
            {
                queue.Enqueue(builder.SetMC(ControlUnitFlag.Step).SetCC(true).SetALUFunction(ALUFunction.Compare).SetXBus(instruction.RegisterName1).SetYBus(instruction.RegisterName2).Build());
                return true;
            }

            return false;
        }

        public static bool TEST(MicrocodeQueue queue, in Instruction instruction)
        {
            MicrocodeBuilder builder = new();
            if (instruction.IsRegister1 && instruction.IsRegister2)
            {
                queue.Enqueue(builder.SetMC(ControlUnitFlag.Step).SetCC(true).SetALUFunction(ALUFunction.And).SetXBus(instruction.RegisterName1).SetYBus(instruction.RegisterName1).Build());
                return true;
            }
            else if (instruction.IsRegister1 && instruction.IsImm2)
            {
                queue.Enqueue(builder.SetMC(ControlUnitFlag.Step).SetCC(true).SetALUFunction(ALUFunction.And).SetXBus(instruction.RegisterName1).Build(instruction.Immediate2));
                return true;
            }
            else if (instruction.IsImm1Address && instruction.IsRegister2)
            {
                queue.Enqueue(builder.SetALUFunction(ALUFunction.PassY).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).SetRAMBusWidth(instruction.Width).SetRAMMode(RAMMode.Read).Build(instruction.Immediate1));
                queue.Enqueue(builder.SetMC(ControlUnitFlag.Step).SetCC(true).SetALUFunction(ALUFunction.And).SetIORAM(RAMIOFlags.RamDataWriteToYRegister).SetXBus(instruction.RegisterName2).Build());
                return true;
            }
            else if (instruction.IsImm1Address && instruction.IsImm2)
            {
                queue.Enqueue(builder.SetALUFunction(ALUFunction.PassY).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).SetRAMBusWidth(instruction.Width).SetRAMMode(RAMMode.Read).Build(instruction.Immediate1));
                queue.Enqueue(builder.SetMC(ControlUnitFlag.Step).SetCC(true).SetALUFunction(ALUFunction.And).SetIORAM(RAMIOFlags.RamDataWriteToYRegister).Build(instruction.Immediate2));
                return true;
            }

            return false;
        }
    }
}