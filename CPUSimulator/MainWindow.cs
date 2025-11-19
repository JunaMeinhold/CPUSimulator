namespace CPUSimulator
{
    using CPUSimulator.Core;
    using CPUSimulator.Core.Assembly;
    using CPUSimulator.Core.Memory;
    using Hexa.NET.ImGui;
    using Hexa.NET.ImGui.Widgets;
    using Hexa.NET.ImGui.Widgets.Dialogs;
    using Hexa.NET.ImGui.Widgets.Extras.TextEditor;
    using Hexa.NET.KittyUI.ImGuiBackend;
    using Hexa.NET.Logging;
    using Hexa.NET.Utilities.Text;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Buffers.Binary;
    using System.Diagnostics;
    using System.Globalization;
    using System.Net;
    using System.Numerics;
    using System.Reflection.Emit;

    public class MainWindow : ImWindow
    {
        private string? path;
        private string text = @"
section .text
;org 16384

start:
    mov rsp, 16384
    mov rax, 10
    call func
	mov rcx, 16384
	mov [rcx+1], rbx
    hlt

func:
    mov rbx, 200
    ret

";

        private Task? task;
        private CancellationTokenSource cancellationTokenSource = new();
        private readonly Processor processor = new();
        private readonly Assembler assembler = new();
        private MemoryViewSettings ramViewSettings = new();
        private MemoryViewSettings romViewSettings = new();

        private readonly TextEditorTab textEditor = new("New File", new TextSource(string.Empty));

        public override string Name { get; } = "CPU Simulator";

        public MainWindow()
        {
            IsEmbedded = true;
        }

        public override unsafe void DrawContent()
        {
            bool running = task != null && !task.IsCompleted;
            if (running)
            {
                if (ImGui.Button("Stop"))
                {
                    Stop();
                }
            }
            else
            {
                if (ImGui.Button("Execute"))
                {
                    Execute();
                }
            }
            ImGui.SameLine();
            if (running)
            {
                bool stepMode = processor.Mode == CPUMode.Step;
                if (stepMode)
                {
                    if (ImGui.Button("Step"))
                    {
                        processor.Step();
                    }
                }
                else
                {
                    bool halted = processor.Mode == CPUMode.Halt;
                    if (halted)
                    {
                        if (ImGui.Button("Resume"))
                        {
                            processor.Resume();
                        }
                    }
                    else
                    {
                        if (ImGui.Button("Halt"))
                        {
                            processor.Halt();
                        }
                    }
                }
            }
            else
            {
                bool stepMode = processor.Mode == CPUMode.Step;
                if (ImGui.Checkbox("Step mode", ref stepMode))
                {
                    processor.StepMode(stepMode);
                }
            }

            if (running)
            {
                ImGui.SameLine();
                ComboEnumHelper<CPUMode>.Text(processor.Mode);
            }

            if (ImGui.BeginTabBar("##TextEditor"))
            {
                var avail = ImGui.GetContentRegionAvail();
                //ImGuiManager.PushFont("CascadiaMono", 17);
                //textEditor.Font = ImGui.GetFont();
                //textEditor
                //ImGuiManager.PopFont();

                ImGui.InputTextMultiline("##d", ref text, 1024, avail, ImGuiInputTextFlags.AllowTabInput);

                if (ImGui.Shortcut((int)(ImGuiKey.ModCtrl | ImGuiKey.A)))
                {
                    textEditor?.SelectAll();
                }

                if (ImGui.Shortcut((int)(ImGuiKey.ModCtrl | ImGuiKey.Z)))
                {
                    textEditor?.Undo();
                }

                if (ImGui.Shortcut((int)(ImGuiKey.ModCtrl | ImGuiKey.Y)))
                {
                    textEditor?.Redo();
                }

                if (ImGui.Shortcut((int)(ImGuiKey.ModCtrl | ImGuiKey.F)))
                {
                    textEditor?.ShowFind();
                }

                ImGui.EndTabBar();
            }

            DisplayRegisters();

            DisplayMemory("RAM", processor.RAM, processor.MMU, ref ramViewSettings);
            DisplayMemory("ROM", processor.ROM, processor.MMU, ref romViewSettings);
            DisplayStack(processor.RAM, processor.Registers[(int)RegisterAddress.RSP - 1], 16384);
        }

        public unsafe void DisplayALU(ArithmeticLogicalUnit alu)
        {
            if (!ImGui.Begin("ALU"))
            {
                return;
            }

            byte* buf = stackalloc byte[2048];
            StrBuilder builder = new(buf, 2048);

            var pos = ImGui.GetCursorScreenPos();
            var size = ImGui.GetContentRegionAvail();

            var draw = ImGui.GetWindowDrawList();
            // Define ALU dimensions
            float width = Math.Min(size.X, 200.0f);  // Cap the width for a consistent size
            float height = Math.Min(size.Y, 120.0f); // Cap the height for a consistent size

            // Calculate ALU vertices relative to `pos`
            Vector2 topLeft = new Vector2(pos.X + (size.X - width) / 2, pos.Y + (size.Y - height) / 2);
            Vector2 topRight = new Vector2(topLeft.X + width, topLeft.Y);
            Vector2 bottomLeft = new Vector2(topLeft.X + width / 3, topLeft.Y + height);
            Vector2 bottomRight = new(topLeft.X + 2 * width / 3, topLeft.Y + height);

            // Draw the ALU shape
            draw.AddLine(topLeft, topRight, ImGui.GetColorU32(ImGuiCol.Text), 2.0f);
            draw.AddLine(topRight, bottomRight, ImGui.GetColorU32(ImGuiCol.Text), 2.0f);
            draw.AddLine(bottomRight, bottomLeft, ImGui.GetColorU32(ImGuiCol.Text), 2.0f);
            draw.AddLine(bottomLeft, topLeft, ImGui.GetColorU32(ImGuiCol.Text), 2.0f);

            {
                // Add text label inside the ALU
                string label = ComboEnumHelper<ALUFunction>.GetName(alu.Function);
                Vector2 textSize = ImGui.CalcTextSize(label);
                Vector2 textPos = new(
                    topLeft.X + (width - textSize.X) / 2,
                    topLeft.Y + (height - textSize.Y) / 2
                );
                draw.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), label);
            }
            {
                ulong value = alu.XRegister.GetValueUInt64();
                builder.Reset();
                builder.Append("X: 0x");
                builder.AppendHex(value, false, true);
                builder.End();

                Vector2 textSize = ImGui.CalcTextSize(builder);
                Vector2 textPos = new(
                    topLeft.X,
                    topLeft.Y - textSize.Y
                );
                draw.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), builder);
            }
            {
                ulong value = alu.YRegister.GetValueUInt64();
                builder.Reset();
                builder.Append("Y: 0x");
                builder.AppendHex(value, false, true);
                builder.End();

                Vector2 textSize = ImGui.CalcTextSize(builder);
                Vector2 textPos = new(
                    topRight.X - textSize.X,
                    topLeft.Y - textSize.Y
                );
                draw.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), builder);
            }
            {
                ulong value = alu.ZRegister.GetValueUInt64();
                builder.Reset();
                builder.Append("Z: 0x");
                builder.AppendHex(value, false, true);
                builder.End();

                Vector2 textSize = ImGui.CalcTextSize(builder);
                Vector2 textPos = new(
                    topLeft.X + (width - textSize.X) / 2,
                    bottomLeft.Y
                );
                draw.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), builder);
            }

            ImGui.End();
        }

        public unsafe void DisplayRegisters()
        {
            byte* buf = stackalloc byte[2048];
            StrBuilder builder = new(buf, 2048);

            if (ImGui.Begin("Registers"))
            {
                ImGuiTableFlags flags =
                ImGuiTableFlags.Reorderable |
                ImGuiTableFlags.Resizable |
                ImGuiTableFlags.Hideable |
                ImGuiTableFlags.SizingFixedFit |
                ImGuiTableFlags.ScrollX |
                ImGuiTableFlags.ScrollY |
                ImGuiTableFlags.PadOuterX | ImGuiTableFlags.ContextMenuInBody | ImGuiTableFlags.NoSavedSettings;
                if (ImGui.BeginTable("Registers", 2, flags))
                {
                    ImGui.TableSetupColumn("Register");
                    ImGui.TableSetupColumn("Value");
                    ImGui.TableHeadersRow();

                    bool isOpen = false;
                    ImGui.PushStyleColor(ImGuiCol.Header, 0x0);
                    foreach (var register in processor.Registers)
                    {
                        var isParent = !RegisterHelper.GetParentRegister(register.Address).HasValue;
                        var hasChildren = RegisterHelper.HasChildren(register.Address);

                        if (isParent)
                        {
                            if (isOpen)
                            {
                                ImGui.Separator();
                                ImGui.TreePop();
                            }
                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            ImGui.Spacing();

                            if (hasChildren)
                            {
                                isOpen = ImGui.TreeNodeEx(register.DebugName, ImGuiTreeNodeFlags.SpanAllColumns);
                            }
                            else
                            {
                                isOpen = false;
                                ImGui.TreeNodeEx(register.DebugName, ImGuiTreeNodeFlags.SpanAllColumns | ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen);
                            }
                        }
                        else
                        {
                            if (!isOpen)
                                continue;
                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            ImGui.Text(register.DebugName);
                        }

                        ImGui.TableSetColumnIndex(1);
                        ImGui.Spacing();

                        builder.Reset();

                        for (int i = register.Value.Length - 1; i >= 0; i--)
                        {
                            var value = register.Value[i];
                            builder.Index += Utf8Formatter.FormatHex(value, builder.Buffer + builder.Index, builder.Count - builder.Index, true, true);
                        }
                        builder.End();
                        ImGui.Text(builder);
                        TooltipValue(builder, register.GetValueUInt64());
                    }

                    if (isOpen)
                    {
                        ImGui.TreePop();
                    }

                    ImGui.PopStyleColor();

                    ImGui.EndTable();
                }

                ImGui.End();
            }
        }

        private static unsafe void TooltipValue(StrBuilder builder, ulong value, bool reverseEndianness = false)
        {
            if (ImGui.BeginItemTooltip())
            {
                if (reverseEndianness)
                {
                    value = BinaryPrimitives.ReverseEndianness(value);
                }
                builder.Reset(); builder.Append("Binary: "u8);
                builder.AppendBinary(value, 64, 0);
                builder.End(); ImGui.Text(builder);

                builder.Reset(); builder.Append("Hexadecimal: 0x"u8);
                builder.AppendHex(value, true, true);
                builder.End(); ImGui.Text(builder);

                builder.Reset(); builder.Append("Decimal (Unsigned): "u8);
                builder.Append(value);
                builder.End(); ImGui.Text(builder);

                builder.Reset(); builder.Append("Decimal (Signed): "u8);
                builder.Append(*(long*)&value);
                builder.End(); ImGui.Text(builder);

                builder.Reset(); builder.Append("Float: "u8);
                builder.Append(*(float*)&value);
                builder.End(); ImGui.Text(builder);

                builder.Reset(); builder.Append("Double: "u8);
                builder.Append(*(double*)&value);
                builder.End(); ImGui.Text(builder);
                ImGui.EndTooltip();
            }
        }

        private JumpRequest? jumpToAddress;

        private struct JumpRequest
        {
            public ulong Address;
            public string Id;

            public JumpRequest(ulong address, string id)
            {
                Address = address;
                Id = id;
            }
        }

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

        public struct MemoryViewSettings
        {
            public uint BytesWidth;

            public MemoryViewSettings()
            {
                BytesWidth = 4;
            }
        }

        public unsafe void DisplayMemory(string label, IMemory memory, MemoryManagementUnit mmu, ref MemoryViewSettings settings)
        {
            if (!ImGui.Begin(label, ImGuiWindowFlags.MenuBar))
            {
                return;
            }

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.MenuItem("Jump"u8))
                {
                    JumpDialog dialog = new()
                    {
                        Max = memory.Size,
                        Userdata = label
                    };
                    dialog.Show((sender, result) =>
                    {
                        if (result == DialogResult.Ok && sender is JumpDialog jump)
                        {
                            jumpToAddress = new(jump.Address, (string)jump.Userdata!);
                        }
                    });
                }
                if (ImGui.BeginMenu("View"u8))
                {
                    if (ImGui.MenuItem("1 Byte"u8))
                    {
                        settings.BytesWidth = 1;
                    }
                    if (ImGui.MenuItem("2 Bytes"u8))
                    {
                        settings.BytesWidth = 2;
                    }
                    if (ImGui.MenuItem("4 Bytes"u8))
                    {
                        settings.BytesWidth = 4;
                    }
                    if (ImGui.MenuItem("8 Bytes"u8))
                    {
                        settings.BytesWidth = 8;
                    }
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }

            byte* buf = stackalloc byte[2048];
            StrBuilder builder = new(buf, 2048);

            ImGuiTableFlags flags =
            ImGuiTableFlags.Reorderable |
            ImGuiTableFlags.Resizable |
            ImGuiTableFlags.Hideable |
            ImGuiTableFlags.SizingFixedFit |
            ImGuiTableFlags.ScrollX |
            ImGuiTableFlags.ScrollY |
            ImGuiTableFlags.PadOuterX | ImGuiTableFlags.ContextMenuInBody | ImGuiTableFlags.NoSavedSettings;

            ulong marValue = mmu.MAR.GetValueUInt64();

            bool highlight = memory.Range.InRange(marValue);
            marValue -= memory.Range.Start;
            marValue += memory.Range.PhysicalOffset;

            var avail = ImGui.GetContentRegionAvail();

            uint busWidth = mmu.BusWidth switch
            {
                RAMBusWidth.Bits8 => 1,
                RAMBusWidth.Bits16 => 2,
                RAMBusWidth.Bits32 => 4,
                RAMBusWidth.Bits64 => 8,
                _ => 0,
            };

            uint byteWidth = settings.BytesWidth;

            if (ImGui.BeginTable(label, 1 + (int)byteWidth, flags))
            {
                ImGui.TableSetupColumn("Address");
                for (int i = 0; i < byteWidth; i++)
                {
                    builder.Reset();
                    builder.Append("0x");
                    builder.Append(i);
                    builder.End();
                    ImGui.TableSetupColumn(builder);
                }
                ImGui.TableSetupScrollFreeze(0, 1);
                ImGui.TableHeadersRow();

                float lineHeight = ImGui.GetTextLineHeightWithSpacing();

                if (jumpToAddress.HasValue && jumpToAddress.Value.Id == label)
                {
                    int item = (int)(jumpToAddress.Value.Address / byteWidth);
                    ImGui.SetScrollY(item * lineHeight);
                    jumpToAddress = default;
                }

                float scroll = ImGui.GetScrollY();
                int start = (int)Math.Floor(scroll / lineHeight) - 1;
                int end = (int)Math.Ceiling(avail.Y / lineHeight) + start + 1;

                int maxItems = (int)memory.Size / (int)byteWidth;

                start = Math.Max(start, 0);
                end = Math.Min(end, maxItems);

                if (start > 0)
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Dummy(new(1, start * lineHeight));
                }

                var draw = ImGui.GetWindowDrawList();

                byte* buffer = stackalloc byte[8];

                for (int i = start; i < end; i++)
                {
                    ulong baseAddress = (uint)i * byteWidth;

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    builder.Reset();
                    builder.Append("0x"u8);
                    builder.AppendHex(baseAddress, true, true);
                    builder.End();
                    ImGui.Text(builder);

                    for (int x = 0; x < byteWidth; x++)
                    {
                        ulong realAddress = baseAddress + (uint)x;
                        byte data = memory.Data[realAddress];
                        ImGui.TableSetColumnIndex(1 + x);
                        builder.Reset();
                        builder.AppendHex(data, true, true);
                        builder.End();
                        Vector2 startCur = ImGui.GetCursorScreenPos();
                        if (highlight && realAddress >= marValue && realAddress < marValue + busWidth)
                        {
                            Vector2 endCur = startCur + ImGui.CalcTextSize(builder);
                            Vector2 delta = endCur - startCur;
                            draw.AddRectFilled(startCur, endCur, ImGui.GetColorU32(ImGuiCol.TextSelectedBg));
                        }
                        ImGui.Text(builder);
                        if (ImGui.BeginItemTooltip())
                        {
                            byte* pData = memory.Data + realAddress;
                            byte* pEnd = memory.Data + memory.Size;
                            int length = (int)(pEnd - pData);
                            int buffered = Math.Min(length, 8);
                            Memcpy(pData, buffer, buffered);
                            bool bigEndian = ImGui.IsKeyDown(ImGuiKey.LeftCtrl);
                            if (bigEndian)
                            {
                                ImGui.Text("Big-Endian"u8);
                            }
                            else
                            {
                                ImGui.Text("Little-Endian"u8);
                            }
                            if (length >= 1)
                            {
                                ImGui.SeparatorText("8 Bit"u8);
                                ImGui.Text(builder.MakeHex("Hex: "u8, buffer, 1, true, true, true, bigEndian));
                                ImGui.Text(builder.MakeDecimal("Dec: "u8, buffer, 1, false, bigEndian));
                                ImGui.Text(builder.MakeDecimal("Dec (signed): "u8, buffer, 1, true, bigEndian));
                            }
                            if (length >= 2)
                            {
                                ImGui.SeparatorText("16 Bit"u8);
                                ImGui.Text(builder.MakeHex("Hex: "u8, buffer, 2, true, true, true, bigEndian));
                                ImGui.Text(builder.MakeDecimal("Dec: "u8, buffer, 2, false, bigEndian));
                                ImGui.Text(builder.MakeDecimal("Dec (signed): "u8, buffer, 2, true, bigEndian));
                            }
                            if (length >= 4)
                            {
                                ImGui.SeparatorText("32 Bit"u8);
                                ImGui.Text(builder.MakeHex("Hex: "u8, buffer, 4, true, true, true, bigEndian));
                                ImGui.Text(builder.MakeDecimal("Dec: "u8, buffer, 4, false, bigEndian));
                                ImGui.Text(builder.MakeDecimal("Dec (signed): "u8, buffer, 4, true, bigEndian));
                            }
                            if (length >= 8)
                            {
                                ImGui.SeparatorText("64 Bit"u8);
                                ImGui.Text(builder.MakeHex("Hex: "u8, buffer, 8, true, true, true, bigEndian));
                                ImGui.Text(builder.MakeDecimal("Dec: "u8, buffer, 8, false, bigEndian));
                                ImGui.Text(builder.MakeDecimal("Dec (signed): "u8, buffer, 8, true, bigEndian));
                            }

                            ImGui.TextDisabled("Hold (ctrl) for Big-Endian"u8);
                            ImGui.EndTooltip();
                        }
                    }
                }

                int diffEnd = maxItems - end;
                if (diffEnd > 0)
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Dummy(new(1, diffEnd * lineHeight));
                }

                ImGui.EndTable();
            }
            ImGui.End();
        }

        public unsafe void DisplayStack(IMemory memory, Register rsp, int size)
        {
            if (!ImGui.Begin("Stack"))
            {
                return;
            }

            byte* buf = stackalloc byte[2048];
            StrBuilder builder = new(buf, 2048);

            ulong rspValue = rsp.GetValueUInt64();

            ImGuiTableFlags flags =
            ImGuiTableFlags.Reorderable |
            ImGuiTableFlags.Resizable |
            ImGuiTableFlags.Hideable |
            ImGuiTableFlags.SizingFixedFit |
            ImGuiTableFlags.ScrollX |
            ImGuiTableFlags.ScrollY |
            ImGuiTableFlags.PadOuterX | ImGuiTableFlags.ContextMenuInBody;

            var avail = ImGui.GetContentRegionAvail();

            const int addressWidth = 8;

            if (ImGui.BeginTable("Stack", 3, flags))
            {
                ImGui.TableSetupColumn("Address");
                ImGui.TableSetupColumn("Value");
                ImGui.TableSetupColumn("Symbol");
                ImGui.TableSetupScrollFreeze(0, 1);
                ImGui.TableHeadersRow();

                float lineHeight = ImGui.GetTextLineHeightWithSpacing();
                float scroll = ImGui.GetScrollY();
                int start = (int)Math.Floor(scroll / lineHeight) - 1;
                int end = (int)Math.Ceiling(avail.Y / lineHeight) + start + 1;

                int maxItems = size / addressWidth;

                start = Math.Max(start, 0);
                end = Math.Min(end, maxItems);

                if (start > 0)
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Dummy(new(1, start * lineHeight));
                }

                var draw = ImGui.GetWindowDrawList();

                for (uint i = (uint)start; i < end; i++)
                {
                    ulong baseAddress = (ulong)(size - (i + 1) * 8);

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    builder.Reset();
                    builder.Append("0x"u8);
                    builder.AppendHex(baseAddress, true, true);
                    builder.End();
                    ImGui.Text(builder);
                    TooltipValue(builder, baseAddress);
                    ImGui.TableSetColumnIndex(1);

                    ulong stackvalue = BinaryPrimitives.ReadUInt64LittleEndian(new ReadOnlySpan<byte>(memory.Data + baseAddress, 8)) - assembler.BaseAddress;

                    builder.Reset();
                    for (int j = addressWidth - 1; j >= 0; j--)
                    {
                        builder.AppendHex(memory.Data[baseAddress + (uint)j], true, true);
                    }
                    builder.End();
                    Vector2 startCur = ImGui.GetCursorScreenPos();
                    if (memory.CanWrite && baseAddress >= rspValue && baseAddress < rspValue + addressWidth)
                    {
                        Vector2 endCur = startCur + ImGui.CalcTextSize(builder);
                        Vector2 delta = endCur - startCur;
                        draw.AddRectFilled(startCur, endCur, ImGui.GetColorU32(ImGuiCol.TextSelectedBg));
                    }
                    ImGui.Text(builder);

                    ImGui.TableSetColumnIndex(2);
                    if (assembler.Symbols.TryGetValue(stackvalue, out var symbol))
                    {
                        builder.Reset();
                        builder.Append(symbol);
                        builder.End();
                        ImGui.Text(builder);
                    }
                    else
                    {
                        ImGui.Text("<unknown>"u8);
                    }
                }

                int diffEnd = maxItems - end;
                if (diffEnd > 0)
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Dummy(new(1, diffEnd * lineHeight));
                }

                ImGui.EndTable();
            }

            ImGui.End();
        }

        /*
        public unsafe void DisplayRom(ReadonlyMemory memory)
        {
            if (!ImGui.Begin("ROM"))
            {
                return;
            }

            byte* buf = stackalloc byte[2048];
            StrBuilder builder = new(buf, 2048);

            ImGuiTableFlags flags =
            ImGuiTableFlags.Reorderable |
            ImGuiTableFlags.Resizable |
            ImGuiTableFlags.Hideable |
            ImGuiTableFlags.SizingFixedFit |
            ImGuiTableFlags.ScrollX |
            ImGuiTableFlags.ScrollY |
            ImGuiTableFlags.PadOuterX | ImGuiTableFlags.ContextMenuInBody;

            var avail = ImGui.GetContentRegionAvail();

            if (ImGui.BeginTable("ROM", 12, flags))
            {
                ImGui.TableSetupColumn("Address");
                ImGui.TableSetupColumn("MC");
                ImGui.TableSetupColumn("MCNext");
                ImGui.TableSetupColumn("CC");
                ImGui.TableSetupColumn("ALU-Mode");
                ImGui.TableSetupColumn("ALU-FC");
                ImGui.TableSetupColumn("X-Bus");
                ImGui.TableSetupColumn("Y-Bus");
                ImGui.TableSetupColumn("Z-Bus");
                ImGui.TableSetupColumn("IO-RAM");
                ImGui.TableSetupColumn("RAM-Mode");
                ImGui.TableSetupColumn("RAM-Bus-Width");
                ImGui.TableSetupScrollFreeze(0, 1);
                ImGui.TableHeadersRow();

                float lineHeight = ImGui.GetTextLineHeightWithSpacing();
                float scroll = ImGui.GetScrollY();
                int start = (int)Math.Floor(scroll / lineHeight) - 1;
                int end = (int)Math.Ceiling(avail.Y / lineHeight) + start + 1;

                int maxItems = memory.Size;

                start = Math.Max(start, 0);
                end = Math.Min(end, maxItems);

                if (start > 0)
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Dummy(new(1, start * lineHeight));
                }

                for (int i = start; i < end; i++)
                {
                    uint baseAddress = (uint)i;
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    builder.Reset();
                    builder.Append("0x"u8);
                    builder.AppendHex(baseAddress, true, true);
                    builder.End();
                    ImGui.Text(builder);
                    ImGui.TableSetColumnIndex(1);

                    builder.Reset();
                    Microcode data = memory.Data[baseAddress];
                    var c = data.Code;
                    builder.Reset(); builder.AppendBinary(c, MicrocodeFieldPositions.MC_BITS, MicrocodeFieldPositions.MC_SHIFT);
                    builder.End(); ImGui.Text(builder);
                    ImGui.TableSetColumnIndex(2);
                    builder.Reset(); builder.AppendBinary(c, MicrocodeFieldPositions.MCNEXT_BITS, MicrocodeFieldPositions.MCNEXT_SHIFT);
                    builder.End(); ImGui.Text(builder);
                    ImGui.TableSetColumnIndex(3);
                    builder.Reset(); builder.AppendBinary(c, MicrocodeFieldPositions.CC_BITS, MicrocodeFieldPositions.CC_SHIFT);
                    builder.End(); ImGui.Text(builder);

                    ImGui.TableSetColumnIndex(4);
                    builder.Reset(); builder.Append(ComboEnumHelper<ALUMode>.GetName(data.ALUMode));
                    builder.End(); ImGui.Text(builder);
                    if (ImGui.BeginItemTooltip())
                    {
                        builder.Reset(); builder.AppendBinary(c, MicrocodeFieldPositions.ALU_MODE_BITS, MicrocodeFieldPositions.ALU_MODE_SHIFT);
                        builder.End(); ImGui.Text(builder);
                        ImGui.EndTooltip();
                    }

                    ImGui.TableSetColumnIndex(5);
                    builder.Reset(); builder.Append(ComboEnumHelper<ALUFunction>.GetName(data.ALUFunction));
                    builder.End(); ImGui.Text(builder);
                    if (ImGui.BeginItemTooltip())
                    {
                        builder.Reset(); builder.AppendBinary(c, MicrocodeFieldPositions.ALU_FC_BITS, MicrocodeFieldPositions.ALU_FC_SHIFT);
                        builder.End(); ImGui.Text(builder);
                        ImGui.EndTooltip();
                    }

                    ImGui.TableSetColumnIndex(6);
                    builder.Reset(); builder.AppendBinary(c, MicrocodeFieldPositions.X_BUS_BITS, MicrocodeFieldPositions.X_BUS_SHIFT);
                    builder.End(); ImGui.Text(builder);
                    ImGui.TableSetColumnIndex(7);
                    builder.Reset(); builder.AppendBinary(c, MicrocodeFieldPositions.Y_BUS_BITS, MicrocodeFieldPositions.Y_BUS_SHIFT);
                    builder.End(); ImGui.Text(builder);
                    ImGui.TableSetColumnIndex(8);
                    builder.Reset(); builder.AppendBinary(c, MicrocodeFieldPositions.Z_BUS_BITS, MicrocodeFieldPositions.Z_BUS_SHIFT);
                    builder.End(); ImGui.Text(builder);
                    ImGui.TableSetColumnIndex(9);
                    builder.Reset(); builder.AppendBinary(c, MicrocodeFieldPositions.IO_RAM_BITS, MicrocodeFieldPositions.IO_RAM_SHIFT);
                    builder.End(); ImGui.Text(builder);
                    ImGui.TableSetColumnIndex(10);
                    builder.Reset(); builder.AppendBinary(c, MicrocodeFieldPositions.RAM_MODE_BITS, MicrocodeFieldPositions.RAM_MODE_SHIFT);
                    builder.End(); ImGui.Text(builder);
                    ImGui.TableSetColumnIndex(11);
                    builder.Reset(); builder.AppendBinary(c, MicrocodeFieldPositions.RAM_BUS_WIDTH_BITS, MicrocodeFieldPositions.RAM_BUS_WIDTH_SHIFT);
                    builder.End(); ImGui.Text(builder);
                }

                int diffEnd = maxItems - end;
                if (diffEnd > 0)
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Dummy(new(1, diffEnd * lineHeight));
                }

                ImGui.EndTable();
            }
            ImGui.End();
        }
        */

        private void Stop()
        {
            cancellationTokenSource.Cancel();
        }

        private unsafe void Execute()
        {
            cancellationTokenSource = new();
            task = Task.Run(() =>
            {
                try
                {

                    var assemblyResult = assembler.Assemble(text);

                    processor.Reset();
                    assemblyResult.Write(processor.ROM.AsSpan());
                    processor.Execute(cancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    LoggerFactory.General.Log(ex);
                    if (Debugger.IsAttached)
                    {
                        throw;
                    }
                }
            });
        }
    }

    public static unsafe class Extensions
    {
        public static StrBuilder MakeDecimal(this ref StrBuilder builder, ReadOnlySpan<byte> label, byte* values, int count, bool signed, bool bigEndian)
        {
            if (bigEndian)
            {
                byte* stack = stackalloc byte[count];
                for (int i = 0; i < count; i++)
                {
                    stack[i] = values[count - i - 1];
                }
                values = stack;
            }
            builder.Reset();
            builder.Append(label);
            switch (count)
            {
                case 1:
                    if (signed) builder.Append(*(sbyte*)values); else builder.Append((ushort)*values);
                    break;

                case 2:
                    if (signed) builder.Append(*(short*)values); else builder.Append(*(ushort*)values);
                    break;

                case 4:
                    if (signed) builder.Append(*(int*)values); else builder.Append(*(uint*)values);
                    break;

                case 8:
                    if (signed) builder.Append(*(long*)values); else builder.Append(*(ulong*)values);
                    break;
            }
            builder.End();
            return builder;
        }

        public static StrBuilder MakeHex(this ref StrBuilder builder, ReadOnlySpan<byte> label, byte* values, int count, bool leadingZeros, bool uppercase, bool prefix, bool bigEndian)
        {
            if (bigEndian)
            {
                byte* stack = stackalloc byte[count];
                for (int i = 0; i < count; i++)
                {
                    stack[i] = values[count - i - 1];
                }
                values = stack;
            }
            builder.Reset();
            builder.Append(label);
            if (prefix)
            {
                builder.Append("0x"u8);
            }
            builder.AppendHex(values, count, leadingZeros, uppercase);
            builder.End();
            return builder;
        }

        public static void AppendHex(this ref StrBuilder builder, byte* values, int count, bool leadingZeros, bool uppercase)
        {
            for (int i = count - 1; i >= 0; i--)
            {
                builder.AppendHex(values[i], leadingZeros, uppercase);
            }
        }

        public static void AppendHexBigEndian(this ref StrBuilder builder, byte* values, int count, bool leadingZeros, bool uppercase)
        {
            for (int i = 0; i < count; i++)
            {
                builder.AppendHex(values[i], leadingZeros, uppercase);
            }
        }

        public static void AppendHex(this ref StrBuilder builder, byte value, bool leadingZeros, bool uppercase)
        {
            builder.Index += Utf8Formatter.FormatHex(value, builder.Buffer + builder.Index, builder.Count - builder.Index, leadingZeros, uppercase);
        }

        public static void AppendHex(this ref StrBuilder builder, int value, bool leadingZeros, bool uppercase)
        {
            value = BinaryPrimitives.ReverseEndianness(value);
            builder.Index += Utf8Formatter.FormatHex(value, builder.Buffer + builder.Index, builder.Count - builder.Index, leadingZeros, uppercase);
        }

        public static void AppendHex(this ref StrBuilder builder, uint value, bool leadingZeros, bool uppercase)
        {
            value = BinaryPrimitives.ReverseEndianness(value);
            builder.Index += Utf8Formatter.FormatHex(value, builder.Buffer + builder.Index, builder.Count - builder.Index, leadingZeros, uppercase);
        }

        public static void AppendHex(this ref StrBuilder builder, long value, bool leadingZeros, bool uppercase)
        {
            value = BinaryPrimitives.ReverseEndianness(value);
            builder.Index += Utf8Formatter.FormatHex(value, builder.Buffer + builder.Index, builder.Count - builder.Index, leadingZeros, uppercase);
        }

        public static void AppendHex(this ref StrBuilder builder, ulong value, bool leadingZeros, bool uppercase)
        {
            value = BinaryPrimitives.ReverseEndianness(value);
            builder.Index += Utf8Formatter.FormatHex(value, builder.Buffer + builder.Index, builder.Count - builder.Index, leadingZeros, uppercase);
        }

        public static void AppendBinary(this ref StrBuilder builder, ulong value, int bits, int offset)
        {
            int capacity = builder.Count - builder.Index;
            if (capacity < bits + 1)
            {
                return;
            }

            if (bits <= 0 || offset < 0 || offset + bits > 64)
            {
                return;
            }

            byte* buf = builder.Buffer + builder.Index;
            for (int i = 0; i < bits; i++)
            {
                int bitOffset = offset + i;
                byte bit = (byte)((value >> bitOffset) & 0x1);
                buf[bits - i - 1] = (byte)('0' + bit);
            }
            buf[bits] = 0;
            builder.Index += bits;
        }

        public static void AppendBinary(this ref StrBuilder builder, byte value, int bits, int offset)
        {
            int capacity = builder.Count - builder.Index;
            if (capacity < bits + 1)
            {
                return;
            }

            if (bits <= 0 || offset < 0 || offset + bits > 64)
            {
                return;
            }

            byte* buf = builder.Buffer + builder.Index;
            for (int i = 0; i < bits; i++)
            {
                int bitOffset = offset + i;
                byte bit = (byte)((value >> bitOffset) & 0x1);
                buf[bits - i - 1] = (byte)('0' + bit);
            }
            buf[bits] = 0;
            builder.Index += bits;
        }
    }
}