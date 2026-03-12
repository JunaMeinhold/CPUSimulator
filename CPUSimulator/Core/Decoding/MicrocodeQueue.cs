namespace CPUSimulator.Core.Decoding
{
    using CPUSimulator.Core;
    using Hexa.NET.Utilities;
    using System.Collections;

    public class MicrocodeQueue : IEnumerable<Microcode>, IDisposable
    {
        private UnsafeList<Microcode> queue = new();

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

        public UnsafeList<Microcode>.Enumerator GetEnumerator()
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

        public void Dispose()
        {
            queue.Release();
            GC.SuppressFinalize(this);
        }
    }
}