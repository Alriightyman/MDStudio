using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDStudioPlus.Targets
{
    public enum Z80Regs
    {
        FA = 0,
        CB = 1,
        ED = 2,
        LH = 3,
        FA_ALT = 4,
        CB_ALT = 5,
        ED_ALT = 6,
        LH_ALT = 7,
        IX = 8,
        IY = 9,
        SP = 10,
        PC = 11
    }

    public enum EmulatorInputs
    {
        InputUp = 0,
        InputDown = 1,
        InputLeft = 2,
        InputRight = 3,
        InputB = 4,
        InputC = 5,
        InputA = 6,
        InputStart = 7,
        InputZ = 8,
        InputY = 9,
        InputX = 10,
        InputMode = 11,
        Input_COUNT = 12
    }

    public abstract class Target
    {
        //Initialisation
        public abstract bool LoadBinary(string filename);

        //Control
        public abstract void Run();
        public abstract void Reset();
        public abstract uint Break();
        public abstract void Resume();
        public abstract uint Step();
        public abstract bool IsHalted();

        //Registers
        public abstract uint GetAReg(int index);
        public abstract uint GetDReg(int index);
        public abstract uint GetPC();
        public abstract uint GetSR();

        //Memory
        public abstract byte ReadByte(uint address);
        public abstract uint ReadLong(uint address);
        public abstract ushort ReadWord(uint address);
        public abstract void ReadMemory(uint address, uint size, byte[] memory);

        //Z80
        public abstract uint GetZ80Reg(Z80Regs reg);
        public abstract byte ReadZ80Byte(uint address);

        //Breakpoints
        public abstract bool AddBreakpoint(uint addr);
        public abstract bool AddWatchpoint(uint fromAddr, uint toAddr);
        public abstract void RemoveBreakpoint(uint addr);
        public abstract void RemoveWatchpoint(uint addr);
        public abstract void RemoveAllBreakpoints();
        public abstract void RemoveAllWatchPoints();

        //Disassembly
        public abstract uint Disassemble(uint address, ref string text);

        // Sound
        public abstract void SetVolume(int vol);
        public abstract void PauseAudio(bool pause);
    }

    abstract class EmulatorTarget : Target
    {
        //Window management
        public abstract void Initialise(int width, int height, IntPtr parent, bool pal, char region);
        public abstract void Shutdown();
        public abstract void BringToFront();

        //Input
        public abstract void SetInputMapping(EmulatorInputs input, int mapping);
        public abstract void SendKeyPress(int vkCode, int keyDown);

        //VDP
        public abstract byte GetVDPRegisterValue(int index);
        public abstract uint GetColor(int index);

        //Control
        public abstract void SoftReset();
    }
}
