namespace CPUSimulator
{
    using CPUSimulator.Core;
    using CPUSimulator.Core.Assembly;
    using CPUSimulator.Core.Memory;
    using Hexa.NET.ImGui;
    using Hexa.NET.ImGui.Widgets;
    using Hexa.NET.ImGui.Widgets.Extras.TextEditor;
    using Hexa.NET.KittyUI.ImGuiBackend;
    using Hexa.NET.Logging;
    using Hexa.NET.Utilities;
    using Hexa.NET.Utilities.Text;
    using System;
    using System.Buffers.Binary;
    using System.Diagnostics;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public class MainWindow : ImWindow
    {
        private readonly string template = """
            section .text
            // org 16384

            start:
                mov rsp, 16384 // Stack Base address
                mov rax, 20480 // RAM Base address, jump to 0x4000 to see
                
                hlt

            section .data
            """;
        private readonly string? path;
        private StdString text;

        private Task? task;
        private CancellationTokenSource cancellationTokenSource = new();
        private readonly Processor processor = new();
        private readonly Assembler assembler = new();
        private MemoryView ramViewSettings = new();
        private MemoryView romViewSettings = new();
        private readonly VideoMonitor videoMonitor;

        private readonly TextEditorTab textEditor = new("New File", new TextSource(string.Empty));

        public override string Name { get; } = "CPU Simulator";

        public MainWindow()
        {
            IsEmbedded = true;
            videoMonitor = new VideoMonitor(processor.VideoDevice, processor.MMU);
            text = new(template);
            Flags |= ImGuiWindowFlags.MenuBar;
        }

        public override unsafe void DrawContent()
        {
            DrawMenuBar();
            bool running = task != null && !task.IsCompleted;
            if (running)
            {
                if (ImGui.Button("Stop"u8))
                {
                    Stop();
                }
            }
            else
            {
                if (ImGui.Button("Execute"u8))
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

                ImGui.SameLine();
                byte* buf = stackalloc byte[256];
                StrBuilder builder = new(buf, 256);
                builder.Append("Latency: "u8);
                if (processor.Latency < 1e-6)
                {
                    builder.Append(processor.Latency * 1_000_000_000);
                    builder.Append(" ns"u8);
                }
                else if (processor.Latency < 1e-3)
                {
                    builder.Append(processor.Latency * 1_000_000);
                    builder.Append(" µs"u8);
                }
                else
                {
                    builder.Append(processor.Latency * 1000);
                    builder.Append(" ms"u8);
                }
                builder.End();
                ImGui.Text(builder);
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
                ImGuiManager.PushFont("CascadiaMono", 17);
                ImGui.PushStyleColor(ImGuiCol.FrameBg, 0x00);
                //textEditor.Font = ImGui.GetFont();
                //textEditor

                var capacity = text.Capacity;
                if (capacity > 0) { ++capacity; }
                ImGui.InputTextMultiline("##d", text.CStr(), (nuint)capacity, avail, ImGuiInputTextFlags.AllowTabInput | ImGuiInputTextFlags.CallbackResize, TextCallback, Unsafe.AsPointer(ref text));
                ImGui.PopStyleColor();
                ImGuiManager.PopFont();

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

            ramViewSettings.DisplayMemory("RAM", processor.RAM, processor.MMU);
            romViewSettings.DisplayMemory("ROM", processor.ROM, processor.MMU);
            DisplayStack(processor.RAM, processor.Registers[(int)RegisterAddress.RSP - 1], 16384);

            DisplayALU(processor.ALU);
            ImGuiManager.PushFont("CascadiaMono", 17);
            videoMonitor.Draw();
            ImGuiManager.PopFont();
        }

        private void DrawMenuBar()
        {   
            if (!ImGui.BeginMenuBar())
            {
                ImGui.EndMenuBar();
                return;
            }

            if (ImGui.BeginMenu("File"u8))
            {
                if (ImGui.MenuItem("New"u8))
                {
                    text.Release();
                    text = new(template);
                }
                ImGui.EndMenu();
            }

            ImGui.EndMenuBar();
        }

        private unsafe int TextCallback(ImGuiInputTextCallbackData* data)
        {
            if (data->EventFlag != ImGuiInputTextFlags.CallbackResize) return 0;
            StdString* str = (StdString*)data->UserData;
            str->Resize(data->BufTextLen);
            data->Buf = str->CStr();
            return 0;
        }

        public unsafe void DisplayRegister(in Register register, ReadOnlySpan<byte> label, Vector2 pos, Vector2 anchor)
        {
            ulong value = register.GetValue<ulong>();
            byte* buf = stackalloc byte[256];
            StrBuilder builder = new(buf, 256);
            builder.Append("0x");
            builder.AppendHex(value, true, true);
            builder.End();
            const float padding = 4.0f;
            const float spacing = 4.0f;
            var id = ImGui.GetID(label);
            var labelSize = ImGui.CalcTextSize(label);
            var valueSize = ImGui.CalcTextSize(builder);
            var size = new Vector2(labelSize.X + valueSize.X + spacing, labelSize.Y) + new Vector2(padding * 2);
            pos -= anchor * size;
            ImRect rect = new(pos, pos + size);
            if (!ImGuiP.ItemAdd(rect, id))
            {
                return;
            }
            var draw = ImGui.GetWindowDrawList();
            float right = pos.X + labelSize.X + spacing;
            float bottom = pos.Y + size.Y;
            draw.AddQuadFilled(pos, new Vector2(right, pos.Y), new(right, bottom), new Vector2(pos.X, bottom), ImGui.GetColorU32(ImGuiCol.FrameBgActive));

            float rightEnd = pos.X + size.X;
            draw.AddQuadFilled(new(right, pos.Y), new Vector2(rightEnd, pos.Y), new(rightEnd, bottom), new Vector2(right, bottom), ImGui.GetColorU32(ImGuiCol.FrameBg));
            var textCol = ImGui.GetColorU32(ImGuiCol.Text);
            Vector2 textPos = pos + new Vector2(padding, padding);
            draw.AddText(textPos, textCol, label);
            textPos.X += labelSize.X + spacing;

            draw.AddText(textPos, textCol, builder);
        }

        public unsafe void DisplayALU(ArithmeticLogicalUnit alu)
        {
            if (!ImGui.Begin("ALU"))
            {
                ImGui.End();
                return;
            }

            byte* buf = stackalloc byte[2048];
            StrBuilder builder = new(buf, 2048);

            var pos = ImGui.GetCursorScreenPos();
            var size = ImGui.GetContentRegionAvail();

            var draw = ImGui.GetWindowDrawList();
            // Define ALU dimensions
            float width = Math.Min(size.X, 200.0f) * 2;  // Cap the width for a consistent size
            float height = Math.Min(size.Y, 120.0f) * 2; // Cap the height for a consistent size

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

            string label = ComboEnumHelper<ALUFunction>.GetName(alu.Function);
            Vector2 textSize = ImGui.CalcTextSize(label);
            Vector2 textPos = new(
                topLeft.X + (width - textSize.X) / 2,
                topLeft.Y + (height - textSize.Y) / 2
            );
            draw.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), label);

            DisplayRegister(alu.XRegister, "X"u8, new(topLeft.X, topLeft.Y), new(0.5f, 1));
            DisplayRegister(alu.YRegister, "Y"u8, new(topRight.X, topLeft.Y), new(0.5f, 1));
            DisplayRegister(alu.ZRegister, "Z"u8, new(topLeft.X + width * 0.5f, bottomLeft.Y), new(0.5f, 0));

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
                        register.GetValue<ulong>().TooltipValue(builder);
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

        public unsafe void DisplayStack(IMemory memory, in Register rsp, int size)
        {
            if (!ImGui.Begin("Stack"))
            {
                return;
            }

            byte* buf = stackalloc byte[2048];
            StrBuilder builder = new(buf, 2048);

            ulong rspValue = rsp.GetValue<ulong>();

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
                    baseAddress.TooltipValue(builder);
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
            var assemblyResult = assembler.Assemble(text);

            cancellationTokenSource = new();
            task = Task.Run(() =>
            {
                try
                {
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
}