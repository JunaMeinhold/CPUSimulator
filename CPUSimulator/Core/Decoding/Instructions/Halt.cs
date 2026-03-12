namespace CPUSimulator.Core.Decoding.Instructions
{
    using CPUSimulator.Core;

    public static class Halt
    {
        public static bool HALT(MicrocodeQueue queue)
        {
            MicrocodeBuilder builder = new();
            queue.Enqueue(builder.SetMC(ControlUnitFlag.Halt).Build());
            return true;
        }
    }
}