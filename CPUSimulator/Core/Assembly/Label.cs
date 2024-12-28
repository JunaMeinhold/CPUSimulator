namespace CPUSimulator.Core.Assembly
{
    public struct Label
    {
        public string Name;
        public ulong Offset;
        public SectionType Section;

        public Label(string name, ulong offset, SectionType section)
        {
            Name = name;
            Offset = offset;
            Section = section;
        }
    }
}