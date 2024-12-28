namespace CPUSimulator.Core.Memory
{
    using CPUSimulator.Core.Buses;
    using System.Buffers.Binary;

    public class Register : IBusInput, IBusOutput
    {
        private readonly byte[] value;
        private readonly int offset;
        private readonly int size;

        public string DebugName { get; }

        public RegisterAddress Address { get; }

        public uint Size => (uint)size;

        public Register(int size, string name, Register? parent = null, int offset = 0, RegisterAddress address = 0)
        {
            this.size = size;
            this.offset = offset;
            Address = address;
            DebugName = name;
            if (parent != null)
            {
                value = parent.value;
            }
            else
            {
                value = new byte[size];
            }
        }

        public void Reset()
        {
            Array.Clear(value, 0, size);
        }

        public Span<byte> Value
        {
            get => value.AsSpan(offset, size);
        }

        public void CopyFrom(Span<byte> other)
        {
            Array.Clear(value);
            int toCopy = Math.Min(size, other.Length);
            other[..toCopy].CopyTo(Value);
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

        public unsafe void SetValue(float constant)
        {
            uint value = *(uint*)&constant;
            Value[0] = (byte)(value & 0xFF);
            Value[1] = (byte)(value >> 8 & 0xFF);
            Value[2] = (byte)(value >> 16 & 0xFF);
            Value[3] = (byte)(value >> 24 & 0xFF);
        }

        public unsafe void SetValue(double constant)
        {
            ulong value = *(ulong*)&constant;
            Value[0] = (byte)(value & 0xFF);
            Value[1] = (byte)(value >> 8 & 0xFF);
            Value[2] = (byte)(value >> 16 & 0xFF);
            Value[3] = (byte)(value >> 24 & 0xFF);
            Value[4] = (byte)(value >> 32 & 0xFF);
            Value[5] = (byte)(value >> 40 & 0xFF);
            Value[6] = (byte)(value >> 48 & 0xFF);
            Value[7] = (byte)(value >> 56 & 0xFF);
        }

        public ulong GetValueUInt64()
        {
            switch (size)
            {
                case 8:
                    return BinaryPrimitives.ReadUInt64LittleEndian(Value);

                case 4:
                    return BinaryPrimitives.ReadUInt32LittleEndian(Value);

                case 2:
                    return BinaryPrimitives.ReadUInt16LittleEndian(Value);

                case 1:
                    return Value[0];
            }
            return 0;
        }

        public uint GetValueUInt32()
        {
            switch (size)
            {
                case 8:
                    return (uint)BinaryPrimitives.ReadUInt64LittleEndian(Value);

                case 4:
                    return BinaryPrimitives.ReadUInt32LittleEndian(Value);

                case 2:
                    return BinaryPrimitives.ReadUInt16LittleEndian(Value);

                case 1:
                    return Value[0];
            }
            return 0;
        }
    }
}