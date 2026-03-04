namespace CPUSimulator.Core.Decoding.Instructions
{
    using CPUSimulator.Core;

    public static class Interrupts
    {
        public static IEnumerable<Microcode> ClearInterruptFlag()
        {
            MicrocodeBuilder builder = new();
            yield return builder.SetMC(ControlUnitFlag.InterruptClearFlag).Build();
        }

        public static IEnumerable<Microcode> SetInterruptFlag()
        {
            MicrocodeBuilder builder = new();
            yield return builder.SetMC(ControlUnitFlag.InterruptSetFlag).Build();
        }
    }
}