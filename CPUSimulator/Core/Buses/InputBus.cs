namespace CPUSimulator.Core.Buses
{
    /// <summary>
    /// Multiple Inputs, one Output.
    /// </summary>
    public class InputBus : IBus
    {
        private readonly bool[] states;

        public InputBus(IBusOutput output, params IBusInput[] inputs)
        {
            Output = output;
            Inputs = inputs;
            states = new bool[Inputs.Length];
        }

        public readonly IBusInput[] Inputs;

        public readonly IBusOutput Output;

        public void Push()
        {
            int i = 0;
            foreach (IBusInput input in Inputs)
            {
                if (states[i])
                {
                    Output.CopyFrom(input.Value);
                }
                i++;
            }
        }

        public unsafe void UpdateState(bool* states, int len, int offset = 0)
        {
            bool* end = states + len;
            int i = offset;
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