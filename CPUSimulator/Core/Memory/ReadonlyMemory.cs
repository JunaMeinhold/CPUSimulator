namespace CPUSimulator.Core.Memory
{
    using System.Buffers.Binary;

    public class ReadonlyMemory
    {
        public ReadonlyMemory()
        {
            MCOP = new(4, "MCOP");
            CC = new(4, "CC");
            NextIMCAR = new(4, "NextIMCAR");
            MCAR = new(4, "MCAR");
            Microcodes = null!;
        }

        public Register MCOP;

        public Register CC;

        public Register NextIMCAR;

        public Register MCAR;

        public Microcode[] Microcodes { get; set; }

        public Microcode Current => Microcodes[BitConverter.ToInt32(MCAR.Value)];

        public bool EndOfData => BitConverter.ToInt32(MCAR.Value) > Microcodes.Length - 1;

        public void Update()
        {
            MCAR.CopyFrom(NextIMCAR.Value);
        }

        public void Compute(int mc, int mcnext)
        {
            var mcar = BinaryPrimitives.ReadInt32LittleEndian(MCAR.Value);
            var mcop = BinaryPrimitives.ReadInt32LittleEndian(MCOP.Value);
            var cc = (ALUFlag)BinaryPrimitives.ReadInt32LittleEndian(CC.Value);
            int nextValue;
            switch (mc)
            {
                case 0b0:
                    nextValue = 4 * mcnext;
                    break;

                case 0b001:
                    nextValue = mcar + 1 + 4 * mcnext;
                    break;

                case 0b010:
                    nextValue = mcar + 1 - 4 * mcnext;
                    break;

                case 0b011:
                    nextValue = 4 * mcop;
                    break;

                case 0b100:
                    if (cc == ALUFlag.ZeroFlag) // Eqauls
                        nextValue = 4 * mcop;
                    else
                        nextValue = mcar + 1;
                    break;

                case 0b101:
                    if (cc == ALUFlag.CarryFlag) // Greater
                        nextValue = 4 * mcop;
                    else
                        nextValue = mcar + 1;
                    break;

                case 0b110:
                    if (cc == ALUFlag.SignFlag) // Less
                        nextValue = 4 * mcop;
                    else
                        nextValue = mcar + 1;
                    break;

                default:
                    return;
            }

            NextIMCAR.SetValue(nextValue);
        }

        public void Reset()
        {
            MCOP.Reset();
            CC.Reset();
            NextIMCAR.Reset();
            MCAR.Reset();
        }
    }
}