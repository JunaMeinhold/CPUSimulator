namespace CPUSimulator.Core.Assembly.Lexical
{
    using CPUSimulator.Core.Decoding;

    public enum Keyword
    {
        Unknown,
        Byte,
        Word,
        Dword,
        Qword,

        // Directives
        Section,

        Global,
        Org,

        // Section names
        Text,

        Data,
        Rodata,
        Bss,

        // Data directives
        Db,  // Define byte

        Dw,  // Define word
        Dd,  // Define double-word
        Dq,  // Define quad-word
        Dbs, // Define null-terminated string

        // Reserve directives
        Resb, // Reserve bytes

        Resw, // Reserve words
        Resd, // Reserve double-words
        Resq, // Reserve quad-words

        // Other directives
        Align, // Align to a specified boundary

        // -------------------------------
        // Opcodes
        // -------------------------------

        // 2 Operand instructions

        MOV,
        LEA,

        ADD,
        SUB,
        MUL,
        DIV,

        OR,
        XOR,
        AND,

        TEST,
        CMP,
        CMPS,
        CMPSB,
        CMPSW,

        // 1 Operand instructions

        CALL,
        PUSH,
        POP,

        DEC,
        INC,
        NEG,
        NOT,

        JAE,
        JB,
        JBE,
        JC,
        JCXZ,
        JE,
        JG,
        JGE,
        JL,
        JLE,
        JMP,
        JNE,
        JNZ,
        LOOP,
        LOOPNE,

        // 0 Operand instructions

        CLI,
        STI,
        HLT,
        IRET,
        RET,
        NOP,
        INT,

        // -------------------------------
        // Registers
        // -------------------------------

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

    public static class KeywordExtensions
    {
        public static bool IsRegister(this Keyword keyword)
        {
            return keyword >= Keyword.RAX && keyword <= Keyword.XMM15;
        }

        public static RegisterAddress ToRegisterAddress(this Keyword keyword)
        {
            return (RegisterAddress)(keyword - Keyword.RAX + 1);
        }

        public static bool IsOpCode(this Keyword keyword)
        {
            return keyword >= Keyword.MOV && keyword <= Keyword.INT;
        }

        public static OpCode ToOpCode(this Keyword keyword)
        {
            return (OpCode)(keyword - Keyword.MOV);
        }
    }
}