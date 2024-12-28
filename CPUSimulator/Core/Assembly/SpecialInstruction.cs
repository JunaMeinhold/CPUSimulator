namespace CPUSimulator.Core.Assembly
{
    public enum SpecialInstruction
    {
        ORG,

        DB,  // Define byte
        DW,  // Define word
        DD,  // Define double-word
        DQ,  // Define quad-word
        RESB, // Reserve bytes
        RESW, // Reserve words
        RESD, // Reserve double-words
        RESQ, // Reserve quad-words

        DBS, // Define null-terminated string

        ALIGN, // Align to a specified boundary

        GLOBAL, // Defines Entrypoint
    }
}