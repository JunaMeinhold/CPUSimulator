namespace CPUSimulator.Core.Assembly.Lexical
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit)]
    public struct Number
    {
        [FieldOffset(0)]
        public byte U8;
        [FieldOffset(0)]
        public ushort U16;
        [FieldOffset(0)]
        public uint U32;
        [FieldOffset(0)]
        public ulong U64;
        [FieldOffset(0)]
        public sbyte I8;
        [FieldOffset(0)]
        public short I16;
        [FieldOffset(0)]
        public int I32;
        [FieldOffset(0)]
        public long I64;
        [FieldOffset(0)]
        public float F32;
        [FieldOffset(0)]
        public double F64;
    }
}
