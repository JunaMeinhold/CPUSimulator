namespace CPUSimulator.Core.Decoding.Instructions
{
    using System;
    using System.Collections.Generic;

    public static class LoadEffectiveAddress
    {
        public static IEnumerable<Microcode> LEA(Instruction instruction)
        {
            MicrocodeBuilder builder = new();
            yield return builder
                .SetMC(ControlUnitFlag.Step)
                .SetALUFunction(ALUFunction.PassY)
                .SetZBus(instruction.RegisterName2)
                .Build(instruction.Immediate);
        }
    }
}