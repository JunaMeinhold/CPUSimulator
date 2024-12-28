namespace CPUSimulator.Core.Decoding
{
    using CPUSimulator.Core;
    using CPUSimulator.Core.Decoding.Instructions;

    public static class Decoder
    {
        public static IEnumerable<Microcode> Decode(Instruction instruction)
        {
            return instruction.OpCode switch
            {
                OpCode.MOV => MoveData.MOV(instruction),
                OpCode.ADD => MathOp(instruction, ALUFunction.Addition),
                OpCode.SUB => MathOp(instruction, ALUFunction.Substraction),
                OpCode.MUL => MathOp(instruction, ALUFunction.Multiplication),
                OpCode.DIV => MathOp(instruction, ALUFunction.Division),
                OpCode.INC => SingleOp(instruction, ALUFunction.Increment),
                OpCode.DEC => SingleOp(instruction, ALUFunction.Decrement),
                OpCode.CMP => Compare.CMP(instruction),
                OpCode.JMP => Jump.JMP(instruction),
                OpCode.JE => Jump.JE(instruction),
                OpCode.JG => Jump.JG(instruction),
                OpCode.JL => Jump.JL(instruction),
                OpCode.JGE => Jump.JGE(instruction),
                OpCode.JLE => Jump.JLE(instruction),
                OpCode.JNE => Jump.JNE(instruction),
                OpCode.JNZ => Jump.JNE(instruction),
                OpCode.CALL => CallReturn.Call(instruction),
                OpCode.RET => CallReturn.Return(),
                OpCode.PUSH => Stack.Push(instruction),
                OpCode.POP => Stack.Pop(instruction),
                OpCode.HLT => Halt.HALT(),
                OpCode.CLI => Interrupts.ClearInterruptFlag(),
                OpCode.STI => Interrupts.SetInterruptFlag(),
                _ => throw new NotImplementedException(),
            };
        }

        public static IEnumerable<Microcode> SingleOp(Instruction instruction, ALUFunction function)
        {
            if (instruction.IsRegister1)
            {
                MicrocodeBuilder builder = new();
                yield return builder
                    .SetMC(ControlUnitFlag.Step)
                    .SetCC(true)
                    .SetALUFunction(function)
                    .SetXBus(instruction.RegisterName1)
                    .SetZBus(instruction.RegisterName1)
                    .Build();
            }
        }

        public static IEnumerable<Microcode> MathOp(Instruction instruction, ALUFunction function)
        {
            MicrocodeBuilder builder = new MicrocodeBuilder().SetALUFunction(function);

            if (instruction.IsRegister1 && instruction.IsImm2)
            {
                yield return builder.SetMC(ControlUnitFlag.Step).SetXBus(instruction.RegisterName1).SetZBus(instruction.RegisterName1).Build(instruction.Immediate);
            }
            if (instruction.IsRegister1 && instruction.IsRegister2)
            {
                yield return builder
                   .SetMC(ControlUnitFlag.Step)
                   .SetCC(true)
                   .SetXBus(instruction.RegisterName1)
                   .SetYBus(instruction.RegisterName2)
                   .SetZBus(instruction.RegisterName1)
                   .Build();
            }
        }
    }
}