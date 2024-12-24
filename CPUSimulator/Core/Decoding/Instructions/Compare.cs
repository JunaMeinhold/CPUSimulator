namespace CPUSimulator.Core.Decoding.Instructions
{
    using CPUSimulator.Core;
    using CPUSimulator.Core.Decoding;

    public static class Compare
    {
        public static void CMP(ParseBlock block)
        {
            MicrocodeBuilder builder = new MicrocodeBuilder().SetNextAddress(block.NextAddress).SetCC(true).SetALUFunction(ALUFunction.Compare);

            if (block.Param1.IsRegister & block.Param2.Type != null)
            {
                builder.SetXBus(block.Param1);

                switch (block.Param2.Type)
                {
                    case NumberType.Byte:
                        block.Add(builder.Build(block.Param2.Number8 ?? 0));
                        break;

                    case NumberType.Int16:
                        block.Add(builder.Build(block.Param2.Number16 ?? 0));
                        break;

                    case NumberType.Int32:
                        block.Add(builder.Build(block.Param2.Number32 ?? 0));
                        break;

                    case NumberType.Int64:
                        block.Add(builder.Build(block.Param2.Number64 ?? 0));
                        break;
                }
            }

            if (block.Param1.IsRegister & block.Param2.IsRegister)
            {
                block.Add(builder
                    .SetXBus(block.Param1)
                    .SetYBus(block.Param2)
                    .Build());
            }
        }
    }
}