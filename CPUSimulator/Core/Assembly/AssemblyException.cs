namespace CPUSimulator.Core.Assembly
{
    using System;

    [Serializable]
    internal class AssemblyException : Exception
    {
        public AssemblyException()
        {
        }

        public AssemblyException(string? message) : base(message)
        {
        }

        public AssemblyException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}