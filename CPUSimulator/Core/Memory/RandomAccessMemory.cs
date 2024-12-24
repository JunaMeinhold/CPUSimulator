namespace CPUSimulator.Core.Memory
{
    using System.Buffers.Binary;

    public class RandomAccessMemory
    {
        public RandomAccessMemory(int size)
        {
            Size = size;
            Data = new byte[size];
            MAR = new(8, "MAR");
            MDR = new(8, "MDR");
        }

        /// <summary>
        /// MemoryAddressRegister
        /// </summary>
        public Register MAR;

        /// <summary>
        /// MemoryDataRegister
        /// </summary>
        public Register MDR;

        public RAMMode Mode;
        public RAMBusWidth BusWidth = RAMBusWidth.Bits8;

        public int Size { get; }

        public byte[] Data;

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
            int width = (int)BusWidth;
            for (uint i = 0; i < width; i++)
            {
                Data[address + i] = MDR.Value[i];
            }
        }

        private void ReadBus()
        {
            ulong address = BinaryPrimitives.ReadUInt64LittleEndian(MAR.Value);
            int width = (int)BusWidth;
            for (uint i = 0; i < width; i++)
            {
                MDR.Value[i] = Data[address + i];
            }
        }

        public void Reset()
        {
            MAR.Reset();
            MDR.Reset();
            Mode = RAMMode.Wait;
            BusWidth = RAMBusWidth.Bits8;
        }
    }
}