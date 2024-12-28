namespace CPUSimulator.Core
{
    public class InterruptController
    {
        private readonly Queue<int> interruptQueue = new();

        public void TriggerInterrupt(int interrupt)
        {
            interruptQueue.Enqueue(interrupt);
        }

        public bool Take(out int id)
        {
            return interruptQueue.TryDequeue(out id);
        }
    }
}