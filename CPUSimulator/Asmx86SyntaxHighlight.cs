namespace CPUSimulator
{
    using Hexa.NET.ImGui.Widgets.Extras.TextEditor;
    using Hexa.NET.Utilities;
    using System;

    public unsafe class AsmX8664SyntaxHighlight : SyntaxHighlight
    {
        public AsmX8664SyntaxHighlight(string name, string pattern) : base(name, pattern)
        {
        }

        protected override void FindMatches(StdWString* text, Span<char> span)
        {
        }
    }
}