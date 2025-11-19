namespace CPUSimulator.Core.Assembly
{
    using Hexa.NET.Utilities;

    public struct Label
    {
        public StringSpan Name;
        public ulong Offset;
        public SectionType Section;

        public Label(StringSpan name, ulong offset, SectionType section)
        {
            Name = name;
            Offset = offset;
            Section = section;
        }
    }
}