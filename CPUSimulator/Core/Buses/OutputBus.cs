namespace CPUSimulator.Core.Buses
{
    /// <summary>
    /// Multiple Outputs, one Input.
    /// </summary>
    public class OutputBus : IBus
    {
        private readonly bool[] states;

        public OutputBus(IBusInput input, params IBusOutput[] outputs)
        {
            Input = input;
            Outputs = outputs;
            states = new bool[outputs.Length];
        }

        public readonly IBusOutput[] Outputs;

        public readonly IBusInput Input;

        public void Push()
        {
            int i = 0;
            foreach (IBusOutput output in Outputs)
            {
                if (states[i])
                {
                    output.CopyFrom(Input.Value);
                }
                i++;
            }
        }

        public unsafe void UpdateState(bool* states, int len)
        {
            bool* end = states + len;
            int i = 0;
            while (states != end)
            {
                this.states[i++] = *states++;
            }
        }

        public unsafe void UpdateState(byte index, int count)
        {
            if (index > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    states[i] = i == index - 1;
                }
            }
            else
            {
                Array.Clear(states, 0, count);
            }
        }
    }
}