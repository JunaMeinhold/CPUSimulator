namespace CPUSimulator.Core.Buses
{
    public interface IBusInput
    {
        public byte[] Value { get; }

        void CopyFrom(byte[] other);
    }
}