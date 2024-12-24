namespace CPUSimulator.Core.Buses
{
    /// <summary>
    /// One Input, One Output.
    /// </summary>
    public class SingleBus : IBus
    {
        private bool state;

        public SingleBus(IBusInput input, IBusOutput output)
        {
            Input = input;
            Output = output;
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
}