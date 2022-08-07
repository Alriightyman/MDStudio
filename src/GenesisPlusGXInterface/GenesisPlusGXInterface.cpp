// This is the main DLL file.

//#include "stdafx.h"
#include <msclr\marshal.h>

#include "GenesisPlusGxInterface.h"

#include "../Genesis-Plus-GX/mdstudio/sdl2/genspluxgx.h"

#include <sdl.h>

using namespace System;
using namespace msclr::interop;

GenesisPlusGXInterface::GenesisPlusGX::GenesisPlusGX()
{
}

GenesisPlusGXInterface::GenesisPlusGX::~GenesisPlusGX()
{
	::Shutdown();
}

int GenesisPlusGXInterface::GenesisPlusGX::Init(int windowWidth, int windowHeight, IntPtr hwnd, bool pal, char region, bool useGamepad)
{
	return ::Init(windowWidth, windowHeight, static_cast<HWND>(hwnd.ToPointer()), (int)pal, region, useGamepad);
}

void GenesisPlusGXInterface::GenesisPlusGX::SetWindowPosition(int x, int y)
{
	::SetWindowPosition(x, y);
}

int	GenesisPlusGXInterface::GenesisPlusGX::GetWindowXPosition()
{
	return ::GetWindowXPosition();
}

int	GenesisPlusGXInterface::GenesisPlusGX::GetWindowYPosition()
{
	return ::GetWindowYPosition();
}

int GenesisPlusGXInterface::GenesisPlusGX::Reset()
{
	return ::Reset();
}

void GenesisPlusGXInterface::GenesisPlusGX::SoftReset()
{
	return ::SoftReset();
}

void GenesisPlusGXInterface::GenesisPlusGX::BringToFront()
{
	::BringToFront();
}

int GenesisPlusGXInterface::GenesisPlusGX::LoadRom(String^ path)
{
	marshal_context^ context = gcnew marshal_context();
	const char* result = context->marshal_as<const char*>(path);
	int returnValue = ::LoadRom(result);
	delete context;
	return returnValue;
}

int GenesisPlusGXInterface::GenesisPlusGX::Update()
{
	return ::Update();
}

int GenesisPlusGXInterface::GenesisPlusGX::AddBreakpoint(int addr)
{
	return ::AddBreakpoint(addr);
}

void GenesisPlusGXInterface::GenesisPlusGX::ClearBreakpoint(int addr)
{
	::ClearBreakpoint(addr);
}

void	GenesisPlusGXInterface::GenesisPlusGX::ClearBreakpoints()
{
	::ClearBreakpoints();
}

int GenesisPlusGXInterface::GenesisPlusGX::AddWatchpoint(int fromAddr, int toAddr)
{
	return ::AddWatchpoint(fromAddr, toAddr);
}

void GenesisPlusGXInterface::GenesisPlusGX::ClearWatchpoint(int fromAddr)
{
	::ClearWatchpoint(fromAddr);
}

void GenesisPlusGXInterface::GenesisPlusGX::ClearWatchpoints()
{
	::ClearWatchpoints();
}

int GenesisPlusGXInterface::GenesisPlusGX::StepInto()
{
	return ::StepInto();
}

bool GenesisPlusGXInterface::GenesisPlusGX::IsDebugging()
{
	return ::IsDebugging() == 1 ? true : false;
}

unsigned int* GenesisPlusGXInterface::GenesisPlusGX::GetProfilerResults(int* instructionCount)
{
	return ::GetProfilerResults(instructionCount);
}

unsigned int GenesisPlusGXInterface::GenesisPlusGX::GetInstructionCycleCount(unsigned int address)
{
	return ::GetInstructionCycleCount(address);
}

int GenesisPlusGXInterface::GenesisPlusGX::GetDReg(int index)
{
	return ::GetDReg(index);
}

int GenesisPlusGXInterface::GenesisPlusGX::GetAReg(int index)
{
	return ::GetAReg(index);
}

int GenesisPlusGXInterface::GenesisPlusGX::GetSR()
{
	return ::GetSR();
}

int GenesisPlusGXInterface::GenesisPlusGX::GetCurrentPC()
{
	return ::GetCurrentPC();
}

int GenesisPlusGXInterface::GenesisPlusGX::GetZ80Reg(Z80Regs reg)
{
	return ::GetZ80Reg((int)reg);
}

int GenesisPlusGXInterface::GenesisPlusGX::Resume()
{
	return ::Resume();
}

int GenesisPlusGXInterface::GenesisPlusGX::Break()
{
	return ::Break();
}

int GenesisPlusGXInterface::GenesisPlusGX::GetRegisters()
{
	return 0;
}

unsigned char GenesisPlusGXInterface::GenesisPlusGX::ReadByte(unsigned int address)
{
	return ::ReadByte(address);
}

unsigned short GenesisPlusGXInterface::GenesisPlusGX::ReadWord(unsigned int address)
{
	return ::ReadWord(address);
}

unsigned int GenesisPlusGXInterface::GenesisPlusGX::ReadLong(unsigned int address)
{
	return ::ReadLong(address);
}

void GenesisPlusGXInterface::GenesisPlusGX::ReadMemory(unsigned int address, unsigned int size, BYTE* memory)
{
	::ReadMemory(address, size, memory);
}

unsigned char GenesisPlusGXInterface::GenesisPlusGX::ReadZ80Byte(unsigned int address)
{
	return ::ReadZ80Byte(address);
}

void GenesisPlusGXInterface::GenesisPlusGX::SetInputMapping(SDLInputs input, int mapping)
{
	::SetInputMapping((int)input, mapping);
}

int GenesisPlusGXInterface::GenesisPlusGX::GetInputMapping(SDLInputs input)
{
	return ::GetInputMapping((int)input);
}

void	GenesisPlusGXInterface::GenesisPlusGX::Show()
{
	ShowSDLWindow();
}

void	GenesisPlusGXInterface::GenesisPlusGX::Hide()
{
	HideSDLWindow();
}

//void	GenesisPlusGXInterface::GenesisPlusGX::KeyPressed(int vkCode, int keyDown)
//{
//	::KeyPressed(vkCode, keyDown);
//}

int GenesisPlusGXInterface::GenesisPlusGX::GetColor(int i)
{
	return ::GetPaletteEntry(i);
}

unsigned char GenesisPlusGXInterface::GenesisPlusGX::GetVDPRegisterValue(int index)
{
	return ::GetVDPRegisterValue(index);
}

unsigned int GenesisPlusGXInterface::GenesisPlusGX::Disassemble(unsigned int address, char* text)
{
	return ::Disassemble(address, text);
}

unsigned char* GenesisPlusGXInterface::GenesisPlusGX::GetVRAM()
{
	return ::GetVRAM();
}

void GenesisPlusGXInterface::GenesisPlusGX::SetVolume(int vol, bool isSetDebug)
{
	::SetVolume(vol, isSetDebug);
}

void GenesisPlusGXInterface::GenesisPlusGX::PauseAudio(bool pause)
{
	::PauseAudio(pause);
}