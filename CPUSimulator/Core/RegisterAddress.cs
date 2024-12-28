namespace CPUSimulator.Core
{
    public enum RegisterAddress : byte
    {
        Disabled = 0,  // Reserved for no active register

        // RAX Family

        RAX = 1,       // Full 64-bit register
        EAX = 2,       // Lower 32 bits of RAX
        AX = 3,        // Lower 16 bits of RAX
        AL = 4,        // Lower 8 bits of AX
        AH = 5,        // Upper 8 bits of AX

        // RBX Family

        RBX = 6,
        EBX = 7,
        BX = 8,
        BL = 9,
        BH = 10,

        // RCX Family

        RCX = 11,
        ECX = 12,
        CX = 13,
        CL = 14,
        CH = 15,

        // RDX Family

        RDX = 16,
        EDX = 17,
        DX = 18,
        DL = 19,
        DH = 20,

        // Other Registers (no sub-registers)

        RSP = 21,
        RBP = 22,
        RDI = 23,
        RSI = 24,

        // R8 Family

        R8 = 25,       // Full 64-bit register
        R8D = 26,      // Lower 32 bits of R8
        R8W = 27,      // Lower 16 bits of R8
        R8B = 28,      // Lower 8 bits of R8

        // R9 Family

        R9 = 29,
        R9D = 30,
        R9W = 31,
        R9B = 32,

        // R10 Family

        R10 = 33,
        R10D = 34,
        R10W = 35,
        R10B = 36,

        // R11 Family

        R11 = 37,
        R11D = 38,
        R11W = 39,
        R11B = 40,

        // R12 Family

        R12 = 41,
        R12D = 42,
        R12W = 43,
        R12B = 44,

        // R13 Family

        R13 = 45,
        R13D = 46,
        R13W = 47,
        R13B = 48,

        // R14 Family

        R14 = 49,
        R14D = 50,
        R14W = 51,
        R14B = 52,

        // R15 Family

        R15 = 53,
        R15D = 54,
        R15W = 55,
        R15B = 56,

        XMM0,
        XMM1,
        XMM2,
        XMM3,
        XMM4,
        XMM5,
        XMM6,
        XMM7,
        XMM8,
        XMM9,
        XMM10,
        XMM11,
        XMM12,
        XMM13,
        XMM14,
        XMM15,
    }
}