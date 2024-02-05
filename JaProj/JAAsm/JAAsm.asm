.data
    i QWORD 0       ; Variable storing the current iteration value
    j QWORD 0       ; Variable storing the current position in the pattern
    k QWORD 0       ; Number of found indices

.code
MyProc1 proc
    mov rcx, rcx    ; rcx contains a pointer to the text in C#
    mov rbx, rbx    ; rbx contains a pointer to the pattern in C#    
    mov rax, rdi    ; rax contains a pointer to the results array
    mov r9, r9      ; r9 contains the length of the pattern
    mov rdx, rdx    ; rdx contains the length of the text
    mov k, 0
    mov i, 0
    mov j, 0
    mov r13, [k]
    mov r11, [i]
    mov r12, [j]

    mov rdi, [rsp + 40] ; rdi contains a pointer to the LPS array in C#
    
; Main Loop (while i < rdx)
while_loop:         
    cmp r11, rdx       
    jge end_program    

    ; Compare pat[j] and str[i]
    movzx r8, byte ptr [rcx + r11]  ; Read str[i]
    movzx r10, byte ptr [rbx + r12] ; Read pat[j]

     ; Przyjête za³o¿enie: d³ugoœæ patternu w bajtach jest przechowywana w rejestrze r9

vpxor ymm1, ymm1, ymm1
      vmovups ymm1, [rbx]

      vpxor ymm2, ymm2, ymm2
      vmovups ymm2, [rcx]

      vpxor ymm3, ymm3, ymm3  ; Wyczyszczenie wektora, w którym bêdziemy trzymaæ wynik porównania

      vpcmpeqb ymm3, ymm1, ymm2  ; Porównanie wektorów ymm1 i ymm2




    ; Compare with ASCII code of the letters
``    cmp r8, r10
    jne not_match      ; If str[i] != pat[j], jump to not_match label

    ; Condition satisfied, increment j and i
    inc r12
    inc r11

not_match:
    cmp r12, r9
    jne not_match_2    ; If j is not equal to the length of the pattern, continue
    
    ; Pattern found, update the result array and reset j
    push r11
    sub r11, r12
    mov qword ptr [rax + r13 * 8], r11
    inc r13
    pop r11
    
    push rdx
    dec r12
    movzx edx, byte ptr [rdi + r12 * 4]
    mov r12, rdx
    pop rdx
    jmp not_match_2

not_match_2:
    cmp r11, rdx 
    jge end_while
    movzx r8, byte ptr [rcx + r11]  ; Read str[i]
    movzx r10, byte ptr [rbx + r12] ; Read pat[j]
    cmp r8, r10
    je end_while

    cmp r12, 0
    jne continue
    inc r11
    jmp end_while

continue:
    push rdx
    dec r12
    movzx edx, byte ptr [rdi + r12 * 4]
    mov r12, rdx
    pop rdx

end_while:
    jmp while_loop

end_program:
    mov rax, r13
    ret              
MyProc1 endp
end
