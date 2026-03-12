namespace CPUSimulator.Core.Decoding.Instructions
{
    using CPUSimulator.Core;
    using CPUSimulator.Core.Decoding;
    using CPUSimulator.Core.Memory;

    public static class CallReturn
    {
        public static bool Call(MicrocodeQueue queue, in Instruction instruction)
        {
            MicrocodeBuilder builder = new();

            if (instruction.IsImm1)
            {
                queue.Enqueue(builder.SetMC(ControlUnitFlag.Call).SetALUFunction(ALUFunction.PassY).SetIORAM(RAMIOFlags.ZRegisterWriteToRamData | RAMIOFlags.RamDataWriteToROM_MCOP).Build(instruction.Immediate1));
                return true;
            }

            if (instruction.IsRegisterAddress1)
            {
                queue.Enqueue(builder.SetMC(ControlUnitFlag.Call).SetALUFunction(ALUFunction.PassX).SetXBus(instruction.RegisterName1).SetIORAM(RAMIOFlags.ZRegisterWriteToRamData | RAMIOFlags.RamDataWriteToROM_MCOP).Build());
                return true;
            }

            return false;
        }

        public static bool Return(MicrocodeQueue queue)
        {
            MicrocodeBuilder builder = new();
            queue.Enqueue(builder.SetMC(ControlUnitFlag.Return).Build());
            return true;
        }
    }
}