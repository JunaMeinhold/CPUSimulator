namespace CPUSimulator.Core.Decoding
{
    using CPUSimulator.Core;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class ParseBlock
    {
        public List<ParseObject> ParseObjects;
        public List<Microcode> Instructions = new();
        public List<long> References = new();

        public string Source;

        public string? Comment;

        public byte NextAddress => (byte)(Address + 1);

        public byte Address;

        public ParseBlock(string input, byte address)
        {
            Address = address;
            Regex regex = new("//(?<=//)(.*)(?=)");
            Comment = regex.Match(input)?.Value ?? null;
            if (Comment is not null && Comment != string.Empty)
                Source = input.Replace(Comment, "");
            else
                Source = input;
            ParseObjects = input.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList().ConvertAll(p => new ParseObject(p));
        }

        public ParseObject Param0 => ParseObjects.Count >= 1 ? ParseObjects[0] : default;

        public ParseObject Param1 => ParseObjects.Count >= 2 ? ParseObjects[1] : default;

        public ParseObject Param2 => ParseObjects.Count >= 3 ? ParseObjects[2] : default;

        public void Add(Microcode microcode)
        {
            Instructions.Add(microcode);
        }
    }
}