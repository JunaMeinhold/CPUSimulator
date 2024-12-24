namespace CPUSimulator.Core
{
    using System.Collections;

    public class BitArrayReader
    {
        private readonly BitArray bitArray;
        public int Position;

        public BitArrayReader(byte[] bytes) : this(new BitArray(bytes))
        {
        }

        public BitArrayReader(BitArray bitArray)
        {
            this.bitArray = bitArray;
            Position = 0;
        }

        public int Length => bitArray.Length;

        public bool[] Read(int count)
        {
            var output = new bool[count];
            for (int i = 0; i < count; i++)
            {
                output[i] = bitArray[Position];
                Position++;
            }
            return output;
        }

        public byte ReadByte(int count)
        {
            return ToByte(Read(count));
        }

        public short ReadShort(int count)
        {
            return ToShort(Read(count));
        }

        public int ReadInt(int count)
        {
            return ToInt(Read(count));
        }

        public long ReadLong(int count)
        {
            return ToLong(Read(count));
        }

        public byte[] ReadBytes(int countBit)
        {
            var data = new byte[countBit / 8];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = ReadByte(8);
            }
            return data;
        }

        private static byte ToByte(bool[] bits)
        {
            byte output = 0;
            for (int i = bits.Length - 1, j = 0; i >= 0; i--, j++)
            {
                if (bits[i])
                {
                    output += (byte)(1 * Pow(2, j));
                }
            }

            return output;
        }

        private static short ToShort(bool[] bits)
        {
            short output = 0;
            for (int i = bits.Length - 1, j = 0; i >= 0; i--, j++)
            {
                if (bits[i])
                {
                    output += (short)(1 * Pow(2, j));
                }
            }

            return output;
        }

        private static int ToInt(bool[] bits)
        {
            var output = 0;
            for (int i = bits.Length - 1, j = 0; i >= 0; i--, j++)
            {
                if (bits[i])
                {
                    output += 1 * Pow(2, j);
                }
            }

            return output;
        }

        private static long ToLong(bool[] bits)
        {
            var output = 0L;
            for (int i = bits.Length - 1, j = 0; i >= 0; i--, j++)
            {
                if (bits[i])
                {
                    output += 1 * Pow(2, j);
                }
            }

            return output;
        }

        private static long Pow(long x, long pow)
        {
            long ret = 1;
            while (pow != 0)
            {
                if ((pow & 1) == 1)
                    ret *= x;
                x *= x;
                pow >>= 1;
            }
            return ret;
        }

        private static int Pow(int x, int pow)
        {
            int ret = 1;
            while (pow != 0)
            {
                if ((pow & 1) == 1)
                    ret *= x;
                x *= x;
                pow >>= 1;
            }
            return ret;
        }

        private static byte Pow(byte x, byte pow)
        {
            byte ret = 1;
            while (pow != 0)
            {
                if ((pow & 1) == 1)
                    ret *= x;
                x *= x;
                pow >>= 1;
            }
            return ret;
        }
    }
}