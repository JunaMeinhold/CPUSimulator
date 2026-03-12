section .text
// org 16384

start:
    mov rsp, 16384 // Stack Base address
    mov rax, 20480 // RAM Base address, jump to 0x4000 to see
    call setmode
	mov byte [rax], 'H'
	mov byte [rax+1], 'e'
	mov byte [rax+2], 'l'
	mov byte [rax+3], 'l'
	mov byte [rax+4], 'o'
	mov byte [rax+5], ' '
	mov byte [rax+6], 'W'
	mov byte [rax+7], 'o'
	mov byte [rax+8], 'r'
	mov byte [rax+9], 'l'
	mov byte [rax+10], 'd'
	mov byte [rax+11], '!'
	call flush
    hlt

flush: // flush video buffer, rax = framebuffer address
    push rbx
    mov rbx, 0xF0000000 // Video device base address
    mov qword [rbx+32], rax // Set framebuffer address
    mov qword [rbx], 0x3    // 0x3 = Flush command
    pop rbx
    ret

setmode:
	push r8
	mov r8, 0xF0000000
	mov qword [r8+8], 160 // Width
	mov qword [r8+16], 90 // Height
	mov qword [r8+24], 0  // ASCII-Mode
	mov qword [r8], 0x2
	nop
	pop r8
	ret

section .data