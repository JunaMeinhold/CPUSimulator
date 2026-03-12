section .text
// org 16384

start:
    cli
    mov rsp, 16384 // Stack Base address
    call setmode
    mov rax, 20480 // RAM Base address, jump to 0x4000 to see
    call fillscreen
    mov rax, 20480
    call flush
    nop
    sti
    hlt

fillscreen:
    push rbx
    push rcx

    mov rcx, 14400           // 160 * 90 pixels
    mov ebx, 0xFFFF0000      // Blue, fully opaque

fill_loop:
    mov [rax], ebx           // write one pixel (4 bytes)

    add rax, 4               // advance to next pixel
    sub rcx, 1
    cmp rcx, 0
    jne fill_loop

    pop rcx
    pop rbx
    ret

flush: // flush video buffer, rax = framebuffer address
    push rbx
    push rcx
    mov rbx, 0xF0000000 // Video device base address
    mov [rbx+32], rax // Set framebuffer address
    mov [rbx], 0x3    // 0x3 = Flush command
    pop rcx
    pop rbx
    ret

setmode:
	push r8
	mov r8, 0xF0000000
	mov [r8+8], 160 // Width
	mov [r8+16], 90 // Height
	mov [r8+24], 1  // Format RGBA8UNorm
	mov [r8], 0x2
	nop
	pop r8
	ret 

section .data