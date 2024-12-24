namespace CPUSimulator.Core.Decoding.Instructions
{
    using CPUSimulator.Core;
    using CPUSimulator.Core.Decoding;
    using CPUSimulator.Core.Memory;

    public static class Jump
    {
        public static void JMP(ParseBlock block)
        {
            if (block.Param1.IsAddress)
            {
                MicrocodeBuilder builder = new();
                block.Add(builder
                   .SetMC(0b011)
                   .SetIORAM(RAMIOFlags.ZRegisterWriteToRamData | RAMIOFlags.RamDataWriteToROM_MCOP)
                   .Build(block.Param1.AddressValue ?? 0));
            }
        }

        public static void JGE(ParseBlock block)
        {
            JE(block);
            JG(block);
        }

        public static void JLE(ParseBlock block)
        {
            JE(block);
            JL(block);
        }

        public static void JE(ParseBlock block)
        {
            if (block.Param1.IsAddress)
            {
                MicrocodeBuilder builder = new();
                block.Add(builder
                    .SetMC(0b100)
                    .SetALUFunction(ALUFunction.PassY)
                    .SetIORAM(RAMIOFlags.ZRegisterWriteToRamData | RAMIOFlags.RamDataWriteToROM_MCOP)
                    .Build(block.Param1.AddressValue ?? 0));
            }
        }

        public static void JG(ParseBlock block)
        {
            if (block.Param1.IsAddress)
            {
                MicrocodeBuilder builder = new();
                block.Add(builder
                    .SetMC(0b101)
                    .SetALUFunction(ALUFunction.PassY)
                    .SetIORAM(RAMIOFlags.ZRegisterWriteToRamData | RAMIOFlags.RamDataWriteToROM_MCOP)
                    .Build(block.Param1.AddressValue ?? 0));
            }
        }

        public static void JL(ParseBlock block)
        {
            if (block.Param1.IsAddress)
            {
                MicrocodeBuilder builder = new();
                block.Add(builder
                    .SetMC(0b110)
                    .SetALUFunction(ALUFunction.PassY)
                    .SetIORAM(RAMIOFlags.ZRegisterWriteToRamData | RAMIOFlags.RamDataWriteToROM_MCOP)
                    .Build(block.Param1.AddressValue ?? 0));
            }
        }
    }
}