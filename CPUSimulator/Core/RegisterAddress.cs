namespace CPUSimulator.Core
{
    public enum RegisterBank : byte
    {
        Invalid = 0x0, // Invalid register bank (used for Disabled state)
        GPR64 = 0x1, // General-Purpose Registers (64-bit, e.g., RAX, RBX, RCX, RDX)
        GPR32 = 0x2, // General-Purpose Registers (32-bit, e.g., EAX, EBX, ECX, EDX)
        GPR16 = 0x3, // General-Purpose Registers (16-bit, e.g., AX, BX, CX, DX)
        GPR8H = 0x4, // General-Purpose Registers (8-bit high, e.g., AH, BH, CH, DH)
        GPR8L = 0x5, // General-Purpose Registers (8-bit low, e.g., AL, BL, CL, DL)
        SEG =   0x6, // Segment Registers (CS, DS, ES, FS, GS, SS)

        CR  =   0x7, // Control Registers CR0-CR8
        DR  =   0x8, // Debug Registers DR0-DR7
        Special = 0x9, // Special Registers (e.g., EFLAGS/RFLAGS)
        Internal = 0xA, // Internal Registers (e.g., temporary or hidden registers)

        XMM =   0xB, // XMM Registers (XMM0-XMM15)
    }

    /// <summary>
    /// Specifies the set of supported CPU register addresses, including general-purpose and SIMD registers, for use in
    /// low-level code generation or emulation scenarios.           
    /// </summary>
    /// <remarks>This enumeration includes 64-bit, 32-bit, 16-bit, and 8-bit sub-registers for x86/x64
    /// architectures, as well as XMM SIMD registers. The values are typically used to identify registers when emitting
    /// or interpreting machine instructions. The Disabled value indicates that no register is selected or
    /// active.</remarks>
    public enum RegisterAddress : byte
    {
        Disabled = (RegisterBank.Invalid << 4) | 0,  // Reserved for no active register

        // RAX Family

        RAX = (RegisterBank.GPR64 << 4) | 0,
        EAX = (RegisterBank.GPR32 << 4) | 0,
        AX = (RegisterBank.GPR16 << 4) | 0,
        AL = (RegisterBank.GPR8L << 4) | 0,
        AH = (RegisterBank.GPR8H << 4) | 0,
        // RBX Family

        RBX = (RegisterBank.GPR64 << 4) | 1,
        EBX = (RegisterBank.GPR32 << 4) | 1,
        BX = (RegisterBank.GPR16 << 4) | 1,
        BL = (RegisterBank.GPR8L << 4) | 1,
        BH = (RegisterBank.GPR8H << 4) | 1,

        // RCX Family

        RCX = (RegisterBank.GPR64 << 4) | 2,
        ECX = (RegisterBank.GPR32 << 4) | 2,
        CX = (RegisterBank.GPR16 << 4) | 2,
        CL = (RegisterBank.GPR8L << 4) | 2,
        CH = (RegisterBank.GPR8H << 4) | 2,

        // RDX Family

        RDX = (RegisterBank.GPR64 << 4) | 3,
        EDX = (RegisterBank.GPR32 << 4) | 3,
        DX = (RegisterBank.GPR16 << 4) | 3,
        DL = (RegisterBank.GPR8L << 4) | 3,
        DH = (RegisterBank.GPR8H << 4) | 3,

        // Other Registers (no sub-registers)

        RSP = (RegisterBank.GPR64 << 4) | 4,
        RBP = (RegisterBank.GPR64 << 4) | 5,
        RDI = (RegisterBank.GPR64 << 4) | 6,
        RSI = (RegisterBank.GPR64 << 4) | 7,

        // R8 Family

        R8 = (RegisterBank.GPR64 << 4) | 8,       // Full 64-bit register
        R8D = (RegisterBank.GPR32 << 4) | 8,      // Lower 32 bits of R8
        R8W = (RegisterBank.GPR16 << 4) | 8,      // Lower 16 bits of R8
        R8B = (RegisterBank.GPR8L << 4) | 8,      // Lower 8 bits of R8

        // R9 Family

        R9 = (RegisterBank.GPR64 << 4) | 9,
        R9D = (RegisterBank.GPR32 << 4) | 9,
        R9W = (RegisterBank.GPR16 << 4) | 9,
        R9B = (RegisterBank.GPR8L << 4) | 9,

        // R10 Family

        R10 = (RegisterBank.GPR64 << 4) | 10,
        R10D = (RegisterBank.GPR32 << 4) | 10,
        R10W = (RegisterBank.GPR16 << 4) | 10,
        R10B = (RegisterBank.GPR8L << 4) | 10,

        // R11 Family

        R11 = (RegisterBank.GPR64 << 4) | 11,
        R11D = (RegisterBank.GPR32 << 4) | 11,
        R11W = (RegisterBank.GPR16 << 4) | 11,
        R11B = (RegisterBank.GPR8L << 4) | 11,

        // R12 Family

        R12 = (RegisterBank.GPR64 << 4) | 12,
        R12D = (RegisterBank.GPR32 << 4) | 12,
        R12W = (RegisterBank.GPR16 << 4) | 12,
        R12B = (RegisterBank.GPR8L << 4) | 12,

        // R13 Family

        R13 = (RegisterBank.GPR64 << 4) | 13,
        R13D = (RegisterBank.GPR32 << 4) | 13,
        R13W = (RegisterBank.GPR16 << 4) | 13,
        R13B = (RegisterBank.GPR8L << 4) | 13,

        // R14 Family

        R14 = (RegisterBank.GPR64 << 4) | 14,
        R14D = (RegisterBank.GPR32 << 4) | 14,
        R14W = (RegisterBank.GPR16 << 4) | 14,
        R14B = (RegisterBank.GPR8L << 4) | 14,

        // R15 Family

        R15 = (RegisterBank.GPR64 << 4) | 15,
        R15D = (RegisterBank.GPR32 << 4) | 15,
        R15W = (RegisterBank.GPR16 << 4) | 15,
        R15B = (RegisterBank.GPR8L << 4) | 15,

        XMM0 = (RegisterBank.XMM << 4) | 0,
        XMM1 = (RegisterBank.XMM << 4) | 1,
        XMM2 = (RegisterBank.XMM << 4) | 2,
        XMM3 = (RegisterBank.XMM << 4) | 3,
        XMM4 = (RegisterBank.XMM << 4) | 4,
        XMM5 = (RegisterBank.XMM << 4) | 5,
        XMM6 = (RegisterBank.XMM << 4) | 6,
        XMM7 = (RegisterBank.XMM << 4) | 7,
        XMM8 = (RegisterBank.XMM << 4) | 8,
        XMM9 = (RegisterBank.XMM << 4) | 9,
        XMM10 = (RegisterBank.XMM << 4) | 10,
        XMM11 = (RegisterBank.XMM << 4) | 11,
        XMM12 = (RegisterBank.XMM << 4) | 12,
        XMM13 = (RegisterBank.XMM << 4) | 13,
        XMM14 = (RegisterBank.XMM << 4) | 14,
        XMM15 = (RegisterBank.XMM << 4) | 15,
    }
}