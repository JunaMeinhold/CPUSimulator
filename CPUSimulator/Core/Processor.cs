namespace CPUSimulator.Core
{
    using CPUSimulator.Core.Buses;
    using CPUSimulator.Core.Memory;
    using static CPUSimulator.Core.MicrocodeFieldPositions;

    public class Processor
    {
        public ArithmeticLogicalUnit ALU;

        public RandomAccessMemory RAM;

        public ReadonlyMemory ROM;

        public Register[] Registers;

        public InputBus XBus;

        public InputBus YBus;

        public OutputBus ZBus;

        public InputBus ORAMBus;

        public OutputBus IRAMBus;

        public SingleBus FlagsCCBus;

        public SingleBus MDRMCOPBus;

        public const int RAX = 0;
        public const int RBX = 1;
        public const int RCX = 2;
        public const int RDX = 3;
        public const int REX = 4;
        public const int RFX = 5;
        public const int RGX = 6;
        public const int RHX = 7;

        public Processor()
        {
            ALU = new();
            ROM = new();
            RAM = new(32);
            Registers = new Register[8];
            Registers[0] = new Register(8, "RAX");
            Registers[1] = new Register(8, "RBX");
            Registers[2] = new Register(4, "RCX");
            Registers[3] = new Register(4, "RDX");
            Registers[4] = new Register(2, "REX");
            Registers[5] = new Register(2, "RFX");
            Registers[6] = new Register(1, "RGX");
            Registers[7] = new Register(1, "RHX");
            List<IBusInput> inputyBus = Registers.ToList().ConvertAll(x => (IBusInput)x);
            XBus = new InputBus(ALU.XRegister, Registers);
            ZBus = new OutputBus(ALU.ZRegister, Registers);
            inputyBus.Add(RAM.MDR);
            YBus = new InputBus(ALU.YRegister, inputyBus.ToArray());
            IRAMBus = new OutputBus(ALU.ZRegister, RAM.MAR, RAM.MDR);
            ORAMBus = new InputBus(ALU.ZRegister, RAM.MAR, RAM.MDR);
            FlagsCCBus = new SingleBus(ALU.FlagRegister, ROM.CC);
            MDRMCOPBus = new SingleBus(RAM.MDR, ROM.MCOP);
        }

        public void Reset()
        {
            ROM.Reset();
            RAM.Reset();
            ALU.Reset();
            foreach (Register reg in Registers)
            {
                reg.Reset();
            }
        }

        public unsafe void Execute()
        {
            bool* xbus = stackalloc bool[8];
            bool* ybus = stackalloc bool[8];
            bool* zbus = stackalloc bool[8];
            bool* ioram = stackalloc bool[6];
            while (!ROM.EndOfData)
            {
                var microcode = ROM.Current;

                // Fetch

                ulong first = microcode.Low;

                int mc = (int)(first >> MC_SHIFT & MC_MASK);
                int mcnext = (int)(first >> MCNEXT_SHIFT & MCNEXT_MASK);

                bool cc = (first >> CC_SHIFT & CC_MASK) != 0;

                int alufc = (int)(first >> ALU_FC_SHIFT & ALU_FC_MASK);

                for (int i = 0; i < X_BUS_BITS; i++)
                {
                    xbus[i] = (first >> X_BUS_SHIFT + i & 0x1) != 0;
                }

                for (int i = 0; i < Y_BUS_BITS; i++)
                {
                    ybus[i] = (first >> Y_BUS_SHIFT + i & 0x1) != 0;
                }

                for (int i = 0; i < Z_BUS_BITS; i++)
                {
                    zbus[i] = (first >> Z_BUS_SHIFT + i & 0x1) != 0;
                }

                for (int i = 0; i < IO_RAM_BITS; i++)
                {
                    ioram[i] = (first >> IO_RAM_SHIFT + i & 0x1) != 0;
                }

                int mode = (int)(first >> RAM_MODE_SHIFT & RAM_MODE_MASK);

                int width = (int)(first >> RAM_BUS_WIDTH_SHIFT & RAM_BUS_WIDTH_MASK);

                ALU.YRegister.SetValue(microcode.High);

                FlagsCCBus.UpdateState(cc);
                ALU.Function = (ALUFunction)alufc;
                XBus.UpdateState(xbus, 8);
                ZBus.UpdateState(zbus, 8);
                IRAMBus.UpdateState(ioram, 2);
                ORAMBus.UpdateState(ioram + 2, 2);
                YBus.UpdateState(ybus, 8);
                YBus.UpdateState(ioram + 4, 1, 8);
                MDRMCOPBus.UpdateState(ioram + 5, 1);
                RAM.Mode = (RAMMode)mode;
                RAM.BusWidth = (RAMBusWidth)width;

                // Fetch Phase 2
                XBus.Push();
                YBus.Push();
                ORAMBus.Push();

                // Execute
                ALU.Execute();
                FlagsCCBus.Push();

                // Store
                ZBus.Push();
                IRAMBus.Push();
                MDRMCOPBus.Push();
                RAM.Update();
                ROM.Compute(mc, mcnext);
                ROM.Update();
            }
        }
    }
}