namespace CPUSimulator.Core.Decoding
{
    using CPUSimulator.Core;
    using System.Collections;

    public class MicrocodeQueue : IEnumerable<Microcode>
    {
        private readonly List<Microcode> queue = new();

        public void Clear()
        {
            queue.Clear();
        }

        public void Enqueue(Microcode microcode)
        {
            queue.Add(microcode);
        }

        public int Count => queue.Count;

        public Microcode this[int index] => queue[index];

        public List<Microcode>.Enumerator GetEnumerator()
        {
            return queue.GetEnumerator();
        }

        IEnumerator<Microcode> IEnumerable<Microcode>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}