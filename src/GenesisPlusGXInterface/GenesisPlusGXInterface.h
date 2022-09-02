#pragma once
// GenesisPlusGXInterface.h

#pragma once

#include <string>

using namespace System;

namespace GenesisPlusGXInterface {

	public ref class GenesisPlusGX
	{
	public:

		enum class SDLInputs
		{
			eInputUp,
			eInputDown,
			eInputLeft,
			eInputRight,
			eInputB,
			eInputC,
			eInputA,
			eInputStart,
			eInputZ,
			eInputY,
			eInputX,
			eInputMode,

			eInput_COUNT
		};

		enum class Z80Regs
		{
			Z80_REG_FA,
			Z80_REG_CB,
			Z80_REG_ED,
			Z80_REG_LH,
			Z80_REG_FA_ALT,
			Z80_REG_CB_ALT,
			Z80_REG_ED_ALT,
			Z80_REG_LH_ALT,
			Z80_REG_IX,
			Z80_REG_IY,
			Z80_REG_SP,
			Z80_REG_PC
		};

		GenesisPlusGX();
		~GenesisPlusGX();

		int		Init(int windowWidth, int windowHeight, IntPtr hwnd, bool pal, char region, bool useGamepad);
		void	SetWindowPosition(int x, int y);
		int		GetWindowXPosition();
		int		GetWindowYPosition();
		void	BringToFront();
		int		Reset();
		void	SoftReset();
		int		LoadRom(String^ path);
		int		Update();

		int		AddBreakpoint(int addr);
		void	ClearBreakpoint(int addr);
		void	ClearBreakpoints();
		int		AddWatchpoint(int fromAddr, int toAddr);
		void	ClearWatchpoint(int fromAddr);
		void	ClearWatchpoints();
		int		StepInto();
		int		Resume();
		int		Break();
		bool	IsDebugging();
		unsigned int* GetProfilerResults(int* instructionCount);
		unsigned int GetInstructionCycleCount(unsigned int address);

		void	Show();
		void	Hide();

		//	Accessors
		int		GetDReg(int index);
		int		GetAReg(int index);
		int		GetSR();
		int		GetCurrentPC();
		int		GetRegisters();
		unsigned char	ReadByte(unsigned int address);
		unsigned short	ReadWord(unsigned int address);
		unsigned int	ReadLong(unsigned int address);
		void    ReadMemory(unsigned int address, unsigned int size, BYTE* memory);

		int		GetZ80Reg(Z80Regs reg);
		unsigned char	ReadZ80Byte(unsigned int address);

		void	SetInputMapping(SDLInputs input, int mapping);
		int		GetInputMapping(SDLInputs input);

		int		GetColor(int index);

		unsigned char	GetVDPRegisterValue(int index);

		unsigned int Disassemble(unsigned int address, char* text);
		array<unsigned int>^ CleanupBreakpoints();
		void SetVolume(int vol, bool isSetDebug);
		void PauseAudio(bool pause);
		unsigned char* GetVRAM();
		unsigned char* GetCRAM();
		unsigned char* Get68kMemory();

		void UpdateDebug();


		void SetVRAM(unsigned char* v_ram) { vram = v_ram; }
		void SetCRAM(unsigned char* c_ram) { cram = c_ram; }
		void SetMem(unsigned char* mem68k) { mem = mem68k; }

	private:
		unsigned char* vram;
		unsigned char* cram;
		unsigned char* mem;

	};
}
