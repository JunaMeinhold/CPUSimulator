namespace CPUSimulator.Core.Decoding.Instructions
{
    using CPUSimulator.Core;

    public static class Interrupts
    {
        public static bool ClearInterruptFlag(MicrocodeQueue queue)
        {
            MicrocodeBuilder builder = new();
            queue.Enqueue(builder.SetMC(ControlUnitFlag.InterruptClearFlag).Build());
            return true;
        }

        public static bool SetInterruptFlag(MicrocodeQueue queue)
        {
            MicrocodeBuilder builder = new();
            queue.Enqueue(builder.SetMC(ControlUnitFlag.InterruptSetFlag).Build());
            return true;
        }
    }
}