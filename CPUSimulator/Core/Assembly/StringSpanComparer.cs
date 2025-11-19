namespace CPUSimulator.Core.Assembly
{
    using Hexa.NET.Utilities;
    using System.Diagnostics.CodeAnalysis;

    public class StringSpanComparer : IEqualityComparer<StringSpan>
    {
        public static readonly StringSpanComparer Instance = new();

        public bool Equals(StringSpan x, StringSpan y)
        {
            return x.SequenceEqual(y);
        }

        public unsafe int GetHashCode([DisallowNull] StringSpan obj)
        {
            const ulong prime = 0x00000100000001b3;
            ulong hash = 0xcbf29ce484222325;
            for (uint i = 0; i < obj.Length; ++i)
            {
                hash ^= obj.Ptr[i];
                hash *= prime;
            }
            return (int)hash;
        }
    }
}