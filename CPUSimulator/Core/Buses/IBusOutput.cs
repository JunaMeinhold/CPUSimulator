namespace CPUSimulator.Core.Buses
{
    public interface IBusOutput
    {
        public byte[] Value { get; }

        void CopyFrom(byte[] other);
    }
}