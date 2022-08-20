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

;==============================================================
; Memory map (named offsets from start of RAM)	
	phase $00FFE000
ram_vscroll			ds.w 1	; Current value of vscroll (1 word)
ram_hscroll			ds.w 1	; Current value of hscroll (1 word)
	dephase
	!org 0
;==============================================================
	
; VDP port addresses
vdp_control				= $00C00004
vdp_data				= $00C00000

; VDP commands
vdp_cmd_vram_write		= $40000000
vdp_cmd_cram_write		= $C0000000
vdp_cmd_vsram_write		= $40000010

; VDP memory addresses
vram_addr_hscroll		= $FC00

; Hardware version address
hardware_ver_address	= $00A10001

; TMSS
tmss_address			= $00A14000
tmss_signature			= 'SEGA'

; Total number of glyphs in the font
num_font_glyphs			= $7

; The size of one palette (in bytes, words, and longwords)
size_palette_b			= $10
size_palette_w			= size_palette_b*2
size_palette_l			= size_palette_b*4

; The size of one graphics tile (in bytes, words, and longwords)
size_tile_b				= $20
size_tile_w				= size_tile_b*2
size_tile_l				= size_tile_b*4

; Hello World draw position as a byte offset
; (there are 40 tiles IDs per line, each tile ID is 2 bytes)
text_pos_x_offset		= $18
text_pos_y_offset		= $C4

;==============================================================
	
; VDP data port setup macros
SetVRAMWrite macro addr
	move.l  #(vdp_cmd_vram_write)|((addr)&$3FFF)<<16|(addr)>>14, vdp_control
    endm
	
SetVSRAMWrite macro addr
	move.l  #(vdp_cmd_vsram_write)|((addr)&$3FFF)<<16|(addr)>>14, vdp_control
    endm
	
SetCRAMWrite macro addr
	move.l  #(vdp_cmd_cram_write)|((addr)&$3FFF)<<16|(addr)>>14, vdp_control
    endm
;==============================================================

; Font glyph tile IDs
tile_id_space	= $0
tile_id_h		= $1
tile_id_e		= $2
tile_id_l		= $3
tile_id_o		= $4
tile_id_w		= $5
tile_id_r		= $6
tile_id_d		= $7
;==============================================================
	
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
	
;==============================================================

	; ROM metadata
	dc.b "SEGA MEGA DRIVE "                                 ; Console name
	dc.b "BIGEVILCORP.    "                                 ; Copyright holder and release date
	dc.b "HELLO WORLD                                     " ; Domestic name
	dc.b "HELLO WORLD                                     " ; International name
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
	
;==============================================================

	; The "main()" function
