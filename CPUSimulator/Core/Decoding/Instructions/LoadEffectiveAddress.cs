namespace CPUSimulator.Core.Decoding.Instructions
{
    using System;
    using System.Collections.Generic;

    public static class LoadEffectiveAddress
    {
        public static bool LEA(MicrocodeQueue queue, in Instruction instruction)
        {
            MicrocodeBuilder builder = new();
            queue.Enqueue(builder
                .SetMC(ControlUnitFlag.Step)
                .SetALUFunction(ALUFunction.PassY)
                .SetZBus(instruction.RegisterName2)
                .Build(instruction.Immediate1));
            return true;
        }
    }
}