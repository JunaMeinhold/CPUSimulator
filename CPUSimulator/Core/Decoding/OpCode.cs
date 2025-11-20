namespace CPUSimulator.Core.Decoding
{
    public enum OpCode : byte
    {
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
    }

    public static class OpCodeExtensions
    {
        public static bool IsBinary(this OpCode opCode)
        {
            return opCode >= OpCode.MOV && opCode <= OpCode.CMPSW;
        }

        public static bool IsUnary(this OpCode opCode)
        {
            return opCode >= OpCode.CALL && opCode <= OpCode.LOOPNE;
        }

        public static bool HasNoOperands(this OpCode opCode)
        {
            return opCode >= OpCode.CLI && opCode <= OpCode.INT;
        }
    }
}