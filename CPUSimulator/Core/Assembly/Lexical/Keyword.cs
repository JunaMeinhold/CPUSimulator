namespace CPUSimulator.Core.Assembly.Lexical
{
    using CPUSimulator.Core.Decoding;
    using Hexa.NET.DXGI;
    using Hexa.NET.Mathematics;
    using System.Runtime.Intrinsics.Arm;
    using System.Security.Cryptography;

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
            return keyword switch
            {
                Keyword.RAX => RegisterAddress.RAX,       // Full 64-bit register
                Keyword.EAX => RegisterAddress.EAX,       // Lower 32 bits of RAX
                Keyword.AX => RegisterAddress.AX,         // Lower 16 bits of RAX
                Keyword.AL => RegisterAddress.AL,         // Lower 8 bits of AX
                Keyword.AH => RegisterAddress.AH,         // Upper 8 bits of AX
                // RBX Family

                Keyword.RBX => RegisterAddress.RBX,
                Keyword.EBX => RegisterAddress.EBX,
                Keyword.BX => RegisterAddress.BX,
                Keyword.BL => RegisterAddress.BL,
                Keyword.BH => RegisterAddress.BH,

                // RCX Family

                Keyword.RCX => RegisterAddress.RCX,
                Keyword.ECX => RegisterAddress.ECX,
                Keyword.CX => RegisterAddress.CX,
                Keyword.CL => RegisterAddress.CL,
                Keyword.CH => RegisterAddress.CH,

                // RDX Family

                Keyword.RDX => RegisterAddress.RDX,
                Keyword.EDX => RegisterAddress.EDX,
                Keyword.DX => RegisterAddress.DX,
                Keyword.DL => RegisterAddress.DL,
                Keyword.DH => RegisterAddress.DH,

                // Other Registers (no sub-registers)

                Keyword.RSP => RegisterAddress.RSP,
                Keyword.RBP => RegisterAddress.RBP,
                Keyword.RDI => RegisterAddress.RDI,
                Keyword.RSI => RegisterAddress.RSI,

                // R8 Family

                Keyword.R8 => RegisterAddress.R8,       // Full 64-bit register
                Keyword.R8D => RegisterAddress.R8D,      // Lower 32 bits of R8
                Keyword.R8W => RegisterAddress.R8W,      // Lower 16 bits of R8
                Keyword.R8B => RegisterAddress.R8B,      // Lower 8 bits of R8

                // R9 Family

                Keyword.R9 => RegisterAddress.R9,
                Keyword.R9D => RegisterAddress.R9D,
                Keyword.R9W => RegisterAddress.R9W,
                Keyword.R9B => RegisterAddress.R9B,

                // R10 Family

                Keyword.R10 => RegisterAddress.R10,
                Keyword.R10D => RegisterAddress.R10D,
                Keyword.R10W => RegisterAddress.R10W,
                Keyword.R10B => RegisterAddress.R10B,

                // R11 Family

                Keyword.R11 => RegisterAddress.R11,
                Keyword.R11D => RegisterAddress.R11D,
                Keyword.R11W => RegisterAddress.R11W,
                Keyword.R11B => RegisterAddress.R11B,

                // R12 Family

                Keyword.R12 => RegisterAddress.R12,
                Keyword.R12D => RegisterAddress.R12D,
                Keyword.R12W => RegisterAddress.R12W,
                Keyword.R12B => RegisterAddress.R12B,

                // R13 Family

                Keyword.R13 => RegisterAddress.R13,
                Keyword.R13D => RegisterAddress.R13D,
                Keyword.R13W => RegisterAddress.R13W,
                Keyword.R13B => RegisterAddress.R13B,

                // R14 Family

                Keyword.R14 => RegisterAddress.R14,
                Keyword.R14D => RegisterAddress.R14D,
                Keyword.R14W => RegisterAddress.R14W,
                Keyword.R14B => RegisterAddress.R14B,

                // R15 Family

                Keyword.R15 => RegisterAddress.R15,
                Keyword.R15D => RegisterAddress.R15D,
                Keyword.R15W => RegisterAddress.R15W,
                Keyword.R15B => RegisterAddress.R15B,

                Keyword.XMM0 => RegisterAddress.XMM0,
                Keyword.XMM1 => RegisterAddress.XMM1,
                Keyword.XMM2 => RegisterAddress.XMM2,
                Keyword.XMM3 => RegisterAddress.XMM3,
                Keyword.XMM4 => RegisterAddress.XMM4,
                Keyword.XMM5 => RegisterAddress.XMM5,
                Keyword.XMM6 => RegisterAddress.XMM6,
                Keyword.XMM7 => RegisterAddress.XMM7,
                Keyword.XMM8 => RegisterAddress.XMM8,
                Keyword.XMM9 => RegisterAddress.XMM9,
                Keyword.XMM10 => RegisterAddress.XMM10,
                Keyword.XMM11 => RegisterAddress.XMM11,
                Keyword.XMM12 => RegisterAddress.XMM12,
                Keyword.XMM13 => RegisterAddress.XMM13,
                Keyword.XMM14 => RegisterAddress.XMM14,
                Keyword.XMM15 => RegisterAddress.XMM15,
                _ => throw new InvalidOperationException($"Keyword {keyword} is not a register.")
            };
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