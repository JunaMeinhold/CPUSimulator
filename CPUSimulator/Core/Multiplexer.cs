namespace CPUSimulator.Core
{
    public class Multiplexer<TItem>
    {
        private readonly int numBankBits;
        private readonly int indexBits;
        private readonly int indexMask;
        private readonly int bankMask;

        private int MaxBankSize => 1 << numBankBits;

        private int MaxIndexSize => 1 << indexBits;

        private readonly List<Bank> banks = [];

        private struct Bank
        {
            public TItem[] Items;
        }

        public Multiplexer(int numBankBits, int indexBits)
        {
            this.numBankBits = numBankBits;
            this.indexBits = indexBits;
            bankMask = (1 << numBankBits) - 1;
            indexMask = (1 << indexBits) - 1;
        }

        public void AddBank(TItem[] items)
        {
            if (banks.Count >= MaxBankSize)
                throw new InvalidOperationException("Maximum number of banks exceeded.");
            if (items.Length > MaxIndexSize)
                throw new ArgumentException("Index bits insufficient for bank size.");
            banks.Add(new Bank { Items = items });
        }

        public TItem? Select(int index)
        {
            if (index == 0) return default;
            int bankIndex = (index >> indexBits) & bankMask;
            int itemIndex = index & indexMask;
            return banks[bankIndex].Items[itemIndex];
        }
    }
}
