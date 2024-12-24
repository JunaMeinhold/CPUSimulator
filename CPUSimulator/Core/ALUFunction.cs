namespace CPUSimulator.Core
{
    public enum ALUFunction : int
    {
        NoOperation,
        NegateZ,
        PassX,
        NegateX,
        PassY,
        NegateY,
        PassYFlipXY,
        PassXFlipXY,
        PassXSetYX,
        Increment,
        Decrement,
        Addition,
        Substraction,
        Multiplication,
        Division,
        Modulus,
        ShiftArithmeticallyLeft,
        ShiftArithmeticallyRight,
        Compare,
        And,
        Nand,
        Or,
        Nor,
        Xor,
        Nxor,
        ShiftLogicalLeft,
        ShiftLogicalRight,
        Comply,
        Null, // 0x0
        Max,  // 0xFFFFFFFF
        N1, // 0x1
        N2, // 0x2
        N3, // 0x3
        N4, // 0x4
        N5, // 0x5
        N6, // 0x6
        N7, // 0x7
        N8, // 0x8
        N9, // 0x9
        NA, // 0xA
        NB, // 0xB
        NC, // 0xC,
        ND, // 0xD,
        NE, // 0xE
        NF, // 0xF
    }
}