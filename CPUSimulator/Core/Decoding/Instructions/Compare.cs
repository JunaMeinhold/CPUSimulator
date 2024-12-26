namespace CPUSimulator.Core.Decoding.Instructions
{
    using CPUSimulator.Core;
    using CPUSimulator.Core.Decoding;

    public static class Compare
    {
        public static void CMP(ref DecodeBlock block, Instruction instruction, int next)
        {
            MicrocodeBuilder builder = new MicrocodeBuilder().SetNextAddress(next).SetCC(true).SetALUFunction(ALUFunction.Compare);

            if (instruction.IsRegister1 && instruction.IsInterm2)
            {
                builder.SetXBus(instruction.RegisterName1);
                block.Add(builder.Build(instruction.Operand2));
            }

            if (instruction.IsRegister1 && instruction.IsRegister2)
            {
                block.Add(builder
                    .SetXBus(instruction.RegisterName1)
                    .SetYBus(instruction.RegisterName2)
                    .Build());
            }
        }
    }
}