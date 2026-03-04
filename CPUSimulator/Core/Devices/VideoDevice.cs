namespace CPUSimulator.Core.Devices
{
    using System.Buffers.Binary;
    using System.Runtime.InteropServices;

    public class VideoDevice
    {
        private MemoryManagementUnit mmu = null!;

        public enum CommandType
        {
            NoOperation = 0,
            Clear = 1,
            SetMode = 2,
            Flush = 3,
        }

        public byte[] DeviceControlMemory;

        public event CommandIssuedHandler? CommandIssued;

        public delegate void CommandIssuedHandler(CommandType command, ulong param1, ulong param2, ulong param3, ulong framebufferAddress);

        public VideoDevice()
        {
            /*
             * [8 bytes] CommandType
             * [8 bytes] Parameter 1
             * [8 bytes] Parameter 2
             * [8 bytes] Parameter 3
             * [8 bytes] Framebuffer Address
             */
            DeviceControlMemory = new byte[8 + 8 + 8 + 8 + 8];
        }

        public ulong Map(MemoryManagementUnit mmu, ulong baseOffset)
        {
            this.mmu = mmu;
            AddressRange range = new(baseOffset, (ulong)DeviceControlMemory.LongLength, 0);
            mmu.Map(range, MMUExecute);
            return (ulong)DeviceControlMemory.LongLength;
        }

        public void Execute()
        {
            ulong cmdRaw = BinaryPrimitives.ReadUInt64LittleEndian(DeviceControlMemory.AsSpan(0, 8));
            if (cmdRaw == 0) return;

            var cmd = (CommandType)cmdRaw;

            ulong p1 = BinaryPrimitives.ReadUInt64LittleEndian(DeviceControlMemory.AsSpan(8, 8));
            ulong p2 = BinaryPrimitives.ReadUInt64LittleEndian(DeviceControlMemory.AsSpan(16, 8));
            ulong p3 = BinaryPrimitives.ReadUInt64LittleEndian(DeviceControlMemory.AsSpan(24, 8));
            ulong fb = BinaryPrimitives.ReadUInt64LittleEndian(DeviceControlMemory.AsSpan(32, 8));

            DeviceControlMemory.AsSpan(0, 8).Clear();

            CommandIssued?.Invoke(cmd, p1, p2, p3, fb);
        }

        private unsafe void MMUExecute(ulong address, Span<byte> span, MMUAction action)
        {
            fixed (byte* reg = span)
            fixed (byte* mem = DeviceControlMemory)
            {
                ulong toCopy = Math.Min((ulong)span.Length, (ulong)DeviceControlMemory.LongLength - address);
                if (action == MMUAction.Write)
                {
                    NativeMemory.Copy(reg, mem + address, (nuint)toCopy);
                }
                else if (action == MMUAction.Read)
                {
                    NativeMemory.Copy(mem + address, reg, (nuint)toCopy);
                }
            }
        }
    }
}
