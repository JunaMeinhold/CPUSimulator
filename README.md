# CPU Simulator

A high-performance CPU simulator with custom assembly language support, built with .NET 9 and featuring a modern UI powered by ImGui.

## Overview

CPU Simulator is an educational and experimental project that implements a complete processor simulation, including:

- **Custom CPU Architecture** - Complete processor implementation with ALU, Control Unit, Memory Management Unit, and register file
- **Assembly Language** - Custom assembly language with lexer, parser, and assembler
- **Memory System** - RAM, ROM, and MMU with configurable memory mapping
- **Visual Debugging** - Real-time visualization of CPU state, registers, and memory
- **Microcode Execution** - Step-by-step instruction execution with microcode-level control
- **Cross-Platform** - Runs on Windows, Linux, and macOS

## Features

### Processor Components

- **Control Unit (CU)** - Manages instruction fetch, decode, and execution cycle
- **Arithmetic Logic Unit (ALU)** - Performs arithmetic and logical operations
- **Memory Management Unit (MMU)** - Handles memory addressing and access control
- **Register File** - General-purpose and special-purpose registers with sub-register support
- **Bus System** - Input/Output buses for data transfer between components
- **Interrupt Controller** - Hardware interrupt handling system
- **Video Device** - Memory-mapped video output device with command-based interface

### Assembly Language

The simulator includes a custom assembly language with:

- **Lexical Analysis** - Token-based parsing with keyword, operator, and number recognition
- **Instruction Set** - Support for data movement, arithmetic, logic, control flow, and stack operations
- **Sections** - `.text`, `.data`, `.rodata`, and `.bss` sections
- **Directives** - Data definition (`db`, `dw`, `dd`, `dq`), string literals, alignment, and space reservation
- **Labels & Symbols** - Label definitions and references with automatic resolution
- **Addressing Modes** - Register, immediate, and memory addressing

### User Interface

- **Code Editor** - Syntax highlighting for assembly code with Cascadia Mono font
- **Register View** - Real-time display of all CPU registers
- **Memory Inspector** - View and modify RAM/ROM contents
- **Step Execution** - Step through instructions one at a time or run continuously
- **Debug Tools** - Built-in debugging capabilities
- **Video Output** - Memory-mapped video device with 160x90 framebuffer support
- **Example Programs** - Sample assembly programs demonstrating features

## Technical Details

### Architecture

```
┌─────────────────────────────────────────────┐
│              Control Unit (CU)              │
│  - Instruction Pointer                      │
│  - Flags Register                           │
│  - Interrupt Handling                       │
└────────────┬────────────────────────────────┘
             │
┌────────────┴────────────────────────────────┐
│                  Buses                      │
│  - X Bus, Y Bus, Z Bus                      │
│  - MAR/MDR Buses                            │
└────────────┬────────────────────────────────┘
             │
┌────────────┼────────────────────────────────┐
│            │                                │
│    ┌───────▼────────┐    ┌────────────┐     │
│    │  Register File │    │    MMU     │     │
│    │  - 16+ Regs    │    │ ┌────────┐ │     │
│    │  - Sub-regs    │    │ │  RAM   │ │     │
│    └───────┬────────┘    │ │  ROM   │ │     │
│            │             │ └────────┘ │     │
│    ┌───────▼────────┐    └────────────┘     │
│    │      ALU       │                       │
│    │  - Arithmetic  │                       │
│    │  - Logic Ops   │                       │
│    │  - Flags       │                       │
│    └────────────────┘                       │
└─────────────────────────────────────────────┘
```

### Key Technologies

- **.NET 9** - Latest .NET runtime with Native AOT support
- **C# 13** - Modern C# with unsafe code for performance
- **Hexa.NET.KittyUI** - ImGui-based UI framework
- **Hexa.NET.ImGui.Widgets** - ImGui widget extensions
- **Hexa.NET.ImGui.Widgets.Extras** - Additional UI components including TextEditor
- **Hexa.NET.Utilities** - High-performance utility libraries
- **Microsoft.CodeAnalysis** - Roslyn compiler platform
- **Unsafe Code** - Pointer arithmetic and unmanaged memory for optimal performance

### Performance Features

- **Native AOT Compilation** - Ahead-of-time compilation for maximum performance
- **Unsafe Memory Management** - Direct memory manipulation with pointers
- **Radix Tree** - Optimized data structure for fast keyword/symbol lookup in the lexer
- **Zero-allocation parsing** - Efficient token streaming with minimal allocations
- **Microcode execution** - Low-level control over instruction execution

## Building

### Prerequisites

- .NET 9 SDK or later
- Supported platforms: Windows, Linux, macOS

### Build Steps

```bash
# Clone the repository
git clone https://github.com/JunaMeinhold/CPUSimulator.git
cd CPUSimulator

# Build the project
dotnet build

# Run the simulator
dotnet run --project CPUSimulator/CPUSimulator.csproj
```

