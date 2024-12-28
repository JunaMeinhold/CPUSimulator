namespace CPUSimulator.Core.Memory
{
    using CPUSimulator.Core;
    using CPUSimulator.Core.Decoding;
    using System;

    public unsafe class ReadonlyMemory : IMemory
    {
        public ReadonlyMemory(uint size)
        {
            Data = AllocT<byte>(size);
            Memset(Data, 0, size);
            Size = size;
        }

        public byte* Data { get; }

        public uint Size { get; }

        public bool CanRead { get; } = true;

        public bool CanWrite { get; } = false;

        public AddressRange Range { get; private set; }

        public void Load(Instruction[] instructions)
        {
            Span<byte> data = new(Data, (int)Size);
            int total = 0;
            foreach (Instruction instruction in instructions)
            {
                int written = instruction.Write(data);
                total += written;
                data = data[written..];
            }
        }

        public void Map(MemoryManagementUnit mmu, AddressRange range)
        {
            Range = range;
            mmu.Map(range, MMUExecute);
        }

        private unsafe void MMUExecute(ulong address, Span<byte> span, MMUAction action)
        {
            if (action == MMUAction.Read)
            {
                fixed (byte* buffer = span)
                {
                    Buffer.MemoryCopy(Data + address, buffer, span.Length, span.Length);
                }
            }
            else
            {
                throw new InvalidOperationException("Cannot write to ROM.");
            }
        }

        public Span<byte> AsSpan()
        {
            return new Span<byte>(Data, (int)Size);
        }

        public void Reset()
        {
            Memset(Data, 0, Size);
        }
    }
}