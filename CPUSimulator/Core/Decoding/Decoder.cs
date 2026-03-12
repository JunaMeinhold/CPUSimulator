namespace CPUSimulator.Core.Decoding
{
    using CPUSimulator.Core;
    using CPUSimulator.Core.Decoding.Instructions;

    public static class Decoder
    {
        public static bool Decode(MicrocodeQueue queue, in Instruction instruction)
        {
            return instruction.OpCode switch
            {
                OpCode.NOP => NoOp(queue),
                OpCode.MOV => MoveData.MOV(queue, instruction),
                OpCode.ADD => MathOp(queue, instruction, ALUFunction.Addition),
                OpCode.SUB => MathOp(queue, instruction, ALUFunction.Substraction),
                OpCode.MUL => MathOp(queue, instruction, ALUFunction.Multiplication),
                OpCode.DIV => MathOp(queue, instruction, ALUFunction.Division),
                OpCode.INC => SingleOp(queue, instruction, ALUFunction.Increment),
                OpCode.DEC => SingleOp(queue, instruction, ALUFunction.Decrement),
                OpCode.TEST => Compare.TEST(queue, instruction),
                OpCode.CMP => Compare.CMP(queue, instruction),
                OpCode.JMP => Jump.JMP(queue, instruction),
                OpCode.JE => Jump.JE(queue, instruction),
                OpCode.JG => Jump.JG(queue, instruction),
                OpCode.JL => Jump.JL(queue, instruction),
                OpCode.JGE => Jump.JGE(queue, instruction),
                OpCode.JLE => Jump.JLE(queue, instruction),
                OpCode.JNE => Jump.JNE(queue, instruction),
                OpCode.JNZ => Jump.JNE(queue, instruction),
                OpCode.CALL => CallReturn.Call(queue, instruction),
                OpCode.RET => CallReturn.Return(queue),
                OpCode.PUSH => StackInstr.Push(queue, instruction),
                OpCode.POP => StackInstr.Pop(queue, instruction),
                OpCode.HLT => Halt.HALT(queue),
                OpCode.CLI => Interrupts.ClearInterruptFlag(queue),
                OpCode.STI => Interrupts.SetInterruptFlag(queue),
                OpCode.LEA => LoadEffectiveAddress.LEA(queue, instruction),
                _ => false,
            };
        }

        private static bool NoOp(MicrocodeQueue queue)
        {
            MicrocodeBuilder builder = new();
            queue.Enqueue(builder.SetMC(ControlUnitFlag.Step).SetALUFunction(ALUFunction.NoOperation).Build());
            return true;
        }

        public static bool SingleOp(MicrocodeQueue queue, in Instruction instruction, ALUFunction function)
        {
            if (instruction.IsRegister1)
            {
                MicrocodeBuilder builder = new();
                queue.Enqueue(builder
                    .SetMC(ControlUnitFlag.Step)
                    .SetCC(true)
                    .SetALUFunction(function)
                    .SetXBus(instruction.RegisterName1)
                    .SetZBus(instruction.RegisterName1)
                    .Build());
                return true;
            }
            return false;
        }

        public static bool MathOp(MicrocodeQueue queue, in Instruction instruction, ALUFunction function)
        {
            MicrocodeBuilder builder = new MicrocodeBuilder().SetALUFunction(function);

            if (instruction.IsRegister1 && instruction.IsImm2)
            {
                queue.Enqueue(builder.SetMC(ControlUnitFlag.Step).SetXBus(instruction.RegisterName1).SetZBus(instruction.RegisterName1).Build(instruction.Immediate2));
                return true;
            }

            if (instruction.IsRegister1 && instruction.IsRegister2)
            {
                queue.Enqueue(builder
                   .SetMC(ControlUnitFlag.Step)
                   .SetCC(true)
                   .SetXBus(instruction.RegisterName1)
                   .SetYBus(instruction.RegisterName2)
                   .SetZBus(instruction.RegisterName1)
                   .Build());
                return true;
            }

            return false;
        }
    }
}