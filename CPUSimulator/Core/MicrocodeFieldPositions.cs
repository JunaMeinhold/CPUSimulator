namespace CPUSimulator.Core
{
    public static class MicrocodeFieldPositions
    {
        // 54 Bit
        // MC  MCNext CC ALU-Mode ALU-FC X-Bus    Y-Bus    Z-Bus    IO-RAM Mode RAM-Bus-Width
        // 000 000000 0  00       000000 00000000 00000000 00000000 000000 00   00

        public const int MC_SHIFT = 48;
        public const int MC_BITS = 3;
        public const ulong MC_MASK = (1UL << MC_BITS) - 1; // 0b111 (3 bits)

        public const int MCNEXT_SHIFT = 43;
        public const int MCNEXT_BITS = 5;
        public const ulong MCNEXT_MASK = (1UL << MCNEXT_BITS) - 1; // 0b11111 (5 bits)

        public const int CC_SHIFT = 42;
        public const int CC_BITS = 1;
        public const ulong CC_MASK = (1UL << CC_BITS) - 1; // 0b1 (1 bit)

        public const int ALU_MODE_SHIFT = 40;
        public const int ALU_MODE_BITS = 2;
        public const ulong ALU_MODE_MASK = (1UL << ALU_MODE_BITS) - 1; // 0b11 (2 bits)

        public const int ALU_FC_SHIFT = 34;
        public const int ALU_FC_BITS = 6;
        public const ulong ALU_FC_MASK = (1UL << ALU_FC_BITS) - 1; // 0b111111 (6 bits)

        public const int X_BUS_SHIFT = 26;
        public const int X_BUS_BITS = 8;
        public const ulong X_BUS_MASK = (1UL << X_BUS_BITS) - 1; // 0b11111111 (8 bits)

        public const int Y_BUS_SHIFT = 18;
        public const int Y_BUS_BITS = 8;
        public const ulong Y_BUS_MASK = (1UL << Y_BUS_BITS) - 1; // 0b11111111 (8 bits)

        public const int Z_BUS_SHIFT = 10;
        public const int Z_BUS_BITS = 8;
        public const ulong Z_BUS_MASK = (1UL << Z_BUS_BITS) - 1; // 0b11111111 (8 bits)

        public const int IO_RAM_SHIFT = 4;
        public const int IO_RAM_BITS = 6;
        public const ulong IO_RAM_MASK = (1UL << IO_RAM_BITS) - 1; // 0b111111 (6 bits)

        public const int RAM_MODE_SHIFT = 2;
        public const int RAM_MODE_BITS = 2;
        public const ulong RAM_MODE_MASK = (1UL << RAM_MODE_BITS) - 1; // 0b11 (2 bits)

        public const int RAM_BUS_WIDTH_SHIFT = 0;
        public const int RAM_BUS_WIDTH_BITS = 2;
        public const ulong RAM_BUS_WIDTH_MASK = (1UL << RAM_BUS_WIDTH_BITS) - 1; // 0b11 (2 bits)
    }
}