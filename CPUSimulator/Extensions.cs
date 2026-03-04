namespace CPUSimulator
{
    using Hexa.NET.ImGui;
    using Hexa.NET.Utilities.Text;
    using System;
    using System.Buffers.Binary;

    public static unsafe class Extensions
    {

        public static void TooltipValue(this ulong value, StrBuilder builder, bool reverseEndianness = false)
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