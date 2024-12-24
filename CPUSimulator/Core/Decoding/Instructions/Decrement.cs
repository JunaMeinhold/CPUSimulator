namespace CPUSimulator.Core.Decoding.Instructions
{
    using CPUSimulator.Core;
    using CPUSimulator.Core.Decoding;

    public static class Decrement
    {
        public static void DEC(ParseBlock block)
        {
            if (block.Param1.IsRegister)
            {
                MicrocodeBuilder builder = new();
                block.Add(builder
                    .SetNextAddress(block.NextAddress)
                    .SetALUFunction(ALUFunction.Decrement)
                    .SetXBus(block.Param1)
                    .SetZBus(block.Param1)
                    .Build());
            }
        }
    }
}