### Build Configuration

The project supports:
- **Platforms**: AnyCPU, x64
- **Native AOT**: Enabled for optimized performance
- **Unsafe Blocks**: Enabled for low-level memory operations

## Usage

### Writing Assembly Code

```asm
section .text
    org 16384           ; Set base address

main:
    mov rax, 10         ; Load immediate value
    mov rbx, 20
    add rax, rbx        ; Add registers
    mov [result], rax   ; Store to memory
    halt                ; Stop execution

section .data
result: dq 0            ; Define qword variable
message: dbs "Hello"    ; Define string
```

### Supported Instructions

- **Data Movement**: `MOV`, `PUSH`, `POP`, `LEA`
- **Arithmetic**: `ADD`, `SUB`, `MUL`, `DIV`, `INC`, `DEC`, `NEG`
- **Logic**: `AND`, `OR`, `XOR`, `NOT`
- **Control Flow**: `JMP`, `CALL`, `RET`, `JE`, `JNE`, `JG`, `JGE`, `JL`, `JLE`, `JAE`, `JB`, `JBE`, `JC`, `JCXZ`, `JNZ`, `LOOP`, `LOOPNE`
- **Comparison**: `CMP`, `TEST`, `CMPS`, `CMPSB`, `CMPSW`
- **Interrupt Control**: `INT`, `CLI`, `STI`, `IRET`
- **Special**: `HLT` (halt), `NOP` (no operation)

### Running Programs

1. Write your assembly code in the editor
2. Click "Assemble" to compile the code
3. Use "Step" to execute one instruction at a time
4. Use "Run" to execute continuously
5. Monitor registers and memory in real-time

## Project Structure

```
CPUSimulator/
├── Core/
│   ├── Assembly/           # Assembler components
│   │   ├── Lexical/        # Lexer and tokenization
│   │   ├── Assembler.cs    # Main assembler
│   │   ├── InstructionParser.cs
│   │   ├── RadixTree.cs    # Fast lookup data structure
│   │   ├── TokenStream.cs  # Token streaming
│   │   └── Section.cs      # Assembly section management
│   ├── Buses/              # Bus system implementation
│   ├── Decoding/           # Instruction decoding
│   │   ├── Instructions/   # Instruction implementations
│   │   ├── Decoder.cs      # Instruction decoder
│   │   └── OpCode.cs       # Opcode definitions
│   ├── Devices/            # Hardware device implementations
│   │   └── VideoDevice.cs  # Memory-mapped video device
│   ├── Memory/             # Memory subsystem
│   │   ├── Register.cs     # Register implementation
│   │   ├── RandomAccessMemory.cs
│   │   └── ReadonlyMemory.cs
│   ├── Processor.cs        # Main CPU implementation
│   ├── ControlUnit.cs      # Control unit
│   ├── ArithmeticLogicalUnit.cs
│   ├── MemoryManagementUnit.cs
│   ├── InterruptController.cs
│   ├── Microcode.cs        # Microcode definitions
│   └── MicrocodeBuilder.cs # Microcode generation
├── MainWindow.cs           # Main UI window
├── VideoMonitor.cs         # Video output display
├── MemoryView.cs           # Memory inspection view
├── Asmx86SyntaxHighlight.cs # Assembly syntax highlighting
├── Program.cs              # Application entry point
├── example/                # Example assembly programs
│   ├── screen.asm          # Video device demo
│   └── ascii.asm           # ASCII demo
└── CPUSimulator.csproj     # Project file
```

## Architecture Details

### Memory Map

- **0x0000 - 0x3FFF** (0-16383): Stack space (RAM)
- **0x4000 - 0x4FFF** (16384-20479): ROM (read-only, 4KB)
- **0x5000 - 0x104FFF** (20480-1069055): RAM (read-write, ~1MB)
- **0x105000+** (1069056+): Video Device Control Memory (40 bytes)
  - Command register, parameters, and framebuffer address

### Register Set

The simulator implements a comprehensive register file with support for full, half, and byte-sized sub-registers (similar to x86-64 architecture).

### Execution Modes

- **Running**: Continuous execution
- **Step**: Single-step through instructions
- **Halt**: Paused execution
- **Wait**: Waiting for interrupt

## Contributing

Contributions are welcome! This is an educational project aimed at understanding CPU architecture and assembly language implementation.

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/JunaMeinhold/CPUSimulator/blob/master/LICENSE.txt) file for more details.

## Acknowledgments

- Built with [Hexa.NET.KittyUI](https://github.com/HexaEngine/Hexa.NET.KittyUI)
- Uses [ImGui](https://github.com/ocornut/imgui) for UI rendering
- Font: [Cascadia Mono](https://github.com/microsoft/cascadia-code) embedded for code display

## Author

**JunaMeinhold** - [GitHub Profile](https://github.com/JunaMeinhold)
