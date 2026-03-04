namespace CPUSimulator
{
    using CPUSimulator.Core;
    using CPUSimulator.Core.Devices;
    using Hexa.NET.ImGui;
    using Hexa.NET.Utilities.Text;
    using System;
    using System.Numerics;

    /// <summary>
    /// ImGui-based video monitor that displays output from the VideoDevice.
    /// Supports both ASCII text mode and RGBA graphics mode.
    /// 
    /// Usage from assembly:
    /// 1. Set video mode: Write parameters to device control memory
    ///    - [0-7]: Command (2 = SetMode)
    ///    - [8-15]: Width (e.g., 80 for text or 320 for graphics)
    ///    - [16-23]: Height (e.g., 25 for text or 240 for graphics)
    ///    - [24-31]: Format (0 = ASCII, 1 = RGBAUNorm)
    ///    - [32-39]: Framebuffer address
    /// 
    /// 2. Write data to the framebuffer memory
    ///    - ASCII mode: 1 byte per character
    ///    - RGBA mode: 4 bytes per pixel (R, G, B, A)
    /// 
    /// 3. Flush to display: Write flush command
    ///    - [0-7]: Command (3 = Flush)
    /// 
    /// Example (ASCII):
    ///   mov rbx, [video_device_base]  ; Video device base address
    ///   mov rax, 2                     ; SetMode command
    ///   mov [rbx], rax
    ///   mov rax, 80                    ; Width
    ///   mov [rbx+8], rax
    ///   mov rax, 25                    ; Height
    ///   mov [rbx+16], rax
    ///   mov rax, 0                     ; Format (0 = ASCII)
    ///   mov [rbx+24], rax
    ///   mov rax, [framebuffer_addr]    ; Framebuffer address
    ///   mov [rbx+32], rax
    ///   
    ///   ; Write "Hello" to framebuffer
    ///   mov rbx, [framebuffer_addr]
    ///   mov byte [rbx], 'H'
    ///   mov byte [rbx+1], 'e'
    ///   mov byte [rbx+2], 'l'
    ///   mov byte [rbx+3], 'l'
    ///   mov byte [rbx+4], 'o'
    ///   
    ///   ; Flush to display
    ///   mov rbx, [video_device_base]
    ///   mov rax, 3                     ; Flush command
    ///   mov [rbx], rax
    /// </summary>
    public class VideoMonitor
    {
        private enum BufferFormat
        {
            ASCII = 0,
            RGBA8UNorm = 1
        }

        private readonly VideoDevice videoDevice;
        private readonly MemoryManagementUnit mmu;
        private ulong framebufferAddress;
        private uint screenWidth = 80;
        private uint screenHeight = 25;
        private BufferFormat format = BufferFormat.ASCII;
        private bool isOpen = true;
        private byte[] framebuffer = new byte[80 * 25];

        public VideoMonitor(VideoDevice videoDevice, MemoryManagementUnit mmu)
        {
            this.videoDevice = videoDevice;
            this.mmu = mmu;
            videoDevice.CommandIssued += OnCommandIssued;
        }

        private void OnCommandIssued(VideoDevice.CommandType command, ulong param1, ulong param2, ulong param3, ulong fb)
        {
            framebufferAddress = fb;

            switch (command)
            {
                case VideoDevice.CommandType.SetMode:
                    screenWidth = (uint)param1;
                    screenHeight = (uint)param2;
                    format = (BufferFormat)param3;

                    // Calculate buffer size based on format
                    uint bufferSize = format switch
                    {
                        BufferFormat.ASCII => screenWidth * screenHeight,
                        BufferFormat.RGBA8UNorm => screenWidth * screenHeight * 4,
                        _ => screenWidth * screenHeight
                    };
                    framebuffer = new byte[bufferSize];
                    break;

                case VideoDevice.CommandType.Clear:
                    Array.Clear(framebuffer);
                    break;

                case VideoDevice.CommandType.Flush:
                    RefreshFramebuffer();
                    break;
            }
        }

        private void RefreshFramebuffer()
        {
            if (framebufferAddress == 0 || framebuffer.Length == 0)
                return;

            try
            {
                mmu.Execute(framebufferAddress, framebuffer, MMUAction.Read);
            }
            catch (Exception)
            {
                // Handle invalid memory access gracefully
            }
        }

        public unsafe void Draw()
        {
            if (!ImGui.Begin("Video Monitor", ref isOpen, ImGuiWindowFlags.MenuBar))
            {
                ImGui.End();
                return;
            }

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("Settings"u8))
                {
                    int width = (int)screenWidth;
                    int height = (int)screenHeight;

                    if (ImGui.InputInt("Width"u8, ref width))
                    {
                        screenWidth = (uint)Math.Max(1, Math.Min(width, 200));
                        framebuffer = new byte[screenWidth * screenHeight];
                    }

                    if (ImGui.InputInt("Height"u8, ref height))
                    {
                        screenHeight = (uint)Math.Max(1, Math.Min(height, 100));
                        framebuffer = new byte[screenWidth * screenHeight];
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.MenuItem("Refresh"u8))
                {
                    RefreshFramebuffer();
                }

                ImGui.EndMenuBar();
            }

            // Display framebuffer info at top
            ImGui.Text($"Framebuffer Address: 0x{framebufferAddress:X}");
            ImGui.Text($"Screen Size: {screenWidth}x{screenHeight}");
            ImGui.Separator();

            switch (format)
            {
                case BufferFormat.ASCII:
                    DrawASCII();
                    break;
                case BufferFormat.RGBA8UNorm:
                    DrawRGBA();
                    break;
                default:
                    ImGui.Text("Unsupported format");
                    break;
            }

            ImGui.End();
        }

        private unsafe void DrawASCII()
        {
            var drawList = ImGui.GetWindowDrawList();
            var pos = ImGui.GetCursorScreenPos();

            var fontSize = ImGui.GetFontSize();
            var charSize = new Vector2(fontSize * 0.6f, fontSize);

            // Add a background for the display area
            var displaySize = new Vector2(screenWidth * charSize.X, screenHeight * charSize.Y);
            drawList.AddRectFilled(
                pos, 
                pos + displaySize, 
                ImGui.GetColorU32(new Vector4(0.1f, 0.1f, 0.1f, 1.0f)));

            byte* buf = stackalloc byte[2];
            buf[1] = 0; // null terminator

            for (uint y = 0; y < screenHeight; y++)
            {
                for (uint x = 0; x < screenWidth; x++)
                {
                    uint index = y * screenWidth + x;
                    if (index >= framebuffer.Length)
                        continue;

                    byte character = framebuffer[index];

                    // Only display printable ASCII characters
                    if (character == 0)
                    {
                        character = (byte)' ';
                    }
                    else if (character < 32 || character > 126)
                    {
                        // Replace non-printable with a dot
                        character = (byte)'.';
                    }

                    buf[0] = character;

                    Vector2 charPos = pos + new Vector2(x * charSize.X, y * charSize.Y);
                    drawList.AddText(charPos, ImGui.GetColorU32(ImGuiCol.Text), buf);
                }
            }

            // Reserve space for the display
            ImGui.Dummy(displaySize);
        }

        private unsafe void DrawRGBA()
        {
            var drawList = ImGui.GetWindowDrawList();
            var pos = ImGui.GetCursorScreenPos();

            const float pixelSize = 2.0f; // Scale up pixels for visibility
            var displaySize = new Vector2(screenWidth * pixelSize, screenHeight * pixelSize);

            fixed (byte* ptr = framebuffer)
            {
                // Framebuffer is stored as RGBA, but ImGui expects ABGR (0xAABBGGRR)
                // On little-endian: reading RGBA bytes as uint gives us (A<<24)|(B<<16)|(G<<8)|R = ABGR ✓
                uint* pixels = (uint*)ptr;

                for (uint y = 0; y < screenHeight; y++)
                {
                    for (uint x = 0; x < screenWidth; x++)
                    {
                        uint pixelIndex = y * screenWidth + x;
                        if (pixelIndex >= framebuffer.Length / 4)
                            continue;

                        // Read pixel: RGBA bytes in memory → ABGR uint (little-endian)
                        uint rgba = pixels[pixelIndex];

                        // Convert RGBA to ABGR for ImGui
                        byte r = (byte)(rgba & 0xFF);
                        byte g = (byte)((rgba >> 8) & 0xFF);
                        byte b = (byte)((rgba >> 16) & 0xFF);
                        byte a = (byte)((rgba >> 24) & 0xFF);

                        uint abgr = (uint)((a << 24) | (b << 16) | (g << 8) | r);

                        Vector2 pixelPos = pos + new Vector2(x * pixelSize, y * pixelSize);
                        Vector2 pixelEnd = pixelPos + new Vector2(pixelSize, pixelSize);
                        drawList.AddRectFilled(pixelPos, pixelEnd, abgr);
                    }
                }
            }

            // Reserve space for the display
            ImGui.Dummy(displaySize);
        }

        public void Show()
        {
            isOpen = true;
        }

        public void Hide()
        {
            isOpen = false;
        }
    }
}
