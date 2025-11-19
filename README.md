# CPU Simulator

A high-performance CPU simulator with custom assembly language support, built with .NET 9 and featuring a modern UI powered by ImGui.

## Overview

CPU Simulator is an educational and experimental project that implements a complete processor simulation, including:

- **Custom CPU Architecture** - Complete processor implementation with ALU, Control Unit, Memory Management Unit, and register file
- **Assembly Language** - Custom assembly language with lexer, parser, and assembler
- **Memory System** - RAM, ROM, and MMU with configurable memory mapping
- **Visual Debugging** - Real-time visualization of CPU state, registers, and memory
- **Microcode Execution** - Step-by-step instruction execution with microcode-level control

## Features

### Processor Components

- **Control Unit (CU)** - Manages instruction fetch, decode, and execution cycle
- **Arithmetic Logic Unit (ALU)** - Performs arithmetic and logical operations
- **Memory Management Unit (MMU)** - Handles memory addressing and access control
- **Register File** - General-purpose and special-purpose registers with sub-register support
- **Bus System** - Input/Output buses for data transfer between components
- **Interrupt Controller** - Hardware interrupt handling system

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
- **Hexa.NET.Utilities** - High-performance utility libraries
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
- Windows (due to WinExe output type)

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
- **Arithmetic**: `ADD`, `SUB`, `MUL`, `DIV`, `INC`, `DEC`
- **Logic**: `AND`, `OR`, `XOR`, `NOT`, `SHL`, `SHR`
- **Control Flow**: `JMP`, `CALL`, `RET`, `JE`, `JNE`, `JG`, `JL`, etc.
- **Comparison**: `CMP`, `TEST`
- **Special**: `HALT`, `NOP`, `INT`

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
│   │   └── RadixTree.cs    # Fast lookup data structure
│   ├── Buses/              # Bus system implementation
│   ├── Decoding/           # Instruction decoding
│   │   └── Instructions/   # Instruction implementations
│   ├── Memory/             # Memory subsystem
│   ├── Processor.cs        # Main CPU implementation
│   ├── ControlUnit.cs      # Control unit
│   ├── ArithmeticLogicalUnit.cs
│   ├── MemoryManagementUnit.cs
│   └── InterruptController.cs
├── MainWindow.cs           # Main UI window
├── Program.cs              # Application entry point
└── CPUSimulator.csproj     # Project file
```

## Architecture Details

### Memory Map

- **0x0000 - 0x3FFF** (0-16383): Stack space
- **0x4000 - 0x4FFF** (16384-20479): ROM (read-only, 4KB)
- **0x5000+** (20480+): RAM (read-write, ~16MB)

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

[License information to be added]

## Acknowledgments

- Built with [Hexa.NET.KittyUI](https://github.com/HexaEngine/Hexa.NET.KittyUI)
- Uses [ImGui](https://github.com/ocornut/imgui) for UI rendering
