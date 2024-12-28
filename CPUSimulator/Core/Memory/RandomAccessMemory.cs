namespace CPUSimulator.Core.Memory
{
    using CPUSimulator.Core;
    using System;

    public unsafe class RandomAccessMemory : IMemory
    {
        public RandomAccessMemory(uint size)
        {
            Size = size;
            Data = AllocT<byte>(size);
        }

        public uint Size { get; }

        public byte* Data { get; }

        public bool CanRead { get; } = true;

        public bool CanWrite { get; } = true;

        public AddressRange Range { get; private set; }

        public void Map(MemoryManagementUnit mmu, AddressRange range)
        {
            Range = range;
            mmu.Map(range, MMUExecute);
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
    }
}