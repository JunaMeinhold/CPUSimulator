namespace CPUSimulator.Core
{
    public enum ControlUnitFlag
    {
        None = 0,
        Step,
        StepBack,
        Jump,
        Equals,
        Greater,
        Less,
        NotEquals,
        Call,
        Return,
        InterruptReturn,
        InterruptSetFlag,
        InterruptClearFlag,
        Push,
        Pop,
        JumpAbs,
        CallAbs,
        Halt,
    }
}