namespace CPUSimulator.Core.Assembly
{
    using Hexa.NET.Utilities;

    public struct AssemblyResult
    {
        public UnsafeList<Section> Sections;

        public AssemblyResult(UnsafeList<Section> sections)
        {
            Sections = sections;
        }

        public void Write(Span<byte> bytes)
        {
            int idx = 0;
            int written = 0;
            foreach (var section in Sections)
            {
                if (section.BaseAddress != ulong.MaxValue)
                {
                    idx = (int)section.BaseAddress;
                }
                switch (section.Type)
                {
                    case SectionType.Text:
                        foreach (var instruction in section.Instructions)
                        {
                            idx += instruction.Write(bytes[idx..]);
                        }
                        break;

                    case SectionType.Data:
                    case SectionType.RoData:
                    case SectionType.Bss:
                        section.Data.AsSpan().CopyTo(bytes[idx..]);
                        idx += section.Data.Count;
                        break;
                }

                written = Math.Max(idx, written);
            }
        }
    }

    public readonly struct SectionComparer : IComparer<Section>
    {
        public static readonly SectionComparer Instance;

        public int Compare(Section x, Section y)
        {
            if (x.BaseAddress == y.BaseAddress)
            {
            }

            return x.BaseAddress.CompareTo(y.BaseAddress);
        }
    }
}