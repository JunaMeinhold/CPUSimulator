namespace CPUSimulator.Core.Decoding
{
    using CPUSimulator.Core;
    using CPUSimulator.Core.Decoding.Instructions;

    public class Decoder
    {
        public static DecodeResult Decode(string code)
        {
            var microcodes = new List<Microcode>();
            var blocks = new List<ParseBlock>();
            var lines = code.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            byte i = 0;
            foreach (string line in lines)
            {
                var block = new ParseBlock(line, i);
                blocks.Add(block);
                if (block.ParseObjects.Count == 0) continue;
                switch (block.Param0.Instruction)
                {
                    case OpCode.MOV:
                        MoveData.MOV(block);
                        break;

                    case OpCode.ADD:
                        Add.ADD(block);
                        break;

                    case OpCode.SUB:
                        Substract.SUB(block);
                        break;

                    case OpCode.MUL:
                        Multiplication.MUL(block);
                        break;

                    case OpCode.DIV:
                        Division.DIV(block);
                        break;

                    case OpCode.INC:
                        Increment.INC(block);
                        break;

                    case OpCode.DEC:
                        Decrement.DEC(block);
                        break;

                    case OpCode.CMP:
                        Compare.CMP(block);
                        break;

                    case OpCode.JMP:
                        Jump.JMP(block);
                        break;

                    case OpCode.JE:
                        Jump.JE(block);
                        break;

                    case OpCode.JG:
                        Jump.JG(block);
                        break;

                    case OpCode.JL:
                        Jump.JL(block);
                        break;

                    case OpCode.JGE:
                        Jump.JGE(block);
                        break;

                    case OpCode.JLE:
                        Jump.JLE(block);
                        break;
                }

                while (block.Instructions.Count < 4)
                {
                    block.Add(new MicrocodeBuilder().SetNextAddress(block.NextAddress).Build());
                }
                i++;
            }
            foreach (ParseBlock parseBlock in blocks)
            {
                microcodes.AddRange(parseBlock.Instructions);
            }
            return new DecodeResult(blocks, [.. microcodes]);
        }
    }
}