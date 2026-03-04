namespace CPUSimulator
{
    using Hexa.NET.ImGui;
    using Hexa.NET.ImGui.Widgets;
    using Hexa.NET.ImGui.Widgets.Dialogs;
    using System;
    using System.Globalization;

    public class JumpDialog : Dialog
    {
        private string value = string.Empty;
        public ulong Address;
        public ulong Max;
        public ulong Min;
        public JumpMode Mode;

        public enum JumpMode
        {
            Hexadecimal,
            Decimal,
        }

        public override string Name { get; } = "Jump";

        protected override ImGuiWindowFlags Flags { get; } = ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking;

        protected override unsafe void DrawContent()
        {
            ComboEnumHelper<JumpMode>.Combo("Mode", ref Mode);

            if (ImGui.InputText("Address", ref value, 1024, ImGuiInputTextFlags.CharsDecimal | ImGuiInputTextFlags.CharsHexadecimal))
            {
            }

            if (ImGui.Button("Cancel"))
            {
                Close(DialogResult.Cancel);
            }

            ImGui.SameLine();

            if (ImGui.Button("Jump") && ulong.TryParse(value, Mode == JumpMode.Hexadecimal ? NumberStyles.HexNumber : NumberStyles.Integer, CultureInfo.CurrentCulture, out var result))
            {
                Address = Math.Clamp(result, Min, Max);
                Close(DialogResult.Ok);
            }
        }
    }
}