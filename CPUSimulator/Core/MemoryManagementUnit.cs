namespace CPUSimulator.Core
{
    using CPUSimulator.Core.Memory;
    using System;
    using System.Buffers.Binary;

    public class MemoryManagementUnit : IDisposable
    {
        private readonly List<(AddressRange Range, Action<ulong, Span<byte>, MMUAction> Fetch)> mappings = [];

        /// <summary>
        /// MemoryAddressRegister
        /// </summary>
        public readonly Register MAR;

        /// <summary>
        /// MemoryDataRegister
        /// </summary>
        public readonly Register MDR;

        public RAMMode Mode;
        public RAMBusWidth BusWidth = RAMBusWidth.Bits8;

        public MemoryManagementUnit()
        {
            MAR = Register.Create(8, RegisterAddress.Disabled, "MAR");
            MDR = Register.Create(8, RegisterAddress.Disabled, "MDR");
        }

        public void Map(AddressRange range, Action<ulong, Span<byte>, MMUAction> fetch)
        {
            mappings.Add((range, fetch));
        }

        public void Execute(ulong virtualAddress, Span<byte> buffer, MMUAction action)
        {
            foreach (var (range, fetch) in mappings)
            {
                if (virtualAddress >= range.Start && virtualAddress < range.End)
                {
                    ulong offset = virtualAddress - range.Start + range.PhysicalOffset;
                    fetch(offset, buffer, action);
                    return;
                }
            }

            throw new InvalidOperationException($"Invalid memory access at address {virtualAddress:X}.");
        }

        public void Update()
        {
            switch (Mode)
            {
                case RAMMode.Wait:
                    return;

                case RAMMode.Write:
                    WriteBus();
                    break;

                case RAMMode.Read:
                    ReadBus();
                    break;

                default:
                    return;
            }
        }

        private void WriteBus()
        {
            ulong address = BinaryPrimitives.ReadUInt64LittleEndian(MAR.Value);
            int width = Convert(BusWidth);

            Execute(address, MDR.Value[..width], MMUAction.Write);
        }

        private static int Convert(RAMBusWidth busWidth)
        {
            return busWidth switch
            {
                RAMBusWidth.Bits8 => 1,
                RAMBusWidth.Bits16 => 2,
                RAMBusWidth.Bits32 => 4,
                RAMBusWidth.Bits64 => 8,
                _ => 0
            };
        }

        private void ReadBus()
        {
            ulong address = BinaryPrimitives.ReadUInt64LittleEndian(MAR.Value);
            int width = Convert(BusWidth);
            Execute(address, MDR.Value[..width], MMUAction.Read);
        }

        public void Reset()
        {
            MAR.Reset();
            MDR.Reset();
            Mode = RAMMode.Wait;
            BusWidth = RAMBusWidth.Bits8;
        }

        public void Dispose()
        {
            MAR.Dispose();
            MDR.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    public struct AddressRange
    {
        public ulong Start;
        public ulong End;
        public ulong PhysicalOffset;

        public AddressRange(ulong start, ulong length, ulong physicalOffset = 0)
        {
            Start = start;
            End = start + length;
            PhysicalOffset = physicalOffset;
        }

        public readonly bool InRange(ulong virtualAddress)
        {
            return virtualAddress >= Start && virtualAddress < End;
        }
    }
}