namespace CPUSimulator.Core
{
    using CPUSimulator.Core.Buses;
    using CPUSimulator.Core.Memory;
    using Hexa.NET.D3DCommon;
    using static CPUSimulator.Core.MicrocodeFieldPositions;

    public enum CPUMode
    {
        Normal,
        Halt,
        Step
    }

    public enum RegisterAddress : byte
    {
        Disabled = 0,  // Reserved for no active register

        // RAX Family

        RAX = 1,       // Full 64-bit register
        EAX = 2,       // Lower 32 bits of RAX
        AX = 3,        // Lower 16 bits of RAX
        AL = 4,        // Lower 8 bits of AX
        AH = 5,        // Upper 8 bits of AX

        // RBX Family

        RBX = 6,
        EBX = 7,
        BX = 8,
        BL = 9,
        BH = 10,

        // RCX Family

        RCX = 11,
        ECX = 12,
        CX = 13,
        CL = 14,
        CH = 15,

        // RDX Family

        RDX = 16,
        EDX = 17,
        DX = 18,
        DL = 19,
        DH = 20,

        // Other Registers (no sub-registers)

        RSP = 21,
        RBP = 22,
        RDI = 23,
        RSI = 24,
    }

    public class RegisterHelper
    {
        public const int RegisterCount = 24;

        public static int GetRegisterSize(RegisterAddress address)
        {
            return address switch
            {
                RegisterAddress.Disabled => 0,    // No register active

                // RAX and sub-registers
                RegisterAddress.RAX => 8,        // Full 64-bit register
                RegisterAddress.EAX => 4,        // Lower 32 bits
                RegisterAddress.AX => 2,         // Lower 16 bits
                RegisterAddress.AL => 1,         // Lower 8 bits
                RegisterAddress.AH => 1,         // Upper 8 bits

                // RBX and sub-registers
                RegisterAddress.RBX => 8,
                RegisterAddress.EBX => 4,
                RegisterAddress.BX => 2,
                RegisterAddress.BL => 1,
                RegisterAddress.BH => 1,

                // RCX and sub-registers
                RegisterAddress.RCX => 8,
                RegisterAddress.ECX => 4,
                RegisterAddress.CX => 2,
                RegisterAddress.CL => 1,
                RegisterAddress.CH => 1,

                // RDX and sub-registers
                RegisterAddress.RDX => 8,
                RegisterAddress.EDX => 4,
                RegisterAddress.DX => 2,
                RegisterAddress.DL => 1,
                RegisterAddress.DH => 1,

                // Other registers (no sub-registers)
                RegisterAddress.RSP => 8,
                RegisterAddress.RBP => 8,
                RegisterAddress.RDI => 8,
                RegisterAddress.RSI => 8,

                // Default case
                _ => throw new ArgumentOutOfRangeException(nameof(address), $"Unknown register: {address}")
            };
        }

        public static int GetRegisterOffset(RegisterAddress address)
        {
            return address switch
            {
                RegisterAddress.Disabled => 0,    // No register active

                // RAX and sub-registers
                RegisterAddress.RAX => 0,         // Full register, no offset
                RegisterAddress.EAX => 0,         // Lower 32 bits start at offset 0
                RegisterAddress.AX => 0,          // Lower 16 bits start at offset 0
                RegisterAddress.AL => 0,          // Lower 8 bits start at offset 0
                RegisterAddress.AH => 1,          // Upper 8 bits start at offset 1

                // RBX and sub-registers
                RegisterAddress.RBX => 0,
                RegisterAddress.EBX => 0,
                RegisterAddress.BX => 0,
                RegisterAddress.BL => 0,
                RegisterAddress.BH => 1,

                // RCX and sub-registers
                RegisterAddress.RCX => 0,
                RegisterAddress.ECX => 0,
                RegisterAddress.CX => 0,
                RegisterAddress.CL => 0,
                RegisterAddress.CH => 1,

                // RDX and sub-registers
                RegisterAddress.RDX => 0,
                RegisterAddress.EDX => 0,
                RegisterAddress.DX => 0,
                RegisterAddress.DL => 0,
                RegisterAddress.DH => 1,

                // Other registers (no sub-registers)
                RegisterAddress.RSP => 0,
                RegisterAddress.RBP => 0,
                RegisterAddress.RDI => 0,
                RegisterAddress.RSI => 0,

                // Default case
                _ => throw new ArgumentOutOfRangeException(nameof(address), $"Unknown register: {address}")
            };
        }

        public static RegisterAddress? GetParentRegister(RegisterAddress address)
        {
            return address switch
            {
                RegisterAddress.EAX or RegisterAddress.AX or RegisterAddress.AL or RegisterAddress.AH => RegisterAddress.RAX,
                RegisterAddress.EBX or RegisterAddress.BX or RegisterAddress.BL or RegisterAddress.BH => RegisterAddress.RBX,
                RegisterAddress.ECX or RegisterAddress.CX or RegisterAddress.CL or RegisterAddress.CH => RegisterAddress.RCX,
                RegisterAddress.EDX or RegisterAddress.DX or RegisterAddress.DL or RegisterAddress.DH => RegisterAddress.RDX,
                _ => null // No parent for other registers
            };
        }
    }

