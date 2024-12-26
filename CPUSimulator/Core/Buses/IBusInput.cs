namespace CPUSimulator.Core.Buses
{
    public interface IBusInput
    {
        public Span<byte> Value { get; }

        void CopyFrom(Span<byte> other);
    }
}