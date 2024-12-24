namespace CPUSimulator.Core.Decoding.Instructions
{
    using CPUSimulator.Core;
    using CPUSimulator.Core.Decoding;

    public static class Increment
    {
        public static void INC(ParseBlock block)
        {
            if (block.Param1.IsRegister)
            {
                MicrocodeBuilder builder = new();
                block.Add(builder
                    .SetNextAddress(block.NextAddress)
                    .SetALUFunction(ALUFunction.Increment)
                    .SetXBus(block.Param1)
                    .SetZBus(block.Param1)
                    .Build());
            }
        }
    }
}