CPU_EntryPoint:

	;==============================================================
	; Initialise the Mega Drive
	;==============================================================

	; Write the TMSS signature (if a model 1+ Mega Drive)
	jsr VDP_WriteTMSS
	
	; Load the initial VDP registers
	jsr VDP_LoadRegisters
	
	;==============================================================
	; Initialise variables in RAM
	;==============================================================
	move.w #$0, ram_vscroll
	move.w #$0, ram_hscroll
	
	;==============================================================
	; Initialise status register and set interrupt level
	;==============================================================
	move.w #$2300, sr
	
	;==============================================================
	; Write a palette to colour memory
	;==============================================================
	
	; Setup the VDP to write to CRAM address $0000 (first palette)
	SetCRAMWrite $0000
	
	; Write the palette
	move.l #Palette, a0				; Move palette address to a0
	move.w #size_palette_l-1, d0	; Loop counter = 8 words in palette (-1 for DBRA loop)
	.PalLp:							; Start of loop
	move.w (a0)+, vdp_data			; Write palette entry, post-increment address
	dbra d0, .PalLp					; Decrement d0 and loop until finished (when d0 reaches -1)
	
	;==============================================================
	; Write the font to tile memory
	;==============================================================
	
	; Setup the VDP to write to VRAM address $0000 (the address of the first graphics tile)
	SetVRAMWrite $0000
	
	; Write the font glyphs
	move.l #CharacterSpace, a0					; Move the address of the first graphics tile into a0
	move.w #(num_font_glyphs*size_tile_l)-1, d0	; Loop counter = 8 longwords per tile (-1 for DBRA loop)
	.CharLp:									; Start of loop
	move.l (a0)+, vdp_data						; Write tile line (4 bytes per line), post-increment address
	dbra d0, .CharLp							; Decrement d0 and loop until finished (when d0 reaches -1)
	
	;==============================================================
	; Write the tile IDs of "HELLO WORLD" to Plane A
	;==============================================================
	
	; Setup the VDP to write the tile ID at text_pos_x,text_pos_y (just the address in memory)
	SetVRAMWrite ((text_pos_y_offset<<8)|text_pos_x_offset)
	
	; then move the character tile ID
	move.w #tile_id_h, vdp_data	; H
	
	; Repeat for the remaining characters in the string (remembering to offset the X coord each time)
	SetVRAMWrite ((text_pos_y_offset<<8)|text_pos_x_offset+2)
	move.w #tile_id_e, vdp_data		; E
	SetVRAMWrite ((text_pos_y_offset<<8)|text_pos_x_offset+4)
	move.w #tile_id_l, vdp_data		; L
	SetVRAMWrite ((text_pos_y_offset<<8)|text_pos_x_offset+6)
	move.w #tile_id_l, vdp_data		; L
	SetVRAMWrite ((text_pos_y_offset<<8)|text_pos_x_offset+8)
	move.w #tile_id_o, vdp_data		; 0
	SetVRAMWrite ((text_pos_y_offset<<8)|text_pos_x_offset+10)
	move.w #tile_id_space, vdp_data	; SPACE
	SetVRAMWrite ((text_pos_y_offset<<8)|text_pos_x_offset+12)
	move.w #tile_id_w, vdp_data		; W
	SetVRAMWrite ((text_pos_y_offset<<8)|text_pos_x_offset+14)
	move.w #tile_id_o, vdp_data		; O
	SetVRAMWrite ((text_pos_y_offset<<8)|text_pos_x_offset+16)
	move.w #tile_id_r, vdp_data		; R
	SetVRAMWrite ((text_pos_y_offset<<8)|text_pos_x_offset+18)
	move.w #tile_id_l, vdp_data		; L
	SetVRAMWrite ((text_pos_y_offset<<8)|text_pos_x_offset+20)
	move.w #tile_id_d, vdp_data		; D
	
	;==============================================================
	; Loop forever
	;==============================================================
	.InfiniteLp:
	bra .InfiniteLp
	
;==============================================================

INT_VBlank:

	; Backup any registers we're about to use to the stack
	move.l d0, -(sp)
	move.l d1, -(sp)
	
	; Fetch current values of VSCROLL and HSCROLL from RAM
	move.w ram_vscroll, d0
	move.w ram_hscroll, d1
	
	; Increment values
	add.w  #$1, d0
	add.w  #$1, d1
	
	; Store back in RAM
	move.w d0, ram_vscroll
	move.w d1, ram_hscroll
	
	; Setup VDP to write to HSCROLL (it's at VRAM address $FC00) and write the word
	SetVRAMWrite vram_addr_hscroll
	move.w d1, vdp_data
	
	; Setup VDP to write to VSCROLL (it has its own dedicated memory, so use the VSRAM macro) and write the word
	SetVSRAMWrite $0000
	move.w d0, vdp_data
	
	; Restore registers from stack (in reverse order)
	move.l (sp)+, d1
	move.l (sp)+, d0
	
	rte

INT_HBlank:
	rte

INT_Null:
	rte

CPU_Exception:
	stop   #$2700
	rte
	
;==============================================================
	
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

;==============================================================
	
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
		
;==============================================================

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
	
;==============================================================
	
	; Font glyphs for "HELO WRD"
	; 'SPACE' is first, which is unneccessary but it's a good teaching tool for
	; why we leave the first tile in memory blank
CharacterSpace:
	dc.l $00000000
	dc.l $00000000
	dc.l $00000000
	dc.l $00000000
	dc.l $00000000
	dc.l $00000000
	dc.l $00000000
	dc.l $00000000
	
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