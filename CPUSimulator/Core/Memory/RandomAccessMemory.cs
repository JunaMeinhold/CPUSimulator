namespace CPUSimulator.Core.Memory
{
    using System;
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
            int width = Convert(BusWidth);
            for (uint i = 0; i < width; i++)
            {
                Data[address + i] = MDR.Value[(int)i];
            }
        }

        private static int Convert(RAMBusWidth busWidth)
        {
            return busWidth switch
            {
                RAMBusWidth.Bits8 => 1,
                RAMBusWidth.Bits16 => 2,
                RAMBusWidth.Bits32 => 3,
                RAMBusWidth.Bits64 => 4,
                _ => 0
            };
        }

        private void ReadBus()
        {
            ulong address = BinaryPrimitives.ReadUInt64LittleEndian(MAR.Value);
            int width = Convert(BusWidth);
            for (uint i = 0; i < width; i++)
            {
                MDR.Value[(int)i] = Data[address + i];
            }
        }

        public void Reset()
        {
            MAR.Reset();
            MDR.Reset();
            Mode = RAMMode.Wait;
            BusWidth = RAMBusWidth.Bits8;
            Array.Clear(Data, 0, Data.Length);
        }
    }
}