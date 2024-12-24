namespace CPUSimulator.Core.Decoding.Instructions
{
    using CPUSimulator.Core;
    using CPUSimulator.Core.Decoding;

    public static class Add
    {
        public static void ADD(ParseBlock block)
        {
            MicrocodeBuilder builder = new MicrocodeBuilder().SetNextAddress(block.NextAddress).SetALUFunction(ALUFunction.Addition);

            if (block.Param1.IsRegister & block.Param2.Type is not null)
            {
                builder.SetXBus(block.Param1).SetZBus(block.Param1);

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
                   .SetZBus(block.Param1)
                   .Build());
            }
        }
    }
}