    public class Processor
    {
        public readonly ArithmeticLogicalUnit ALU;
        public readonly RandomAccessMemory RAM;
        public readonly ReadonlyMemory ROM;
        public readonly Register[] Registers;
        public readonly InputBus XBus;
        public readonly InputBus YBus;
        public readonly OutputBus ZBus;
        public readonly SingleBus YMDRBus;
        public readonly InputBus ORAMBus;
        public readonly OutputBus IRAMBus;
        public readonly SingleBus FlagsCCBus;
        public readonly SingleBus MDRMCOPBus;

        private readonly ManualResetEventSlim handle = new(true);
        private CPUMode executionMode;

        public Processor()
        {
            ALU = new();
            ROM = new();
            RAM = new(1048576);

            RegisterAddress[] registerAddresses = Enum.GetValues<RegisterAddress>();

            Registers = new Register[RegisterHelper.RegisterCount]; // skip disabled and count.
            var parentMap = new Dictionary<RegisterAddress, Register>();
            for (int i = 1; i < registerAddresses.Length; i++)
            {
                RegisterAddress registerAddress = registerAddresses[i];
                string name = registerAddress.ToString().ToLower();

                int size = RegisterHelper.GetRegisterSize(registerAddress);
                int offset = RegisterHelper.GetRegisterOffset(registerAddress);

                Register? parent = RegisterHelper.GetParentRegister(registerAddress) is { } parentAddress ? parentMap.GetValueOrDefault(parentAddress) : null;

                var register = new Register(size, name, parent, offset);
                Registers[i - 1] = register;

                if (parent == null)
                {
                    parentMap[registerAddress] = register;
                }
            }

            XBus = new InputBus(ALU.XRegister, Registers);
            ZBus = new OutputBus(ALU.ZRegister, Registers);
            YBus = new InputBus(ALU.YRegister, Registers);
            YMDRBus = new(ALU.YRegister, RAM.MDR);
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

        public CPUMode Mode { get => executionMode; }

        public void Step()
        {
            handle.Set();
        }

        public void StepMode(bool enabled)
        {
            if (enabled)
            {
                executionMode = CPUMode.Step;
                handle.Reset();
            }
            else
            {
                executionMode = CPUMode.Normal;
                handle.Set();
            }
        }

        public void Halt()
        {
            executionMode = CPUMode.Halt;
            handle.Reset();
        }

        public void Continue()
        {
            executionMode = CPUMode.Normal;
            handle.Set();
        }

        public unsafe void Execute(CancellationToken token)
        {
            bool* ioram = stackalloc bool[6];
            while (!ROM.EndOfData)
            {
                var microcode = ROM.Current;

                handle.Wait(token);

                if (token.IsCancellationRequested) return;

                if (executionMode == CPUMode.Step)
                {
                    handle.Reset();
                }

                // Fetch

                ulong code = microcode.Code;

                int mc = (int)(code >> MC_SHIFT & MC_MASK);
                int mcnext = (int)(code >> MCNEXT_SHIFT & MCNEXT_MASK);

                bool cc = (code >> CC_SHIFT & CC_MASK) != 0;

                int aluMode = (int)(code >> ALU_MODE_SHIFT & ALU_MODE_MASK);

                int alufc = (int)(code >> ALU_FC_SHIFT & ALU_FC_MASK);

                byte xbus = (byte)(code >> X_BUS_SHIFT & X_BUS_MASK);
                byte ybus = (byte)(code >> Y_BUS_SHIFT & Y_BUS_MASK);
                byte zbus = (byte)(code >> Z_BUS_SHIFT & Z_BUS_MASK);

                for (int i = 0; i < IO_RAM_BITS; i++)
                {
                    ioram[i] = (code >> IO_RAM_SHIFT + i & 0x1) != 0;
                }

                int mode = (int)(code >> RAM_MODE_SHIFT & RAM_MODE_MASK);

                int width = (int)(code >> RAM_BUS_WIDTH_SHIFT & RAM_BUS_WIDTH_MASK);

                ALU.YRegister.SetValue(microcode.Intermediate);

                FlagsCCBus.UpdateState(cc);
                ALU.Mode = (ALUMode)aluMode;
                ALU.Function = (ALUFunction)alufc;
                XBus.UpdateState(xbus, RegisterHelper.RegisterCount);
                ZBus.UpdateState(zbus, RegisterHelper.RegisterCount);
                IRAMBus.UpdateState(ioram, 2);
                ORAMBus.UpdateState(ioram + 2, 2);
                YBus.UpdateState(ybus, RegisterHelper.RegisterCount);
                YMDRBus.UpdateState(*(ioram + 4));
                MDRMCOPBus.UpdateState(ioram + 5, 1);
                RAM.Mode = (RAMMode)mode;
                RAM.BusWidth = (RAMBusWidth)width;

                XBus.Push();
                YBus.Push();
                YMDRBus.Push();
                ORAMBus.Push();

                // Execute
                ALU.Execute();
                FlagsCCBus.Push();

                // Store
                ZBus.Push();
                IRAMBus.Push();
                MDRMCOPBus.Push();
                RAM.Update();
                ROM.Compute((MemoryControlFlag)mc, mcnext);
                ROM.Update();
            }
        }
    }
}