namespace CPUSimulator.Core.Decoding.Instructions
{
    using CPUSimulator.Core;

    public static class Halt
    {
        public static IEnumerable<Microcode> HALT()
        {
            MicrocodeBuilder builder = new();
            yield return builder.SetMC(ControlUnitFlag.Halt).Build();
        }
    }
}