namespace CPUSimulator.Core.Decoding.Instructions
{
    using CPUSimulator.Core;
    using CPUSimulator.Core.Decoding;
    using CPUSimulator.Core.Memory;

    public static class Jump
    {
        public static IEnumerable<Microcode> JMP(Instruction instruction)
        {
            if (instruction.IsImm1)
            {
                MicrocodeBuilder builder = new();
                yield return builder
                      .SetMC(ControlUnitFlag.Jump)
                      .SetALUFunction(ALUFunction.PassY)
                      .SetIORAM(RAMIOFlags.ZRegisterWriteToRamData | RAMIOFlags.RamDataWriteToROM_MCOP)
                      .Build(instruction.Immediate);
            }
            if (instruction.IsRegisterAddress1)
            {
                MicrocodeBuilder builder = new();
                yield return (builder
                   .SetMC(ControlUnitFlag.Jump)
                   .SetALUFunction(ALUFunction.PassX)
                   .SetXBus(instruction.RegisterName1)
                   .SetIORAM(RAMIOFlags.ZRegisterWriteToRamData | RAMIOFlags.RamDataWriteToROM_MCOP)
                   .Build());
            }
        }

        public static IEnumerable<Microcode> JGE(Instruction instruction)
        {
            yield return JE(instruction).First();
            yield return JG(instruction).First();
        }

        public static IEnumerable<Microcode> JLE(Instruction instruction)
        {
            yield return JE(instruction).First();
            yield return JL(instruction).First();
        }

        public static IEnumerable<Microcode> JE(Instruction instruction)
        {
            if (instruction.IsImm1)
            {
                MicrocodeBuilder builder = new();
                yield return builder
                    .SetMC(ControlUnitFlag.Equals)
                    .SetALUFunction(ALUFunction.PassY)
                    .SetIORAM(RAMIOFlags.ZRegisterWriteToRamData | RAMIOFlags.RamDataWriteToROM_MCOP)
                    .Build(instruction.Immediate);
            }
        }

        public static IEnumerable<Microcode> JG(Instruction instruction)
        {
            if (instruction.IsImm1)
            {
                MicrocodeBuilder builder = new();
                yield return (builder
                    .SetMC(ControlUnitFlag.Greater)
                    .SetALUFunction(ALUFunction.PassY)
                    .SetIORAM(RAMIOFlags.ZRegisterWriteToRamData | RAMIOFlags.RamDataWriteToROM_MCOP)
                    .Build(instruction.Immediate));
            }
        }

        public static IEnumerable<Microcode> JL(Instruction instruction)
        {
            if (instruction.IsImm1)
            {
                MicrocodeBuilder builder = new();
                yield return (builder
                    .SetMC(ControlUnitFlag.Less)
                    .SetALUFunction(ALUFunction.PassY)
                    .SetIORAM(RAMIOFlags.ZRegisterWriteToRamData | RAMIOFlags.RamDataWriteToROM_MCOP)
                    .Build(instruction.Immediate));
            }
        }

        public static IEnumerable<Microcode> JNE(Instruction instruction)
        {
            if (instruction.IsImm1)
            {
                MicrocodeBuilder builder = new();
                yield return (builder
                    .SetMC(ControlUnitFlag.NotEquals)
                    .SetALUFunction(ALUFunction.PassY)
                    .SetIORAM(RAMIOFlags.ZRegisterWriteToRamData | RAMIOFlags.RamDataWriteToROM_MCOP)
                    .Build(instruction.Immediate));
            }
        }
    }
}