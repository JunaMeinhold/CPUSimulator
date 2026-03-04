namespace CPUSimulator.Core.Buses
{
    using System.Buffers.Binary;

    /// <summary>
    /// One Input, One Output.
    /// </summary>
    public class SingleBus<TIn, TOut> : IBus where TIn : IBusInput where TOut : IBusOutput
    {
        private bool state;
        private readonly ulong mask;

        public SingleBus(TIn input, TOut output, ulong mask = ulong.MaxValue)
        {
            Input = input;
            Output = output;
            this.mask = mask;
        }

        public TIn Input { get; }

        public TOut Output { get; }

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

    public class SingleFlagBus<TIn, TOut> : IBus where TIn : IBusInput where TOut : IBusOutput
    {
        private bool state;
        private readonly ulong mask;

        public SingleFlagBus(TIn input, TOut output, ulong mask = ulong.MaxValue)
        {
            Input = input;
            Output = output;
            this.mask = mask;
        }

        public TIn Input { get; }

        public TOut Output { get; }

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