namespace CPUSimulator.Core.Buses
{
    using Hexa.NET.KittyUI.UI.NodeEditor;
    using Newtonsoft.Json.Linq;
    using System.Buffers.Binary;

    /// <summary>
    /// One Input, One Output.
    /// </summary>
    public class SingleBus : IBus
    {
        private bool state;
        private readonly ulong mask;

        public SingleBus(IBusInput input, IBusOutput output, ulong mask = ulong.MaxValue)
        {
            Input = input;
            Output = output;
            this.mask = mask;
        }

        public IBusInput Input { get; }

        public IBusOutput Output { get; }

        public void Push()
        {
            if (state)
            {
                Output.CopyFrom(Input.Value);
            }
        }

        public bool State { get => state; }

        public unsafe void UpdateState(bool* states, int len, int offset = 0)
        {
            state = *states;
        }

        public void UpdateState(bool state)
        {
            this.state = state;
        }
    }

    public class SingleFlagBus : IBus
    {
        private bool state;
        private readonly ulong mask;

        public SingleFlagBus(IBusInput input, IBusOutput output, ulong mask = ulong.MaxValue)
        {
            Input = input;
            Output = output;
            this.mask = mask;
        }

        public IBusInput Input { get; }

        public IBusOutput Output { get; }

        public void Push()
        {
            if (state)
            {
                ulong flag = BinaryPrimitives.ReadUInt64LittleEndian(Input.Value);
                ulong value = BinaryPrimitives.ReadUInt64LittleEndian(Output.Value);
                value &= ~(ulong)ALUFlag.ALUFlagsMask;
                value |= flag & (ulong)ALUFlag.ALUFlagsMask;

                BinaryPrimitives.WriteUInt64LittleEndian(Output.Value, value);
            }
        }

        public bool State { get => state; }

        public unsafe void UpdateState(bool* states, int len, int offset = 0)
        {
            state = *states;
        }

        public void UpdateState(bool state)
        {
            this.state = state;
        }
    }
}