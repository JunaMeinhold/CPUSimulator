namespace CPUSimulator.Core.Memory
{
    using CPUSimulator.Core;
    using System;

    public unsafe class RandomAccessMemory : IMemory, IDisposable
    {
        private byte* data;
        private uint size;

        public RandomAccessMemory(uint size)
        {
            this.size = size;
            data = AllocT<byte>(size);
        }

        public byte* Data => data;

        public uint Size => size;
        
        public bool CanRead { get; } = true;

        public bool CanWrite { get; } = true;

        public AddressRange Range { get; private set; }

        public ulong Map(MemoryManagementUnit mmu, AddressRange range)
        {
            Range = range;
            mmu.Map(range, MMUExecute);
            return range.Length;
        }

        private unsafe void MMUExecute(ulong address, Span<byte> span, MMUAction action)
        {
            fixed (byte* buffer = span)
            {
                if (action == MMUAction.Read)
                {
                    ulong toCopy = Math.Min((ulong)span.Length, Size - address);
                    Memcpy(Data + address, buffer, toCopy);
                }
                else
                {
                    Buffer.MemoryCopy(buffer, Data + address, Size - address, (ulong)span.Length);
                }
            }
        }

        public void Reset()
        {
            Memset(Data, 0, Size);
        }

        public void Dispose()
        {
            if (data != null)
            {
                Free(data);
                data = null;
                size = 0;
            }
            GC.SuppressFinalize(this);
        }
    }
}