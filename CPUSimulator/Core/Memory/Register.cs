namespace CPUSimulator.Core.Memory
{
    using CPUSimulator.Core.Buses;

    public class Register : IBusInput, IBusOutput
    {
        private readonly byte[] _value;
        private readonly int size;

        public string DebugName { get; }

        public Register(int size, string name)
        {
            this.size = size;
            DebugName = name;
            _value = new byte[size];
        }

        public void Reset()
        {
            Array.Clear(_value, 0, size);
        }

        public byte[] Value
        {
            get => _value;
        }

        public void CopyFrom(byte[] other)
        {
            Buffer.BlockCopy(other, 0, _value, 0, Math.Min(size, other.Length));
        }

        public void SetValue(byte constant)
        {
            Value[0] = constant;
        }

        public void SetValue(short constant)
        {
            Value[0] = (byte)(constant & 0xFF);
            Value[1] = (byte)(constant >> 8 & 0xFF);
        }

        public void SetValue(int constant)
        {
            Value[0] = (byte)(constant & 0xFF);
            Value[1] = (byte)(constant >> 8 & 0xFF);
            Value[2] = (byte)(constant >> 16 & 0xFF);
            Value[3] = (byte)(constant >> 24 & 0xFF);
        }

        public void SetValue(long constant)
        {
            Value[0] = (byte)(constant & 0xFF);
            Value[1] = (byte)(constant >> 8 & 0xFF);
            Value[2] = (byte)(constant >> 16 & 0xFF);
            Value[3] = (byte)(constant >> 24 & 0xFF);
            Value[4] = (byte)(constant >> 32 & 0xFF);
            Value[5] = (byte)(constant >> 40 & 0xFF);
            Value[6] = (byte)(constant >> 48 & 0xFF);
            Value[7] = (byte)(constant >> 56 & 0xFF);
        }

        public void SetValue(ulong constant)
        {
            Value[0] = (byte)(constant & 0xFF);
            Value[1] = (byte)(constant >> 8 & 0xFF);
            Value[2] = (byte)(constant >> 16 & 0xFF);
            Value[3] = (byte)(constant >> 24 & 0xFF);
            Value[4] = (byte)(constant >> 32 & 0xFF);
            Value[5] = (byte)(constant >> 40 & 0xFF);
            Value[6] = (byte)(constant >> 48 & 0xFF);
            Value[7] = (byte)(constant >> 56 & 0xFF);
        }
    }
}