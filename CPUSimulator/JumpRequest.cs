namespace CPUSimulator
{
    public struct JumpRequest
    {
        public ulong Address;
        public string Id;

        public JumpRequest(ulong address, string id)
        {
            Address = address;
            Id = id;
        }
    }
}