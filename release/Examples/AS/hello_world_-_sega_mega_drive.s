;==============================================================
;		Original Author: Matt Phillips - bigevilcorporation.co.uk
;		AS Conversion:  Alriightyman
;==============================================================

	cpu 68000			; set the processor to m68k
	padding off			; we don't want AS padding out dc.b instructions
	listing purecode	; Want listing file, but only the final code in expanded macros
	supmode on			; we don't need warnings about privileged instructions
	page	0			; Don't want form feeds

;==============================================================
;		Some helpful macros for things like 'even'
;			 (borrowed from S3K Disassembly)
;==============================================================
; define an alternate org that fills the extra space with 0s instead of FFs
org0 macro address
.diff := address - *
	if .diff < 0
		error "too much stuff before org0 $\{address} ($\{(-.diff)} bytes)"
	else
		while .diff > 1024
			; AS can only generate 1 kb of code on a single line
			dc.b [1024]0
.diff := .diff - 1024
		endm
		dc.b [.diff]0
	endif
    endm	
; define an alternate cnop that fills the extra space with 0s instead of FFs
cnop0 macro offset,alignment
	org0 (*-1+(alignment)-((*-1+(-(offset)))#(alignment)))
    endm
; define an alternate align that fills the extra space with 0s instead of FFs
align0 macro alignment
	cnop0 0,alignment
    endm
; define the even pseudo-instruction
even macro
	align0 2
    endm

	 ; VDP port addresses
vdp_control				= $00C00004
vdp_data				= $00C00000

; VDP commands
vdp_cmd_vram_write		= $40000000
vdp_cmd_cram_write		= $C0000000

; Hardware version address
hardware_ver_address	= $00A10001

; TMSS
tmss_address			= $00A14000
tmss_signature			= 'SEGA'
	
; Data port setup macros
SetVRAMWrite macro addr
	move.l  #(vdp_cmd_vram_write)|((addr)&$3FFF)<<16|(addr)>>14,vdp_control
    endm
	
SetCRAMWrite macro addr
	move.l  #(vdp_cmd_cram_write)|((addr)&$3FFF)<<16|(addr)>>14,vdp_control
    endm
	 
	if * <> 0
        fatal "StartOfROM was $\{*} but it should be 0"
    endif
ROM_Start

	; CPU vector table
	dc.l   $00FFE000			; Initial stack pointer value
	dc.l   CPU_EntryPoint		; Start of program
	dc.l   CPU_Exception 		; Bus error
	dc.l   CPU_Exception 		; Address error
	dc.l   CPU_Exception 		; Illegal instruction
	dc.l   CPU_Exception 		; Division by zero
	dc.l   CPU_Exception 		; CHK CPU_Exception
	dc.l   CPU_Exception 		; TRAPV CPU_Exception
	dc.l   CPU_Exception 		; Privilege violation
	dc.l   INT_Null				; TRACE exception
	dc.l   INT_Null				; Line-A emulator
	dc.l   INT_Null				; Line-F emulator
	dc.l   INT_Null				; Unused (reserved)
	dc.l   INT_Null				; Unused (reserved)
	dc.l   INT_Null				; Unused (reserved)
	dc.l   INT_Null				; Unused (reserved)
	dc.l   INT_Null				; Unused (reserved)
	dc.l   INT_Null				; Unused (reserved)
	dc.l   INT_Null				; Unused (reserved)
	dc.l   INT_Null				; Unused (reserved)
	dc.l   INT_Null				; Unused (reserved)
	dc.l   INT_Null				; Unused (reserved)
	dc.l   INT_Null				; Unused (reserved)
	dc.l   INT_Null				; Unused (reserved)
	dc.l   INT_Null				; Spurious exception
	dc.l   INT_Null				; IRQ level 1
	dc.l   INT_Null				; IRQ level 2
	dc.l   INT_Null				; IRQ level 3
	dc.l   INT_HBlank			; IRQ level 4 (horizontal retrace interrupt)
	dc.l   INT_Null  			; IRQ level 5
	dc.l   INT_VBlank			; IRQ level 6 (vertical retrace interrupt)
	dc.l   INT_Null				; IRQ level 7
	dc.l   INT_Null				; TRAP #00 exception
	dc.l   INT_Null				; TRAP #01 exception
	dc.l   INT_Null				; TRAP #02 exception
	dc.l   INT_Null				; TRAP #03 exception
	dc.l   INT_Null				; TRAP #04 exception
	dc.l   INT_Null				; TRAP #05 exception
	dc.l   INT_Null				; TRAP #06 exception
	dc.l   INT_Null				; TRAP #07 exception
	dc.l   INT_Null				; TRAP #08 exception
	dc.l   INT_Null				; TRAP #09 exception
	dc.l   INT_Null				; TRAP #10 exception
	dc.l   INT_Null				; TRAP #11 exception
	dc.l   INT_Null				; TRAP #12 exception
	dc.l   INT_Null				; TRAP #13 exception
	dc.l   INT_Null				; TRAP #14 exception
	dc.l   INT_Null				; TRAP #15 exception
	dc.l   INT_Null				; Unused (reserved)
	dc.l   INT_Null				; Unused (reserved)
	dc.l   INT_Null				; Unused (reserved)
	dc.l   INT_Null				; Unused (reserved)
	dc.l   INT_Null				; Unused (reserved)
	dc.l   INT_Null				; Unused (reserved)
	dc.l   INT_Null				; Unused (reserved)
	dc.l   INT_Null				; Unused (reserved)
	dc.l   INT_Null				; Unused (reserved)
	dc.l   INT_Null				; Unused (reserved)
	dc.l   INT_Null				; Unused (reserved)
	dc.l   INT_Null				; Unused (reserved)
	dc.l   INT_Null				; Unused (reserved)
	dc.l   INT_Null				; Unused (reserved)
	dc.l   INT_Null				; Unused (reserved)
	dc.l   INT_Null				; Unused (reserved)

	; ROM metadata
	dc.b "SEGA MEGA DRIVE "                                 ; Console name
	dc.b "AUTHOR NAME HERE"                                 ; Copyright holder and release date
	dc.b "GAME TITLE                                      " ; Domestic name
	dc.b "GAME TITLE                                      " ; International name
	dc.b "GM XXXXXXXX-XX"                                   ; Version number
	dc.w $0000                                             ; Checksum
	dc.b "J               "                                 ; I/O support
	dc.l ROM_Start                                          ; Start address of ROM
	dc.l ROM_End-1                                          ; End address of ROM
	dc.l $00FF0000                                         ; Start address of RAM
	dc.l $00FF0000+$0000FFFF                              ; End address of RAM
	dc.l $00000000                                         ; SRAM enabled
	dc.l $00000000                                         ; Unused
	dc.l $00000000                                         ; Start address of SRAM
	dc.l $00000000                                         ; End address of SRAM
	dc.l $00000000                                         ; Unused
	dc.l $00000000                                         ; Unused
	dc.b "                                        "         ; Notes (unused)
	dc.b "  E             "                                 ; Country codes
	
CPU_EntryPoint

	jsr VDP_WriteTMSS
	jsr VDP_LoadRegisters
	
	SetCRAMWrite $0000
	move.l #Palette, a0
	move.w #$F, d0			; Loop counter = 16 words in palette (-1)
	.PalLp:					; Start of loop
	move.w (a0)+, vdp_data	; Write palette entry, post-increment address
	dbra d0, .PalLp			; Loop until finished
	
	SetVRAMWrite $0020
	move.l #CharacterH, a0
	move.w #(8*40)-1, d0	; Loop counter = 8 longwords in tile (-1)
	.CharLp:				; Start of loop
	move.l (a0)+, vdp_data	; Write tile line, post-increment address
	dbra d0, .CharLp		; Loop until finished
	
	SetVRAMWrite $C418
	move.w #$0001, vdp_data	; H
	SetVRAMWrite $C41A
	move.w #$0002, vdp_data	; E
	SetVRAMWrite $C41C
	move.w #$0003, vdp_data	; L
	SetVRAMWrite $C41E
	move.w #$0003, vdp_data	; L
	SetVRAMWrite $C420
	move.w #$0004, vdp_data	; 0
	
	SetVRAMWrite $C424
	move.w #$0005, vdp_data	; W
	SetVRAMWrite $C426
	move.w #$0004, vdp_data	; O
	SetVRAMWrite $C428
	move.w #$0006, vdp_data	; R
	SetVRAMWrite $C42A
	move.w #$0003, vdp_data	; L
	SetVRAMWrite $C42C
	move.w #$0007, vdp_data	; D
	
	; Halt CPU
	stop   #$2700

INT_VBlank:
	rte

INT_HBlank:
	rte

INT_Null:
	rte

CPU_Exception:
	stop   #$2700
	rte
	
VDP_WriteTMSS:

	move.b hardware_ver_address, d0			; Move Megadrive hardware version to d0
	andi.b #$0F, d0						; The version is stored in last four bits, so mask it with 0F
	beq .Skip								; If version is equal to 0, skip TMSS signature
	move.l #tmss_signature, tmss_address	; Move the string "SEGA" to $A14000
	.Skip:

	; Check VDP
	move.w vdp_control, d0					; Read VDP status register (hangs if no access)
	
	rts
	
VDP_LoadRegisters:

	; Set VDP registers
	move.l #VDPRegisters, a0	; Load address of register init table into a0
	move.w #$17, d0			; 24 registers to write (-1 for loop counter)
	move.w #$8000, d1			; 'Set register 0' command

	.CopyVDP:
	move.b (a0)+, d1			; Move register value to lower byte of d1
	move.w d1, vdp_control		; Write command and value to VDP control port
	add.w  #$0100, d1			; Increment register #
	dbra   d0, .CopyVDP
	
	rts
	
; Initial VDP register values
VDPRegisters:
	dc.b $14 ; 0:  H interrupt on, palettes on
	dc.b $74 ; 1:  V interrupt on, display on, DMA on, Genesis mode on
	dc.b $30 ; 2:  Pattern table for Scroll Plane A at VRAM $C000 (bits 3-5 = bits 13-15)
	dc.b $00 ; 3:  Pattern table for Window Plane at VRAM $0000 (disabled) (bits 1-5 = bits 11-15)
	dc.b $07 ; 4:  Pattern table for Scroll Plane B at VRAM $E000 (bits 0-2 = bits 11-15)
	dc.b $78 ; 5:  Sprite table at VRAM $F000 (bits 0-6 = bits 9-15)
	dc.b $00 ; 6:  Unused
	dc.b $00 ; 7:  Background colour - bits 0-3 = colour, bits 4-5 = palette
	dc.b $00 ; 8:  Unused
	dc.b $00 ; 9:  Unused
	dc.b $08 ; 10: Frequency of Horiz. interrupt in Rasters (number of lines travelled by the beam)
	dc.b $00 ; 11: External interrupts off, V scroll fullscreen, H scroll fullscreen
	dc.b $81 ; 12: Shadows and highlights off, interlace off, H40 mode (320 x 224 screen res)
	dc.b $3F ; 13: Horiz. scroll table at VRAM $FC00 (bits 0-5)
	dc.b $00 ; 14: Unused
	dc.b $02 ; 15: Autoincrement 2 bytes
	dc.b $01 ; 16: Vert. scroll 32, Horiz. scroll 64
	dc.b $00 ; 17: Window Plane X pos 0 left (pos in bits 0-4, left/right in bit 7)
	dc.b $00 ; 18: Window Plane Y pos 0 up (pos in bits 0-4, up/down in bit 7)
	dc.b $FF ; 19: DMA length lo byte
	dc.b $FF ; 20: DMA length hi byte
	dc.b $00 ; 21: DMA source address lo byte
	dc.b $00 ; 22: DMA source address mid byte
	dc.b $80 ; 23: DMA source address hi byte, memory-to-VRAM mode (bits 6-7)
	
	even	

	; Palette
Palette:
	dc.w $0000	; Transparent
	dc.w $0000	; Black
	dc.w $0EEE	; White
	dc.w $000E	; Red
	dc.w $00E0	; Blue
	dc.w $0E00	; Green
	dc.w $0E0E	; Pink
	dc.w $0000
	dc.w $0000
	dc.w $0000
	dc.w $0000
	dc.w $0000
	dc.w $0000
	dc.w $0000
	dc.w $0000
	dc.w $0000
	
	; Font glyphs for "HELOWRD"
CharacterH:
	dc.l $22000220
	dc.l $22000220
	dc.l $22000220
	dc.l $22222220
	dc.l $22000220
	dc.l $22000220
	dc.l $22000220
	dc.l $00000000
	
CharacterE:
	dc.l $22222220
	dc.l $22000000
	dc.l $22000000
	dc.l $22222220
	dc.l $22000000
	dc.l $22000000
	dc.l $22222220
	dc.l $00000000
	
CharacterL:
	dc.l $22000000
	dc.l $22000000
	dc.l $22000000
	dc.l $22000000
	dc.l $22000000
	dc.l $22000000
	dc.l $22222220
	dc.l $00000000
	
CharacterO:
	dc.l $22222220
	dc.l $22000220
	dc.l $22000220
	dc.l $22000220
	dc.l $22000220
	dc.l $22000220
	dc.l $22222220
	dc.l $00000000
	
CharacterW:
	dc.l $22000220
	dc.l $22000220
	dc.l $22000220
	dc.l $22020220
	dc.l $22020220
	dc.l $22020220
	dc.l $22222220
	dc.l $00000000
	
CharacterR:
	dc.l $22222200
	dc.l $22000220
	dc.l $22000220
	dc.l $22222200
	dc.l $22022000
	dc.l $22002200
	dc.l $22000220
	dc.l $00000000
	
CharacterD:
	dc.l $22222200
	dc.l $22002220
	dc.l $22000220
	dc.l $22000220
	dc.l $22000220
	dc.l $22002220
	dc.l $22222200
	dc.l $00000000

ROM_End