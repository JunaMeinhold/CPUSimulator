namespace CPUSimulator.Core.Decoding.Instructions
{
    using CPUSimulator.Core;
    using CPUSimulator.Core.Decoding;
    using CPUSimulator.Core.Memory;

    public static class Jump
    {
        public static bool JMP(MicrocodeQueue queue, in Instruction instruction)
        {
            if (instruction.IsImm1)
            {
                MicrocodeBuilder builder = new();
                queue.Enqueue(builder
                      .SetMC(ControlUnitFlag.Jump)
                      .SetALUFunction(ALUFunction.PassY)
                      .SetIORAM(RAMIOFlags.ZRegisterWriteToRamData | RAMIOFlags.RamDataWriteToROM_MCOP)
                      .Build(instruction.Immediate1));
                return true;
            }

            if (instruction.IsRegisterAddress1)
            {
                MicrocodeBuilder builder = new();
                queue.Enqueue(builder
                   .SetMC(ControlUnitFlag.Jump)
                   .SetALUFunction(ALUFunction.PassX)
                   .SetXBus(instruction.RegisterName1)
                   .SetIORAM(RAMIOFlags.ZRegisterWriteToRamData | RAMIOFlags.RamDataWriteToROM_MCOP)
                   .Build());
                return true;
            }

            return false;
        }

        public static bool JGE(MicrocodeQueue queue, in Instruction instruction)
        {
            bool result = true;
            result &= JE(queue, instruction);
            result &= JG(queue, instruction);
            return result;
        }

        public static bool JLE(MicrocodeQueue queue, in Instruction instruction)
        {
            bool result = true;
            result &= JE(queue, instruction);
            result &= JL(queue, instruction);
            return result;
        }

        public static bool JE(MicrocodeQueue queue, Instruction instruction)
        {
            if (instruction.IsImm1)
            {
                MicrocodeBuilder builder = new();
                queue.Enqueue(builder
                    .SetMC(ControlUnitFlag.Equals)
                    .SetALUFunction(ALUFunction.PassY)
                    .SetIORAM(RAMIOFlags.ZRegisterWriteToRamData | RAMIOFlags.RamDataWriteToROM_MCOP)
                    .Build(instruction.Immediate1));
                return true;
            }

            return false;
        }

        public static bool JG(MicrocodeQueue queue, Instruction instruction)
        {
            if (instruction.IsImm1)
            {
                MicrocodeBuilder builder = new();
                queue.Enqueue(builder
                    .SetMC(ControlUnitFlag.Greater)
                    .SetALUFunction(ALUFunction.PassY)
                    .SetIORAM(RAMIOFlags.ZRegisterWriteToRamData | RAMIOFlags.RamDataWriteToROM_MCOP)
                    .Build(instruction.Immediate1));
                return true;
            }

            return false;
        }

        public static bool JL(MicrocodeQueue queue, Instruction instruction)
        {
            if (instruction.IsImm1)
            {
                MicrocodeBuilder builder = new();
                queue.Enqueue(builder
                    .SetMC(ControlUnitFlag.Less)
                    .SetALUFunction(ALUFunction.PassY)
                    .SetIORAM(RAMIOFlags.ZRegisterWriteToRamData | RAMIOFlags.RamDataWriteToROM_MCOP)
                    .Build(instruction.Immediate1));
                return true;
            }

            return false;
        }

        public static bool JNE(MicrocodeQueue queue, Instruction instruction)
        {
            if (instruction.IsImm1)
            {
                MicrocodeBuilder builder = new();
                queue.Enqueue(builder
                    .SetMC(ControlUnitFlag.NotEquals)
                    .SetALUFunction(ALUFunction.PassY)
                    .SetIORAM(RAMIOFlags.ZRegisterWriteToRamData | RAMIOFlags.RamDataWriteToROM_MCOP)
                    .Build(instruction.Immediate1));
                return true;
            }

            return false;
        }
    }
}