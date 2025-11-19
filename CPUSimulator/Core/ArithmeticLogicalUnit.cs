namespace CPUSimulator.Core
{
    using CPUSimulator.Core.Memory;
    using System;
    using System.Buffers.Binary;

    public class ArithmeticLogicalUnit : IDisposable
    {
        public ArithmeticLogicalUnit()
        {
            XRegister = Register.Create(8, RegisterAddress.Disabled, "XR");
            YRegister = Register.Create(8, RegisterAddress.Disabled, "YR");
            ZRegister = Register.Create(8, RegisterAddress.Disabled, "ZR");
            FlagRegister = Register.Create(8, RegisterAddress.Disabled, "FR");
        }

        public Register XRegister;
        public Register YRegister;
        public Register ZRegister;
        public Register FlagRegister;
        public ALUFunction Function;
        public ALUMode Mode;

        public void Reset()
        {
            Function = ALUFunction.NoOperation;
            Mode = ALUMode.Unsigned;
            XRegister.Reset();
            YRegister.Reset();
            ZRegister.Reset();
            FlagRegister.Reset();
        }

        private long ZValueSigned
        {
            set
            {
                UpdateFlag(value);
                ZRegister.SetValue(value);
            }
        }

        private ulong ZValueUnsigned
        {
            set
            {
                UpdateFlag(value);
                ZRegister.SetValue(value);
            }
        }

        private void UpdateFlag(long b)
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

            FlagRegister.SetValue((ulong)flags);
        }

        private void UpdateFlag(ulong b)
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

            FlagRegister.SetValue((ulong)flags);
        }

        public void Execute()
        {
            switch (Mode)
            {
                case ALUMode.Unsigned:
                    ExecuteUnsigned();
                    break;

                case ALUMode.Signed:
                    ExecuteSigned();
                    break;

                case ALUMode.Float:
                    ExecuteFloat();
                    break;
            }
        }

        private unsafe void ExecuteUnsigned()
        {
            ulong x = BinaryPrimitives.ReadUInt64LittleEndian(XRegister.Value);
            ulong y = BinaryPrimitives.ReadUInt64LittleEndian(YRegister.Value);
            ulong z = BinaryPrimitives.ReadUInt64LittleEndian(ZRegister.Value);
            switch (Function)
            {
                case ALUFunction.NoOperation:
                    ZValueUnsigned = z;
                    break;

                case ALUFunction.PassX:
                    ZValueUnsigned = x;
                    break;

                case ALUFunction.PassY:
                    ZValueUnsigned = y;
                    break;

                case ALUFunction.PassYFlipXY:
                    {
                        ZValueUnsigned = x;
                        XRegister.SetValue(y);
                        YRegister.SetValue(x);
                    }
                    break;

                case ALUFunction.PassXFlipXY:
                    {
                        ZValueUnsigned = x;
                        XRegister.SetValue(y);
                        YRegister.SetValue(x);
                    }
                    break;

                case ALUFunction.PassXSetYX:
                    ZValueUnsigned = x;
                    XRegister.SetValue(y);
                    break;

                case ALUFunction.Increment:
                    ZValueUnsigned = x + 1;
                    break;

                case ALUFunction.Decrement:
                    ZValueUnsigned = x - 1;
                    break;

                case ALUFunction.Addition:
                    ZValueUnsigned = x + y;
                    break;

                case ALUFunction.Substraction:
                    ZValueUnsigned = x - y;
                    break;

                case ALUFunction.Multiplication:
                    ZValueUnsigned = x * y;
                    break;

                case ALUFunction.Division:
                    ZValueUnsigned = x / y;
                    break;

                case ALUFunction.Modulus:
                    ZValueUnsigned = x % y;
                    break;

                case ALUFunction.ShiftArithmeticallyLeft:
                    ZValueUnsigned = x << (int)y;
                    break;

                case ALUFunction.ShiftArithmeticallyRight:
                    {
                        var t = x >> (int)y;
                        if ((x & 0x8000) == 0x8000)
                        {
                            t |= 0x8000;
                        }
                        ZValueUnsigned = t;
                    }
                    break;

                case ALUFunction.Compare:
                    ulong b = x - y;
                    ZRegister.SetValue(b);
                    ALUFlag flags = ALUFlag.NoneFlag;

                    if (b == 0)
                    {
                        flags |= ALUFlag.ZeroFlag; // Result is zero
                    }

                    if (x < y)
                    {
                        flags |= ALUFlag.CarryFlag; // Borrow occurred in unsigned subtraction
                    }

                    if (((long)x - (long)y) < 0)
                    {
                        flags |= ALUFlag.SignFlag; // Result is negative in signed subtraction
                    }

                    bool overflow = ((x ^ y) & (x ^ b) & 0x8000000000000000) != 0;
                    if (overflow)
                    {
                        flags |= ALUFlag.OverflowFlag; // Signed overflow occurred
                    }

                    FlagRegister.SetValue((ulong)flags);
                    break;

                case ALUFunction.And:
                    ZValueUnsigned = x & y;
                    break;

                case ALUFunction.Nand:
                    ZValueUnsigned = x & y ^ ulong.MaxValue;
                    break;

                case ALUFunction.Or:
                    ZValueUnsigned = x | y;
                    break;

                case ALUFunction.Nor:
                    ZValueUnsigned = (x | y) ^ ulong.MaxValue;
                    break;

                case ALUFunction.Xor:
                    ZValueUnsigned = x ^ y;
                    break;

                case ALUFunction.Nxor:
                    ZValueUnsigned = x ^ y ^ ulong.MaxValue;
                    break;

                case ALUFunction.ShiftLogicalLeft:
                    ZValueUnsigned = x << (int)y;
                    break;

                case ALUFunction.ShiftLogicalRight:
                    ZValueUnsigned = x >> (int)y;
                    break;

                case ALUFunction.Comply:
                    ZValueUnsigned = x & y ^ x;
                    break;

                case ALUFunction.Min:
                    ZValueUnsigned = x > y ? y : x;
                    break;

                case ALUFunction.Max:
                    ZValueUnsigned = x > y ? x : y;
                    break;

                default:
                    ZValueUnsigned = z;
                    break;
            }
        }

        private void ExecuteSigned()
        {
            long x = BinaryPrimitives.ReadInt64LittleEndian(XRegister.Value);
            long y = BinaryPrimitives.ReadInt64LittleEndian(YRegister.Value);
            long z = BinaryPrimitives.ReadInt64LittleEndian(ZRegister.Value);
            switch (Function)
            {
                case ALUFunction.NoOperation:
                    ZValueSigned = z;
                    break;

                case ALUFunction.NegateZ:
                    ZValueSigned = -z;
                    break;

                case ALUFunction.PassX:
                    ZValueSigned = x;
                    break;

                case ALUFunction.NegateX:
                    ZValueSigned = -x;
                    break;

                case ALUFunction.PassY:
                    ZValueSigned = y;
                    break;

                case ALUFunction.NegateY:
                    ZValueSigned = -y;
                    break;

                case ALUFunction.PassYFlipXY:
                    {
                        ZValueSigned = x;
                        XRegister.SetValue(y);
                        YRegister.SetValue(x);
                    }
                    break;

                case ALUFunction.PassXFlipXY:
                    {
                        ZValueSigned = x;
                        XRegister.SetValue(y);
                        YRegister.SetValue(x);
                    }
                    break;

                case ALUFunction.PassXSetYX:
                    ZValueSigned = x;
                    XRegister.SetValue(y);
                    break;

                case ALUFunction.Increment:
                    ZValueSigned = x + 1;
                    break;

                case ALUFunction.Decrement:
                    ZValueSigned = x - 1;
                    break;

                case ALUFunction.Addition:
                    ZValueSigned = x + y;
                    break;

                case ALUFunction.Substraction:
                    ZValueSigned = x - y;
                    break;

                case ALUFunction.Multiplication:
                    ZValueSigned = x * y;
                    break;

                case ALUFunction.Division:
                    ZValueSigned = x / y;
                    break;

                case ALUFunction.Modulus:
                    ZValueSigned = x % y;
                    break;

                case ALUFunction.ShiftArithmeticallyLeft:
                    ZValueSigned = x << (int)y;
                    break;

                case ALUFunction.ShiftArithmeticallyRight:
                    {
                        var t = x >> (int)y;
                        if ((x & 0x8000) == 0x8000)
                        {
                            t |= 0x8000;
                        }
                        ZValueSigned = t;
                    }
                    break;

                case ALUFunction.Compare:
                    ZValueSigned = x - y;
                    break;

                case ALUFunction.And:
                    ZValueSigned = x & y;
                    break;

                case ALUFunction.Nand:
                    ZValueSigned = x & y ^ long.MaxValue;
                    break;

                case ALUFunction.Or:
                    ZValueSigned = x | y;
                    break;

                case ALUFunction.Nor:
                    ZValueSigned = (x | y) ^ 0xFFFFFFFF;
                    break;

                case ALUFunction.Xor:
                    ZValueSigned = x ^ y;
                    break;

                case ALUFunction.Nxor:
                    ZValueSigned = x ^ y ^ 0xFFFFFFFF;
                    break;

                case ALUFunction.ShiftLogicalLeft:
                    ZValueSigned = x << (int)y;
                    break;

                case ALUFunction.ShiftLogicalRight:
                    ZValueSigned = x >> (int)y;
                    break;

                case ALUFunction.Comply:
                    ZValueSigned = x & y ^ x;
                    break;

                case ALUFunction.Min:
                    ZValueSigned = x > y ? y : x;
                    break;

                case ALUFunction.Max:
                    ZValueSigned = x > y ? x : y;
                    break;

                default:
                    ZValueSigned = z;
                    break;
            }
        }

        private void ExecuteFloat()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            XRegister.Dispose();
            YRegister.Dispose();
            ZRegister.Dispose();
            FlagRegister.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}