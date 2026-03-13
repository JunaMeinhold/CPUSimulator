namespace CPUSimulator.Core
{
    using CPUSimulator.Core.Decoding;
    using CPUSimulator.Core.Memory;

    public class RegisterHelper
    {
        public const int RegisterCount = 72;

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

                // R8 and sub-registers
                RegisterAddress.R8 => 8,
                RegisterAddress.R8D => 4,
                RegisterAddress.R8W => 2,
                RegisterAddress.R8B => 1,

                // R9 and sub-registers
                RegisterAddress.R9 => 8,
                RegisterAddress.R9D => 4,
                RegisterAddress.R9W => 2,
                RegisterAddress.R9B => 1,

                // R10 and sub-registers
                RegisterAddress.R10 => 8,
                RegisterAddress.R10D => 4,
                RegisterAddress.R10W => 2,
                RegisterAddress.R10B => 1,

                // R11 and sub-registers
                RegisterAddress.R11 => 8,
                RegisterAddress.R11D => 4,
                RegisterAddress.R11W => 2,
                RegisterAddress.R11B => 1,

                // R12 and sub-registers
                RegisterAddress.R12 => 8,
                RegisterAddress.R12D => 4,
                RegisterAddress.R12W => 2,
                RegisterAddress.R12B => 1,

                // R13 and sub-registers
                RegisterAddress.R13 => 8,
                RegisterAddress.R13D => 4,
                RegisterAddress.R13W => 2,
                RegisterAddress.R13B => 1,

                // R14 and sub-registers
                RegisterAddress.R14 => 8,
                RegisterAddress.R14D => 4,
                RegisterAddress.R14W => 2,
                RegisterAddress.R14B => 1,

                // R15 and sub-registers
                RegisterAddress.R15 => 8,
                RegisterAddress.R15D => 4,
                RegisterAddress.R15W => 2,
                RegisterAddress.R15B => 1,

                RegisterAddress.XMM0 => 16,
                RegisterAddress.XMM1 => 16,
                RegisterAddress.XMM2 => 16,
                RegisterAddress.XMM3 => 16,
                RegisterAddress.XMM4 => 16,
                RegisterAddress.XMM5 => 16,
                RegisterAddress.XMM6 => 16,
                RegisterAddress.XMM7 => 16,
                RegisterAddress.XMM8 => 16,
                RegisterAddress.XMM9 => 16,
                RegisterAddress.XMM10 => 16,
                RegisterAddress.XMM11 => 16,
                RegisterAddress.XMM12 => 16,
                RegisterAddress.XMM13 => 16,
                RegisterAddress.XMM14 => 16,
                RegisterAddress.XMM15 => 16,

                // Default case
                _ => throw new ArgumentOutOfRangeException(nameof(address), $"Unknown register: {address}")
            };
        }

        public static RegisterBank GetRegisterBank(RegisterAddress address)
        {
            return (RegisterBank)(((byte)address) >> 4);
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

                // R8 and sub-registers
                RegisterAddress.R8 => 0,
                RegisterAddress.R8D => 0,
                RegisterAddress.R8W => 0,
                RegisterAddress.R8B => 0,

                // R9 and sub-registers
                RegisterAddress.R9 => 0,
                RegisterAddress.R9D => 0,
                RegisterAddress.R9W => 0,
                RegisterAddress.R9B => 0,

                // R10 and sub-registers
                RegisterAddress.R10 => 0,
                RegisterAddress.R10D => 0,
                RegisterAddress.R10W => 0,
                RegisterAddress.R10B => 0,

                // R11 and sub-registers
                RegisterAddress.R11 => 0,
                RegisterAddress.R11D => 0,
                RegisterAddress.R11W => 0,
                RegisterAddress.R11B => 0,

                // R12 and sub-registers
                RegisterAddress.R12 => 0,
                RegisterAddress.R12D => 0,
                RegisterAddress.R12W => 0,
                RegisterAddress.R12B => 0,

                // R13 and sub-registers
                RegisterAddress.R13 => 0,
                RegisterAddress.R13D => 0,
                RegisterAddress.R13W => 0,
                RegisterAddress.R13B => 0,

                // R14 and sub-registers
                RegisterAddress.R14 => 0,
                RegisterAddress.R14D => 0,
                RegisterAddress.R14W => 0,
                RegisterAddress.R14B => 0,

                // R15 and sub-registers
                RegisterAddress.R15 => 0,
                RegisterAddress.R15D => 0,
                RegisterAddress.R15W => 0,
                RegisterAddress.R15B => 0,

                RegisterAddress.XMM0 => 0,
                RegisterAddress.XMM1 => 0,
                RegisterAddress.XMM2 => 0,
                RegisterAddress.XMM3 => 0,
                RegisterAddress.XMM4 => 0,
                RegisterAddress.XMM5 => 0,
                RegisterAddress.XMM6 => 0,
                RegisterAddress.XMM7 => 0,
                RegisterAddress.XMM8 => 0,
                RegisterAddress.XMM9 => 0,
                RegisterAddress.XMM10 => 0,
                RegisterAddress.XMM11 => 0,
                RegisterAddress.XMM12 => 0,
                RegisterAddress.XMM13 => 0,
                RegisterAddress.XMM14 => 0,
                RegisterAddress.XMM15 => 0,

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
                RegisterAddress.R8D or RegisterAddress.R8W or RegisterAddress.R8B => RegisterAddress.R8,
                RegisterAddress.R9D or RegisterAddress.R9W or RegisterAddress.R9B => RegisterAddress.R9,
                RegisterAddress.R10D or RegisterAddress.R10W or RegisterAddress.R10B => RegisterAddress.R10,
                RegisterAddress.R11D or RegisterAddress.R11W or RegisterAddress.R11B => RegisterAddress.R11,
                RegisterAddress.R12D or RegisterAddress.R12W or RegisterAddress.R12B => RegisterAddress.R12,
                RegisterAddress.R13D or RegisterAddress.R13W or RegisterAddress.R13B => RegisterAddress.R13,
                RegisterAddress.R14D or RegisterAddress.R14W or RegisterAddress.R14B => RegisterAddress.R14,
                RegisterAddress.R15D or RegisterAddress.R15W or RegisterAddress.R15B => RegisterAddress.R15,
                _ => null // No parent for other registers
            };
        }

        public static bool HasChildren(RegisterAddress address)
        {
            return address switch
            {
                RegisterAddress.RAX => true,
                RegisterAddress.RBX => true,
                RegisterAddress.RCX => true,
                RegisterAddress.RDX => true,
                RegisterAddress.R8 => true,
                RegisterAddress.R9 => true,
                RegisterAddress.R10 => true,
                RegisterAddress.R11 => true,
                RegisterAddress.R12 => true,
                RegisterAddress.R13 => true,
                RegisterAddress.R14 => true,
                RegisterAddress.R15 => true,
                _ => false
            };
        }
    }

    public static class EnumExtensions
    {
        public static int GetImmSize(this OperandSource source)
        {
            return source switch
            {
                OperandSource.Imm8 => 1,
                OperandSource.Imm16 => 2,
                OperandSource.Imm32 => 4,
                OperandSource.Imm64 => 8,
                OperandSource.ImmAddress => 8,
                _ => throw new ArgumentOutOfRangeException(nameof(source), $"Not an immediate source: {source}")
            };
        }

        public static int GetRAMBusWidthSize(this RAMBusWidth width)
        {
            return width switch
            {
                RAMBusWidth.Bits8 => 1,
                RAMBusWidth.Bits16 => 2,
                RAMBusWidth.Bits32 => 4,
                RAMBusWidth.Bits64 => 8,
                _ => throw new ArgumentOutOfRangeException(nameof(width), $"Unknown RAM bus width: {width}")
            };
        }
    }
}