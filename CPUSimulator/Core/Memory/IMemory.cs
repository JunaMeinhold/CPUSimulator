namespace CPUSimulator.Core.Memory
{
    using CPUSimulator.Core;

    public interface IMemory
    {
        bool CanRead { get; }

        bool CanWrite { get; }

        unsafe byte* Data { get; }

        uint Size { get; }

        ulong Map(MemoryManagementUnit mmu, AddressRange range);

        void Reset();

        public AddressRange Range { get; }
    }
}