namespace CPUSimulator
{
    using CPUSimulator.Core;
    using CPUSimulator.Core.Memory;
    using Hexa.NET.ImGui;
    using Hexa.NET.ImGui.Widgets.Dialogs;
    using Hexa.NET.KittyUI.ImGuiBackend;
    using Hexa.NET.Utilities.Text;
    using System;
    using System.Numerics;

    public class MemoryView
    {
        private JumpRequest? jumpToAddress;
        public uint BytesWidth;

        public MemoryView()
        {
            BytesWidth = 4;
        }

        private void HandleDialog(object? sender, DialogResult result)
        {
            if (result == DialogResult.Ok && sender is JumpDialog jump)
            {
                jumpToAddress = new(jump.Address, (string)jump.Userdata!);
            }
        }

        public unsafe void DisplayMemory(string label, IMemory memory, MemoryManagementUnit mmu)
        {
            if (!ImGui.Begin(label, ImGuiWindowFlags.MenuBar))
            {
                ImGui.End();
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
                    dialog.Show(HandleDialog);
                }
                if (ImGui.BeginMenu("View"u8))
                {
                    if (ImGui.MenuItem("1 Byte"u8))
                    {
                        BytesWidth = 1;
                    }
                    if (ImGui.MenuItem("2 Bytes"u8))
                    {
                        BytesWidth = 2;
                    }
                    if (ImGui.MenuItem("4 Bytes"u8))
                    {
                        BytesWidth = 4;
                    }
                    if (ImGui.MenuItem("8 Bytes"u8))
                    {
                        BytesWidth = 8;
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

            ulong marValue = mmu.MAR.GetValue<ulong>();

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

            uint byteWidth = BytesWidth;

            if (ImGui.BeginTable(label, 1 + (int)byteWidth + 1, flags))
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
                ImGui.TableSetupColumn("Text"u8);
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
                byte* textBuffer = stackalloc byte[(int)(byteWidth + 1)];
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
                        textBuffer[x] = (data >= 32 && data <= 126) ? data : (byte)'.';
                        if (!ImGui.TableSetColumnIndex(1 + x))
                        {
                            continue;
                        }
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

                    if (ImGui.TableSetColumnIndex((int)(byteWidth + 1)))
                    {
                        ImGuiManager.PushFont("CascadiaMonoEven", 14);
                        ImGui.Text(textBuffer);
                        ImGuiManager.PopFont();
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

    }

}