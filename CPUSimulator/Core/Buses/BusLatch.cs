namespace CPUSimulator.Core.Buses
{
    public class BusLatch<T> where T : IBusInput
    {
        private int activeIndex;

        public BusLatch(T[] ports)
        {
            Ports = ports;
        }

        public readonly T[] Ports;

        public T Active => Ports[activeIndex];

        public void SetActive(int index) => activeIndex = index;
    }
}