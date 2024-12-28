namespace CPUSimulator.Core.Decoding
{
    public enum OperandSource : byte
    {
        None = 0,
        Imm8,
        Imm16,
        Imm32,
        Imm64,
        ImmAddress,

        // RAX Family

        RAX,       // Full 64-bit register
        EAX,       // Lower 32 bits of RAX
        AX,        // Lower 16 bits of RAX
        AL,        // Lower 8 bits of AX
        AH,        // Upper 8 bits of AX

        // RBX Family

        RBX,
        EBX,
        BX,
        BL,
        BH,

        // RCX Family

        RCX,
        ECX,
        CX,
        CL,
        CH,

        // RDX Family

        RDX,
        EDX,
        DX,
        DL,
        DH,

        // Other Registers (no sub-registers)

        RSP,
        RBP,
        RDI,
        RSI,

        // R8 Family

        R8,       // Full 64-bit register
        R8D,      // Lower 32 bits of R8
        R8W,      // Lower 16 bits of R8
        R8B,      // Lower 8 bits of R8

        // R9 Family

        R9,
        R9D,
        R9W,
        R9B,

        // R10 Family

        R10,
        R10D,
        R10W,
        R10B,

        // R11 Family

        R11,
        R11D,
        R11W,
        R11B,

        // R12 Family

        R12,
        R12D,
        R12W,
        R12B,

        // R13 Family

        R13,
        R13D,
        R13W,
        R13B,

        // R14 Family

        R14,
        R14D,
        R14W,
        R14B,

        // R15 Family

        R15,
        R15D,
        R15W,
        R15B,
        RegisterCount
    }
}