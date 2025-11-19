namespace CPUSimulator.Core.Memory
{
    using CPUSimulator.Core.Buses;
    using System.Runtime.CompilerServices;

    public unsafe struct Register : IBusInput, IBusOutput
    {
        private byte* memory;
        private uint size;
        private readonly RegisterAddress address;
        private readonly bool owns;
        private readonly string debugName;

        public Register(byte* memory, uint size, RegisterAddress address, string debugName, bool owns)
        {
            this.memory = memory;
            this.size = size;
            this.address = address;
            this.owns = owns;
            this.debugName = debugName;
            Reset();
        }

        public static Register Create(uint size, RegisterAddress address, string debugName)
        {
            var mem = Alloc(size);
            return new((byte*)mem, size, address, debugName, true);
        }

        public readonly Span<byte> Value => new(memory, (int)size);

        public readonly uint Size => size;

        public string DebugName => debugName;

        public RegisterAddress Address => address;

        public void Dispose()
        {
            if (memory != null)
            {
                if (owns)
                {
                    Free(memory);
                }

                memory = null;
                size = 0;
            }
        }

        public readonly Register MakeSubRegister(uint start, uint length, RegisterAddress address, string debugName)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(length, size - start);
            return new(memory + start, length, address, debugName, false);
        }

        public readonly void Reset()
        {
            Memset(memory, 0, size);
        }

        public readonly void CopyFrom(Span<byte> other)
        {
            Reset();
            var toCopy = Math.Min(size, other.Length);
            fixed (byte* mem = other)
            {
                Memcpy(mem, memory, toCopy);
            }
        }

        public readonly void SetValue<T>(T value) where T : unmanaged
        {
            uint sizeToCopy = (uint)sizeof(T);
            if (sizeToCopy > size)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            sizeToCopy = Math.Min(sizeToCopy, size);
            if (BitConverter.IsLittleEndian)
            {
                Memcpy(&value, memory, sizeToCopy);
            }
            else
            {
                var ptr = (byte*)&value;
                for (uint i = 0; i < sizeToCopy; ++i)
                {
                    memory[i] = ptr[sizeToCopy - i - 1];
                }
            }
        }

        public T GetValue<T>() where T : unmanaged
        {
            uint sizeT = (uint)sizeof(T);
            sizeT = Math.Min(size, sizeT);

            Unsafe.SkipInit(out T value);
            if (BitConverter.IsLittleEndian)
            {
                Memcpy(memory, &value, sizeT);
            }
            else
            {
                var ptr = (byte*)&value;
                for (uint i = 0; i < sizeT; ++i)
                {
                    ptr[i] = memory[i - sizeT - 1];
                }
            }

            return value;
        }
    }
}