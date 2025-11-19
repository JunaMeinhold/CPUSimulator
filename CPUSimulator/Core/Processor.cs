namespace CPUSimulator.Core
{
    using CPUSimulator.Core.Buses;
    using CPUSimulator.Core.Decoding;
    using CPUSimulator.Core.Memory;
    using System.Diagnostics;
    using static CPUSimulator.Core.MicrocodeFieldPositions;

    public class Processor : IDisposable
    {
        public readonly ControlUnit CU;
        public readonly MemoryManagementUnit MMU = new();
        public readonly ArithmeticLogicalUnit ALU;
        public readonly RandomAccessMemory RAM;
        public readonly ReadonlyMemory ROM;
        public readonly Register[] Registers;
        public readonly InputBus<Register, Register> XBus;
        public readonly InputBus<Register, Register> YBus;
        public readonly OutputBus<Register, Register> ZBus;
        public readonly SingleBus<Register, Register> YMDRBus;
        public readonly InputBus<Register, Register> ORAMBus;
        public readonly OutputBus<Register, Register> IRAMBus;
        public readonly SingleBus<Register, Register> FlagsCCBus;
        public readonly SingleBus<Register, Register> MDRMCOPBus;
        public readonly InterruptController InterruptController = new();

        private readonly ManualResetEventSlim simulationHandle = new(true);
        private readonly ManualResetEventSlim interruptHandle = new(true);
        private CPUMode executionMode;

        public Processor()
        {
            const uint memory = 1024 * 1024 * 16;
            ALU = new();
            ROM = new(4096);
            RAM = new(memory);

            RAM.Map(MMU, new(0, 16384)); // stack.
            ROM.Map(MMU, new(16384, 4096));
            RAM.Map(MMU, new(16384 + 4096, memory - 16384, physicalOffset: 16384));

            RegisterAddress[] registerAddresses = Enum.GetValues<RegisterAddress>();

            Registers = new Register[RegisterHelper.RegisterCount];
            var parentMap = new Dictionary<RegisterAddress, Register>();
            for (int i = 1; i < registerAddresses.Length; i++)
            {
                RegisterAddress registerAddress = registerAddresses[i];
                string name = registerAddress.ToString();

                int size = RegisterHelper.GetRegisterSize(registerAddress);
                int offset = RegisterHelper.GetRegisterOffset(registerAddress);

                Register? parent = RegisterHelper.GetParentRegister(registerAddress) is { } parentAddress ? parentMap.GetValueOrDefault(parentAddress) : null;
                Register register;
                if (parent != null)
                {
                    register = parent.Value.MakeSubRegister((uint)offset, (uint)size, registerAddress, name);
                }
                else
                {
                    register = Register.Create((uint)size, registerAddress, name);
                }
                Registers[i - 1] = register;

                if (parent == null)
                {
                    parentMap[registerAddress] = register;
                }
            }

            CU = new(MMU, Registers[(int)RegisterAddress.RSP - 1], romStart: 16384, 0);

            XBus = new(ALU.XRegister, Registers);
            ZBus = new(ALU.ZRegister, Registers);
            YBus = new(ALU.YRegister, Registers);
            YMDRBus = new(ALU.YRegister, MMU.MDR);
            IRAMBus = new(ALU.ZRegister, [MMU.MAR, MMU.MDR]);
            ORAMBus = new(ALU.ZRegister, [MMU.MAR, MMU.MDR]);
            FlagsCCBus = new(ALU.FlagRegister, CU.RFlags);
            MDRMCOPBus = new(MMU.MDR, CU.IOP);
        }

        public void Reset()
        {
            ROM.Reset();
            RAM.Reset();
            CU.Reset();
            MMU.Reset();
            ALU.Reset();
            foreach (var reg in Registers)
            {
                reg.Reset();
            }
            if (executionMode == CPUMode.Halt)
            {
                simulationHandle.Set();
                executionMode = CPUMode.Running;
            }
            interruptHandle.Set();
        }

        public CPUMode Mode { get => executionMode; }

        public double Latency { get; private set; }

        public void Step()
        {
            simulationHandle.Set();
        }

        public void StepMode(bool enabled)
        {
            if (enabled)
            {
                executionMode = CPUMode.Step;
                simulationHandle.Reset();
            }
            else
            {
                executionMode = CPUMode.Running;
                simulationHandle.Set();
            }
        }

        public void Halt()
        {
            executionMode = CPUMode.Halt;
            simulationHandle.Reset();
        }

        public void Resume()
        {
            if (executionMode == CPUMode.Halt)
            {
                executionMode = CPUMode.Running;
                simulationHandle.Set();
            }
        }

        public void TriggerInterrupt(int interruptNumber)
        {
            InterruptController.TriggerInterrupt(interruptNumber);
            interruptHandle.Set();
        }

        private void HandlePendingInterrupts()
        {
            if (!CU.InterruptEnabled) return;
            if (InterruptController.Take(out int it))
            {
                CU.HandleInterrupt(it);
            }
        }

        public unsafe void Execute(CancellationToken token)
        {
            bool* ioram = stackalloc bool[6];
            while (true)
            {
                long start = Stopwatch.GetTimestamp();
                HandlePendingInterrupts();
                var instruction = CU.Fetch();

                if (token.IsCancellationRequested) return;

                foreach (var microcode in Decoder.Decode(instruction))
                {
                    // Fetch
                    ulong code = microcode.Code;

                    ControlUnitFlag controlFlag = (ControlUnitFlag)(code >> MC_SHIFT & MC_MASK);

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
                    MMU.Mode = (RAMMode)mode;
                    MMU.BusWidth = (RAMBusWidth)width;

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
                    MMU.Update();

                    if (controlFlag == ControlUnitFlag.Halt)
                    {
                        var before = executionMode;
                        executionMode = CPUMode.Wait;
                        interruptHandle.Reset();
                        try
                        {
                            interruptHandle.Wait(token);
                        }
                        catch (OperationCanceledException)
                        {
                            return;
                        }
                        executionMode = before;
                    }

                    try
                    {
                        simulationHandle.Wait(token);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }

                    if (token.IsCancellationRequested) return;

                    if (executionMode == CPUMode.Step)
                    {
                        simulationHandle.Reset();
                    }
                    if (CU.Update(controlFlag))
                    {
                        break;
                    }
                }

                long end = Stopwatch.GetTimestamp();
                Latency = (end - start) / (double)Stopwatch.Frequency;
            }
        }

        public void Dispose()
        {
            CU.Dispose();
            MMU.Dispose();
            ALU.Dispose();
            RAM.Dispose();
            ROM.Dispose();
            foreach (var register in Registers)
            {
                register.Dispose();
            }
            simulationHandle.Dispose();
            interruptHandle.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}