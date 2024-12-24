namespace CPUSimulator.Core.Decoding.Instructions
{
    using CPUSimulator.Core;
    using CPUSimulator.Core.Decoding;
    using CPUSimulator.Core.Memory;

    public static class MoveData
    {
        public static void MOV(ParseBlock block)
        {
            MicrocodeBuilder builder = new MicrocodeBuilder().SetNextAddress(block.NextAddress);

            if (block.Param1.IsRegister && block.Param2.Type != null)
            {
                builder.SetALUFunction(ALUFunction.PassY).SetZBus(block.Param1);

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

            if (block.Param1.IsRegister && block.Param2.IsRegister)
            {
                block.Add(builder.SetALUFunction(ALUFunction.PassX).SetXBus(block.Param1).SetZBus(block.Param2).Build());
            }

            if (block.Param1.IsAddress && block.Param2.Type != null)
            {
                builder.SetALUFunction(ALUFunction.PassY);
                block.Add(builder.SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).Build(block.Param1.AddressValue ?? 0));

                builder.SetIORAM(RAMIOFlags.ZRegisterWriteToRamData).SetRAMMode(RAMMode.Write);

                switch (block.Param2.Type)
                {
                    case NumberType.Byte:
                        block.Add(builder.SetRAMBusWidth(RAMBusWidth.Bits8).Build(block.Param2.Number8 ?? 0));
                        break;

                    case NumberType.Int16:
                        block.Add(builder.SetRAMBusWidth(RAMBusWidth.Bits16).Build(block.Param2.Number16 ?? 0));
                        break;

                    case NumberType.Int32:
                        block.Add(builder.SetRAMBusWidth(RAMBusWidth.Bits32).Build(block.Param2.Number32 ?? 0));
                        break;

                    case NumberType.Int64:
                        block.Add(builder.SetRAMBusWidth(RAMBusWidth.Bits64).Build(block.Param2.Number64 ?? 0));
                        break;
                }
            }

            if (block.Param1.IsRegister && block.Param2.IsAddress)
            {
                block.Add(builder.SetALUFunction(ALUFunction.PassY).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).Build(block.Param1.AddressValue ?? 0));
                block.Add(builder.SetALUFunction(ALUFunction.PassX).SetIORAM(RAMIOFlags.ZRegisterWriteToRamData).SetXBus(block.Param1).SetRAMMode(RAMMode.Write).SetRAMBusWidth(block.Param1).Build());
            }

            if (block.Param1.IsAddress && block.Param2.IsRegister)
            {
                block.Add(builder.SetALUFunction(ALUFunction.PassY).SetIORAM(RAMIOFlags.ZRegisterWriteToRamAddress).Build(block.Param1.AddressValue ?? 0));
                block.Add(builder.SetALUFunction(ALUFunction.NoOperation).SetZBus(block.Param2).SetIORAM(RAMIOFlags.RamDataWriteToZRegister).SetRAMMode(RAMMode.Read).SetRAMBusWidth(block.Param2).Build());
            }
        }
    }
}