using CPUSimulator.Core.Memory;

namespace CPUSimulator.Core
{
    using CPUSimulator.Core.Decoding;
    using System.Buffers.Binary;

    public class ControlUnit
    {
        private readonly MemoryManagementUnit mmu;
        private readonly ulong romStart;
        private readonly ulong ivtBase;
        public readonly Register RSP;
        public readonly Register RIP;
        public readonly Register IOP;
        public readonly Register RFlags;
        private uint nextOffset = 0;
        private uint lastOffset = 0;

        public ControlUnit(MemoryManagementUnit mmu, Register rsp, ulong romStart, ulong ivtBase)
        {
            RIP = new(8, "RIP");
            RFlags = new(8, "RFlags");
            IOP = new(8, "IOP");
            this.mmu = mmu;
            RSP = rsp;
            this.romStart = romStart;
            this.ivtBase = ivtBase;
        }

        public bool InterruptEnabled => (((ALUFlag)RFlags.GetValueUInt64()) & ALUFlag.InterruptEnableFlag) != 0;

        public unsafe Instruction Fetch()
        {
            ulong current = RIP.GetValueUInt64();
            Span<byte> buffer = stackalloc byte[sizeof(Instruction)];
            mmu.Execute(current, buffer, MMUAction.Read);
            Instruction instruction = default;
            lastOffset = nextOffset;
            nextOffset = (uint)instruction.Read(buffer);
            return instruction;
        }

        public unsafe bool Update(ControlUnitFlag controlFlag)
        {
            ulong current = RIP.GetValueUInt64();
            ulong iop = IOP.GetValueUInt64();
            ALUFlag flags = (ALUFlag)BinaryPrimitives.ReadUInt64LittleEndian(RFlags.Value);
            ulong next;
            bool result = true;

            switch (controlFlag)
            {
                default:
                    next = current;
                    result = false;
                    break;

                case ControlUnitFlag.Halt:
                case ControlUnitFlag.Step:
                    next = current + nextOffset;
                    break;

                case ControlUnitFlag.StepBack:
                    next = current - lastOffset;
                    break;

                case ControlUnitFlag.Jump:
                    next = ComputeNextRelative(current, iop);
                    break;

                case ControlUnitFlag.Equals:
                    if ((flags & ALUFlag.ZeroFlag) != 0)
                        next = ComputeNextRelative(current, iop);
                    else
                        next = current + nextOffset;
                    break;

                case ControlUnitFlag.Greater:
                    if ((flags & ALUFlag.CarryFlag) != 0) // Greater
                        next = ComputeNextRelative(current, iop);
                    else
                        next = current + nextOffset;
                    break;

                case ControlUnitFlag.Less:
                    if ((flags & ALUFlag.SignFlag) != 0) // Less
                        next = ComputeNextRelative(current, iop);
                    else
                        next = current + nextOffset;
                    break;

                case ControlUnitFlag.NotEquals:
                    if ((flags & ALUFlag.ZeroFlag) == 0)
                        next = ComputeNextRelative(current, iop);
                    else
                        next = current + nextOffset;
                    break;

                case ControlUnitFlag.Call:
                    {
                        ulong returnAddress = current + nextOffset;
                        ulong rsp = RSP.GetValueUInt64();
                        rsp -= sizeof(ulong);
                        RSP.SetValue(rsp);

                        Span<byte> buffer = stackalloc byte[8];
                        BinaryPrimitives.WriteUInt64LittleEndian(buffer, returnAddress);
                        mmu.Execute(rsp, buffer, MMUAction.Write);

                        next = ComputeNextRelative(current, iop);
                    }
                    break;

                case ControlUnitFlag.Return:
                    {
                        ulong rsp = RSP.GetValueUInt64();

                        Span<byte> buffer = stackalloc byte[8];
                        mmu.Execute(rsp, buffer, MMUAction.Read);
                        ulong returnAddress = BinaryPrimitives.ReadUInt64LittleEndian(buffer);

                        rsp += sizeof(ulong);
                        RSP.SetValue(rsp);

                        next = returnAddress;
                    }
                    break;

                case ControlUnitFlag.InterruptReturn:
                    {
                        ulong rsp = RSP.GetValueUInt64();
                        Span<byte> buffer = stackalloc byte[12];
                        mmu.Execute(rsp, buffer, MMUAction.Read);

                        // Restore CC
                        RFlags.CopyFrom(buffer[..4]);

                        // Restore RIP
                        mmu.Execute(rsp, buffer, MMUAction.Read);
                        next = BinaryPrimitives.ReadUInt64LittleEndian(buffer[4..]);
                        rsp += RIP.Size + RFlags.Size;

                        RSP.SetValue(rsp);
                    }
                    break;

                case ControlUnitFlag.InterruptSetFlag:
                    flags |= ALUFlag.InterruptEnableFlag;
                    RFlags.SetValue((ulong)flags);
                    goto case ControlUnitFlag.Step;

                case ControlUnitFlag.InterruptClearFlag:
                    flags &= ~ALUFlag.InterruptEnableFlag;
                    RFlags.SetValue((ulong)flags);
                    goto case ControlUnitFlag.Step;
            }

            RIP.SetValue(next);

            return result;
        }

        public unsafe void HandleInterrupt(int interruptNumber)
        {
            // Save the current state onto the stack
            ulong rsp = RSP.GetValueUInt64();
            rsp -= RIP.Size + RFlags.Size;

            Span<byte> buffer = stackalloc byte[12];
            RFlags.Value.CopyTo(buffer[..4]);
            RIP.Value.CopyTo(buffer[4..]);

            mmu.Execute(rsp, buffer, MMUAction.Write);

            RSP.SetValue(rsp);

            // Fetch the interrupt handler address from the IVT
            ulong ivtAddress = ivtBase + (ulong)interruptNumber * sizeof(ulong);
            mmu.Execute(ivtAddress, buffer[..8], MMUAction.Read);
            ulong handlerAddress = BinaryPrimitives.ReadUInt64LittleEndian(buffer);

            // Set RIP to the interrupt handler
            RIP.SetValue(handlerAddress);
        }

        private static unsafe ulong ComputeNextRelative(ulong current, ulong iop)
        {
            int signBit = (int)(iop >> 63 & 1);
            long value = *(long*)&iop;

            ulong next;
            if (signBit == 1)
            {
                next = current - (ulong)-value;
            }
            else
            {
                next = current + (ulong)value;
            }

            return next;
        }

        public void Reset()
        {
            RIP.Reset();
            RIP.SetValue(romStart);
            IOP.Reset();
            RFlags.Reset();
        }
    }
}