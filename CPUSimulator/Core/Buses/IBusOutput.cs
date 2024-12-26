namespace CPUSimulator.Core.Buses
{
    public interface IBusOutput
    {
        public Span<byte> Value { get; }

        void CopyFrom(Span<byte> other);
    }
}