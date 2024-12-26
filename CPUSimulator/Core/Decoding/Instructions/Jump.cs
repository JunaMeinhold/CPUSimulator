namespace CPUSimulator.Core.Decoding.Instructions
{
    using CPUSimulator.Core;
    using CPUSimulator.Core.Decoding;
    using CPUSimulator.Core.Memory;

    public static class Jump
    {
        public static void JMP(ref DecodeBlock block, Instruction instruction, int next)
        {
            if (instruction.IsAddress1)
            {
                MicrocodeBuilder builder = new();
                block.Add(builder
                   .SetMC(0b011)
                   .SetIORAM(RAMIOFlags.ZRegisterWriteToRamData | RAMIOFlags.RamDataWriteToROM_MCOP)
                   .Build(instruction.Operand1));
            }
        }

        public static void JGE(ref DecodeBlock block, Instruction instruction, int next)
        {
            JE(ref block, instruction, next);
            JG(ref block, instruction, next);
        }

        public static void JLE(ref DecodeBlock block, Instruction instruction, int next)
        {
            JE(ref block, instruction, next);
            JL(ref block, instruction, next);
        }

        public static void JE(ref DecodeBlock block, Instruction instruction, int next)
        {
            if (instruction.IsAddress1)
            {
                MicrocodeBuilder builder = new();
                block.Add(builder
                    .SetMC(0b100)
                    .SetALUFunction(ALUFunction.PassY)
                    .SetIORAM(RAMIOFlags.ZRegisterWriteToRamData | RAMIOFlags.RamDataWriteToROM_MCOP)
                    .Build(instruction.Operand1));
            }
        }

        public static void JG(ref DecodeBlock block, Instruction instruction, int next)
        {
            if (instruction.IsAddress1)
            {
                MicrocodeBuilder builder = new();
                block.Add(builder
                    .SetMC(0b101)
                    .SetALUFunction(ALUFunction.PassY)
                    .SetIORAM(RAMIOFlags.ZRegisterWriteToRamData | RAMIOFlags.RamDataWriteToROM_MCOP)
                    .Build(instruction.Operand1));
            }
        }

        public static void JL(ref DecodeBlock block, Instruction instruction, int next)
        {
            if (instruction.IsAddress1)
            {
                MicrocodeBuilder builder = new();
                block.Add(builder
                    .SetMC(0b110)
                    .SetALUFunction(ALUFunction.PassY)
                    .SetIORAM(RAMIOFlags.ZRegisterWriteToRamData | RAMIOFlags.RamDataWriteToROM_MCOP)
                    .Build(instruction.Operand1));
            }
        }
    }
}