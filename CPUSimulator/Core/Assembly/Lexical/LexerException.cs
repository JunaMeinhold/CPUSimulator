namespace CPUSimulator.Core.Assembly.Lexical
{
    using System;

    [Serializable]
    internal class LexerException : Exception
    {
        private string v;
        private unsafe SourceText* source;
        private uint idx;
        private unsafe byte* pCur;

        public LexerException()
        {
        }

        public LexerException(string? message) : base(message)
        {
        }

        public LexerException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public unsafe LexerException(string v, SourceText* source, uint idx, byte* pCur)
        {
            this.v = v;
            this.source = source;
            this.idx = idx;
            this.pCur = pCur;
        }
    }
}