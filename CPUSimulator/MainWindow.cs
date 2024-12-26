namespace CPUSimulator
{
    using CPUSimulator.Core;
    using CPUSimulator.Core.Assembly;
    using CPUSimulator.Core.Decoding;
    using CPUSimulator.Core.Memory;
    using Hexa.NET.ImGui;
    using Hexa.NET.ImGui.Widgets;
    using Hexa.NET.KittyUI.ImGuiBackend;
    using Hexa.NET.Utilities.Text;
    using System;
    using System.Numerics;

    public class MainWindow : ImWindow
    {
        private string code = string.Empty;
        private string? path;

        private Task? task;
        private CancellationTokenSource cancellationTokenSource = new();
        private readonly Processor processor = new();

        protected override string Name { get; } = "CPU Simulator";

        public override unsafe void DrawContent()
        {
            if (task != null && !task.IsCompleted)
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
            var avail = ImGui.GetContentRegionAvail();
            ImGuiManager.PushFont("CascadiaMono");
            ImGui.InputTextMultiline("##Code", ref code, 1024, avail);
            ImGuiManager.PopFont();

            DisplayRegisters();

            DisplayRam(processor.RAM);
            DisplayRom(processor.ROM);
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

                    foreach (var register in processor.Registers)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(register.DebugName);
                        ImGui.TableSetColumnIndex(1);
                        builder.Reset();
                        foreach (byte b in register.Value)
                        {
                            builder.Index += Utf8Formatter.FormatHex(b, builder.Buffer + builder.Index, builder.Count - builder.Index, true, true);
                        }
                        builder.End();
                        ImGui.Text(builder);
                        if (ImGui.BeginItemTooltip())
                        {
                            builder.Reset(); builder.Append("Binary: "u8);
                            foreach (var b in register.Value)
                            {
                                builder.AppendBinary(b, 8, 0);
                            }
                            builder.End(); ImGui.Text(builder);

                            builder.Reset(); builder.Append("Decimal: "u8);
                            builder.Append(register.GetValueUInt64());
                            builder.End(); ImGui.Text(builder);
                            ImGui.EndTooltip();
                        }
                    }

                    ImGui.EndTable();
                }

                ImGui.End();
            }
        }

        public unsafe void DisplayRam(RandomAccessMemory memory)
        {
            if (!ImGui.Begin("RAM"))
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
            ImGuiTableFlags.PadOuterX | ImGuiTableFlags.ContextMenuInBody | ImGuiTableFlags.NoSavedSettings;

            ulong marValue = memory.MAR.GetValueUInt64();

            builder.Reset();
            builder.Append("MAR: 0x"u8);
            builder.AppendHex(marValue, true, true);
            builder.End();
            ImGui.Text(builder);

            builder.Reset();
            builder.Append("MDR: 0x"u8);
            builder.AppendHex(memory.MDR.GetValueUInt64(), true, true);
            builder.End();
            ImGui.Text(builder);

            ImGui.Text("State: "u8); ImGui.SameLine(); ComboEnumHelper<RAMMode>.Text(memory.Mode);
            ImGui.Text("Width: "u8); ImGui.SameLine(); ComboEnumHelper<RAMBusWidth>.Text(memory.BusWidth);

            var avail = ImGui.GetContentRegionAvail();

            uint busWidth = memory.BusWidth switch
            {
                RAMBusWidth.Bits8 => 1,
                RAMBusWidth.Bits16 => 2,
                RAMBusWidth.Bits32 => 4,
                RAMBusWidth.Bits64 => 8,
                _ => 0,
            };

            const int byteWidth = 4;

            if (ImGui.BeginTable("RAM", 1 + byteWidth, flags))
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
                float scroll = ImGui.GetScrollY();
                int start = (int)Math.Floor(scroll / lineHeight) - 1;
                int end = (int)Math.Ceiling(avail.Y / lineHeight) + start + 1;

                int maxItems = memory.Size / byteWidth;

                start = Math.Max(start, 0);
                end = Math.Min(end, maxItems);

                if (start > 0)
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Dummy(new(1, start * lineHeight));
                }

                var draw = ImGui.GetWindowDrawList();

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
                        byte data = processor.RAM.Data[realAddress];
                        ImGui.TableSetColumnIndex(1 + x);
                        builder.Reset();
                        builder.AppendHex(data, true, true);
                        builder.End();
                        Vector2 startCur = ImGui.GetCursorScreenPos();
                        if (realAddress >= marValue && realAddress < marValue + busWidth)
                        {
                            Vector2 endCur = startCur + ImGui.CalcTextSize(builder);
                            Vector2 delta = endCur - startCur;
                            draw.AddRectFilled(startCur, endCur, ImGui.GetColorU32(ImGuiCol.TextSelectedBg));
                        }
                        ImGui.Text(builder);
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
                    Microcode data = memory.Microcodes[baseAddress];
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

        private void Stop()
        {
            cancellationTokenSource.Cancel();
        }

        private void Execute()
        {
            cancellationTokenSource = new();
            task = Task.Run(() =>
            {
                var assemblyResult = Assembler.Assemble(code);
                var result = Decoder.Decode(assemblyResult);

                processor.Reset();
                processor.ROM.Microcodes = result.Microcodes;
                processor.Execute(cancellationTokenSource.Token);
            });
        }
    }

    public static unsafe class Extensions
    {
        public static void AppendHex(this ref StrBuilder builder, byte value, bool leadingZeros, bool uppercase)
        {
            builder.Index += Utf8Formatter.FormatHex(value, builder.Buffer + builder.Index, builder.Count - builder.Index, leadingZeros, uppercase);
        }

        public static void AppendHex(this ref StrBuilder builder, int value, bool leadingZeros, bool uppercase)
        {
            builder.Index += Utf8Formatter.FormatHex(value, builder.Buffer + builder.Index, builder.Count - builder.Index, leadingZeros, uppercase);
        }

        public static void AppendHex(this ref StrBuilder builder, uint value, bool leadingZeros, bool uppercase)
        {
            builder.Index += Utf8Formatter.FormatHex(value, builder.Buffer + builder.Index, builder.Count - builder.Index, leadingZeros, uppercase);
        }

        public static void AppendHex(this ref StrBuilder builder, long value, bool leadingZeros, bool uppercase)
        {
            builder.Index += Utf8Formatter.FormatHex(value, builder.Buffer + builder.Index, builder.Count - builder.Index, leadingZeros, uppercase);
        }

        public static void AppendHex(this ref StrBuilder builder, ulong value, bool leadingZeros, bool uppercase)
        {
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