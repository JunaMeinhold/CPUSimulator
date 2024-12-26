namespace CPUSimulator.Core.Memory
{
    using System.Buffers.Binary;

    public enum MemoryControlFlag
    {
        None = 0,
        Step = 0b001,
        StepBack = 0b010,
        Jump = 0b011,
        Equals = 0b100,
        Greater = 0b101,
        Less = 0b110,
    }

    public class MemoryManagementUnit
    {
        public MemoryManagementUnit(ReadonlyMemory readonlyMemory, RandomAccessMemory randomAccessMemory)
        {
            MCOP = new(8, "MCOP");
            CC = new(4, "CC");
            NextIMCAR = new(8, "NextIMCAR");
            MCAR = new(8, "MCAR");
            this.readonlyMemory = readonlyMemory;
            this.randomAccessMemory = randomAccessMemory;
        }

        public Register MCOP;
        public Register CC;
        public Register NextIMCAR;
        public Register MCAR;
        private readonly ReadonlyMemory readonlyMemory;
        private readonly RandomAccessMemory randomAccessMemory;
    }

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

        public int Size => Microcodes?.Length ?? 0;

        public void Update()
        {
            MCAR.CopyFrom(NextIMCAR.Value);
        }

        public Register PC = new(8, "PC");

        public void Update(MemoryControlFlag mc)
        {
            ulong value = BinaryPrimitives.ReadUInt64LittleEndian(PC.Value);
            ulong mcop = BinaryPrimitives.ReadUInt64LittleEndian(MCOP.Value);
            ALUFlag cc = (ALUFlag)BinaryPrimitives.ReadInt32LittleEndian(CC.Value);

            ulong nextValue = value + 1;

            switch (mc)
            {
                case MemoryControlFlag.StepBack:
                    nextValue = value - 1;
                    break;

                case MemoryControlFlag.Jump:
                    nextValue = mcop;
                    break;

                case MemoryControlFlag.Equals:
                    if ((cc & ALUFlag.ZeroFlag) != 0)
                        nextValue = mcop;
                    else
                        nextValue = value + 1;
                    break;

                case MemoryControlFlag.Greater:
                    if ((cc & ALUFlag.CarryFlag) != 0) // Greater
                        nextValue = mcop;
                    else
                        nextValue = value + 1;
                    break;

                case MemoryControlFlag.Less:
                    if ((cc & ALUFlag.SignFlag) != 0) // Less
                        nextValue = mcop;
                    else
                        nextValue = value + 1;
                    break;
            }
        }

        public void Compute(MemoryControlFlag mc, int mcnext)
        {
            var mcar = BinaryPrimitives.ReadInt32LittleEndian(MCAR.Value);
            var mcop = BinaryPrimitives.ReadInt32LittleEndian(MCOP.Value);
            var cc = (ALUFlag)BinaryPrimitives.ReadInt32LittleEndian(CC.Value);
            int nextValue;
            switch (mc)
            {
                case MemoryControlFlag.None:
                    nextValue = 4 * mcnext;
                    break;

                case MemoryControlFlag.Step:
                    nextValue = mcar + 1 + 4 * mcnext;
                    break;

                case MemoryControlFlag.StepBack:
                    nextValue = mcar + 1 - 4 * mcnext;
                    break;

                case MemoryControlFlag.Jump:
                    nextValue = 4 * mcop;
                    break;

                case MemoryControlFlag.Equals:
                    if ((cc & ALUFlag.ZeroFlag) != 0) // Eqauls
                        nextValue = 4 * mcop;
                    else
                        nextValue = mcar + 1;
                    break;

                case MemoryControlFlag.Greater:
                    if ((cc & ALUFlag.CarryFlag) != 0) // Greater
                        nextValue = 4 * mcop;
                    else
                        nextValue = mcar + 1;
                    break;

                case MemoryControlFlag.Less:
                    if ((cc & ALUFlag.SignFlag) != 0) // Less
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