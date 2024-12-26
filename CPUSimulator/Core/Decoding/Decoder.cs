namespace CPUSimulator.Core.Decoding
{
    using CPUSimulator.Core;
    using CPUSimulator.Core.Assembly;
    using CPUSimulator.Core.Decoding.Instructions;

    public static class Decoder
    {
        public static DecodeResult Decode(AssemblyResult result)
        {
            Microcode[] microcodes = new Microcode[result.Instructions.Length * 4];

            for (int i = 0; i < result.Instructions.Length; i++)
            {
                var instruction = result.Instructions[i];
                var block = Decode(instruction, i + 1);
                int baseIdx = i * 4;
                for (int j = 0; j < 4; j++)
                {
                    microcodes[baseIdx + j] = block.Block[j];
                }
            }

            return new DecodeResult(microcodes);
        }

        public static DecodeBlock Decode(Instruction instruction, int next)
        {
            DecodeBlock block = new();

            switch (instruction.OpCode)
            {
                case OpCode.MOV:
                    MoveData.MOV(ref block, instruction, next);
                    break;

                case OpCode.ADD:
                    MathOp(ref block, instruction, next, ALUFunction.Addition);
                    break;

                case OpCode.SUB:
                    MathOp(ref block, instruction, next, ALUFunction.Substraction);
                    break;

                case OpCode.MUL:
                    MathOp(ref block, instruction, next, ALUFunction.Multiplication);
                    break;

                case OpCode.DIV:
                    MathOp(ref block, instruction, next, ALUFunction.Division);
                    break;

                case OpCode.INC:
                    SingleOp(ref block, instruction, next, ALUFunction.Increment);
                    break;

                case OpCode.DEC:
                    SingleOp(ref block, instruction, next, ALUFunction.Decrement);
                    break;

                case OpCode.CMP:
                    Compare.CMP(ref block, instruction, next);
                    break;

                case OpCode.JMP:
                    Jump.JMP(ref block, instruction, next);
                    break;

                case OpCode.JE:
                    Jump.JE(ref block, instruction, next);
                    break;

                case OpCode.JG:
                    Jump.JG(ref block, instruction, next);
                    break;

                case OpCode.JL:
                    Jump.JL(ref block, instruction, next);
                    break;

                case OpCode.JGE:
                    Jump.JGE(ref block, instruction, next);
                    break;

                case OpCode.JLE:
                    Jump.JLE(ref block, instruction, next);
                    break;
            }

            while (block.Index < 4)
            {
                block.Add(new MicrocodeBuilder().SetNextAddress(next).Build());
            }

            return block;
        }

        public static void SingleOp(ref DecodeBlock block, Instruction instruction, int next, ALUFunction function)
        {
            if (instruction.IsRegister1)
            {
                MicrocodeBuilder builder = new();
                block.Add(builder
                    .SetNextAddress(next)
                    .SetALUFunction(function)
                    .SetXBus(instruction.RegisterName1)
                    .SetZBus(instruction.RegisterName1)
                    .Build());
            }
        }

        public static void MathOp(ref DecodeBlock block, Instruction instruction, int next, ALUFunction function)
        {
            MicrocodeBuilder builder = new MicrocodeBuilder().SetNextAddress(next).SetALUFunction(function);

            if (instruction.IsRegister1 && instruction.IsInterm2)
            {
                block.Add(builder.SetXBus(instruction.RegisterName1).SetZBus(instruction.RegisterName1).Build(instruction.Operand2));
            }
            if (instruction.IsRegister1 && instruction.IsRegister2)
            {
                block.Add(builder
                   .SetXBus(instruction.RegisterName1)
                   .SetYBus(instruction.RegisterName2)
                   .SetZBus(instruction.RegisterName1)
                   .Build());
            }
        }
    }
}