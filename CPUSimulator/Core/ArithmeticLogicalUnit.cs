namespace CPUSimulator.Core
{
    using CPUSimulator.Core.Memory;
    using System.Buffers.Binary;

    public class ArithmeticLogicalUnit
    {
        public ArithmeticLogicalUnit()
        {
            XRegister = new(8, "XR");
            YRegister = new(8, "YR");
            ZRegister = new(8, "ZR");
            FlagRegister = new(4, "FR");
        }

        public Register XRegister;

        public Register YRegister;

        public Register ZRegister;

        public Register FlagRegister;

        public ALUFunction Function;

        public void Reset()
        {
            Function = ALUFunction.NoOperation;
            XRegister.Reset();
            YRegister.Reset();
            ZRegister.Reset();
            FlagRegister.Reset();
        }

        private long ZValue
        {
            set
            {
                UpdateFlag(value);
                ZRegister.SetValue(value);
            }
        }

        private long UpdateFlag(long b)
        {
            ALUFlag flags = ALUFlag.NoneFlag;
            if (b == 0)
            {
                // is zero
                flags |= ALUFlag.ZeroFlag;
            }
            else
            {
                if (b < 0)
                {
                    // < zero
                    flags |= ALUFlag.SignFlag;
                }
                else
                {
                    // > zero
                    flags |= ALUFlag.CarryFlag;
                }
            }

            FlagRegister.SetValue((int)flags);
            return b;
        }

        public void Execute()
        {
            long x = BinaryPrimitives.ReadInt64LittleEndian(XRegister.Value);
            long y = BinaryPrimitives.ReadInt64LittleEndian(YRegister.Value);
            long z = BinaryPrimitives.ReadInt64LittleEndian(ZRegister.Value);
            switch (Function)
            {
                case ALUFunction.NoOperation:
                    ZValue = z;
                    break;

                case ALUFunction.NegateZ:
                    ZValue = -z;
                    break;

                case ALUFunction.PassX:
                    ZValue = x;
                    break;

                case ALUFunction.NegateX:
                    ZValue = -x;
                    break;

                case ALUFunction.PassY:
                    ZValue = y;
                    break;

                case ALUFunction.NegateY:
                    ZValue = -y;
                    break;

                case ALUFunction.PassYFlipXY:
                    {
                        ZValue = x;
                        XRegister.SetValue(y);
                        YRegister.SetValue(x);
                    }
                    break;

                case ALUFunction.PassXFlipXY:
                    {
                        ZValue = x;
                        XRegister.SetValue(y);
                        YRegister.SetValue(x);
                    }
                    break;

                case ALUFunction.PassXSetYX:
                    ZValue = x;
                    XRegister.SetValue(y);
                    break;

                case ALUFunction.Increment:
                    ZValue = x + 1;
                    break;

                case ALUFunction.Decrement:
                    ZValue = x - 1;
                    break;

                case ALUFunction.Addition:
                    ZValue = x + y;
                    break;

                case ALUFunction.Substraction:
                    ZValue = x - y;
                    break;

                case ALUFunction.Multiplication:
                    ZValue = x * y;
                    break;

                case ALUFunction.Division:
                    ZValue = x / y;
                    break;

                case ALUFunction.Modulus:
                    ZValue = x % y;
                    break;

                case ALUFunction.ShiftArithmeticallyLeft:
                    ZValue = x << (int)y;
                    break;

                case ALUFunction.ShiftArithmeticallyRight:
                    {
                        var t = x >> (int)y;
                        if ((x & 0x8000) == 0x8000)
                        {
                            t |= 0x8000;
                        }
                        ZValue = t;
                    }
                    break;

                case ALUFunction.Compare:
                    ZValue = x - y;
                    break;

                case ALUFunction.And:
                    ZValue = x & y;
                    break;

                case ALUFunction.Nand:
                    ZValue = x & y ^ long.MaxValue;
                    break;

                case ALUFunction.Or:
                    ZValue = x | y;
                    break;

                case ALUFunction.Nor:
                    ZValue = (x | y) ^ 0xFFFFFFFF;
                    break;

                case ALUFunction.Xor:
                    ZValue = x ^ y;
                    break;

                case ALUFunction.Nxor:
                    ZValue = x ^ y ^ 0xFFFFFFFF;
                    break;

                case ALUFunction.ShiftLogicalLeft:
                    ZValue = x << (int)y;
                    break;

                case ALUFunction.ShiftLogicalRight:
                    ZValue = x >> (int)y;
                    break;

                case ALUFunction.Comply:
                    ZValue = x & y ^ x;
                    break;

                case ALUFunction.Null:
                    ZValue = 0;
                    break;

                case ALUFunction.Max:
                    ZValue = 0xFFFFFFFF;
                    break;

                case ALUFunction.N1:
                    ZValue = 0x1;
                    break;

                case ALUFunction.N2:
                    ZValue = 0x2;
                    break;

                case ALUFunction.N3:
                    ZValue = 0x3;
                    break;

                case ALUFunction.N4:
                    ZValue = 0x4;
                    break;

                case ALUFunction.N5:
                    ZValue = 0x5;
                    break;

                case ALUFunction.N6:
                    ZValue = 0x6;
                    break;

                case ALUFunction.N7:
                    ZValue = 0x7;
                    break;

                case ALUFunction.N8:
                    ZValue = 0x8;
                    break;

                case ALUFunction.N9:
                    ZValue = 0x9;
                    break;

                case ALUFunction.NA:
                    ZValue = 0xA;
                    break;

                case ALUFunction.NB:
                    ZValue = 0xB;
                    break;

                case ALUFunction.NC:
                    ZValue = 0xC;
                    break;

                case ALUFunction.ND:
                    ZValue = 0xD;
                    break;

                case ALUFunction.NE:
                    ZValue = 0xE;
                    break;

                case ALUFunction.NF:
                    ZValue = 0xF;
                    break;

                default:
                    ZValue = z;
                    break;
            }
        }
    }
}