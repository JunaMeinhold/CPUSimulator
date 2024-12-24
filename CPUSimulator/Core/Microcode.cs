namespace CPUSimulator.Core
{
    public unsafe struct Microcode
    {
        public ulong Low;
        public ulong High;

        public Microcode(ulong code)
        {
            Source = string.Empty;
            Low = code;
            High = 0;
        }

        public Microcode(ulong code, byte value)
        {
            Source = string.Empty;
            Low = code;
            High = value;
        }

        public Microcode(ulong code, short value)
        {
            Source = string.Empty;
            Low = code;
            High = *(ushort*)&value;
        }

        public Microcode(ulong code, int value)
        {
            Source = string.Empty;
            Low = code;
            High = *(uint*)&value;
        }

        public Microcode(ulong code, long value)
        {
            Source = string.Empty;
            Low = code;
            High = *(ulong*)&value;
        }

        private static ulong Parse(string code)
        {
            ulong result = 0;

            int bit = 0;
            for (int i = 0; i < code.Length; i++)
            {
                var c = code[i];
                bool isTrue = c == '1';
                if (isTrue || c == '0')
                {
                    result |= (isTrue ? 1ul : 0ul) << 48 - bit;
                    bit++;
                }
            }

            return result;
        }

        public string Source;
    }
}