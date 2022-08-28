using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDStudioPlus.Targets
{
    class TargetGenesisPlusGX : EmulatorTarget
    {
        private GenesisPlusGxThread thread;
        private static string disassembleString = new string('\0', 128);

        public TargetGenesisPlusGX()
        {
            try
            {
                thread = new GenesisPlusGxThread();
            }
            catch (Exception e)
            {
                throw (e);
            }
        }

        public override void Initialise(int width, int height, IntPtr parent, bool pal, char region)
        {
            thread.Init(width, height, parent, pal, region);
        }

        public override void Shutdown()
        {
            thread.Stop();
            thread.Destroy();
        }

        public override void BringToFront()
        {
            GenesisPlusGxThread.GetGenPlusGX().BringToFront();
        }

        public override void SetInputMapping(EmulatorInputs input, int mapping)
        {
            GenesisPlusGxThread.GetGenPlusGX().SetInputMapping((GenesisPlusGXInterface.GenesisPlusGX.SDLInputs)input, mapping);
        }

        public override void SendKeyPress(int vkCode, int keyDown)
        {
            //GenesisPlusGxThread.GetGenPlusGX().KeyPressed(vkCode, keyDown);
        }

        public override bool LoadBinary(string filename)
        {
            thread.LoadRom(filename);
            return true;
        }

        public override void Run()
        {
            thread.Start();

        }

        public override void Reset()
        {
            GenesisPlusGxThread.GetGenPlusGX().Reset();
        }

        public override void SoftReset()
        {
            GenesisPlusGxThread.GetGenPlusGX().SoftReset();
        }

        public override uint Break()
        {
            GenesisPlusGxThread.GetGenPlusGX().Break();
            return GetPC();
        }

        public override void Resume()
        {
            GenesisPlusGxThread.GetGenPlusGX().Resume();
        }

        public override uint Step()
        {
            GenesisPlusGxThread.GetGenPlusGX().StepInto();
            return GetPC();
        }

        public override bool IsHalted()
        {
            return (GenesisPlusGxThread.GetGenPlusGX() != null) ? GenesisPlusGxThread.GetGenPlusGX().IsDebugging() : false;
        }

        public override uint GetAReg(int index)
        {
            return (uint)GenesisPlusGxThread.GetGenPlusGX().GetAReg(index);
        }

        public override uint GetDReg(int index)
        {
            return (uint)GenesisPlusGxThread.GetGenPlusGX().GetDReg(index);
        }

        public override uint GetPC()
        {
            return (uint)GenesisPlusGxThread.GetGenPlusGX().GetCurrentPC();
        }

        public override uint GetSR()
        {
            return (uint)GenesisPlusGxThread.GetGenPlusGX().GetSR();
        }

        public override byte ReadByte(uint address)
        {
            return GenesisPlusGxThread.GetGenPlusGX().ReadByte(address);
        }

        public override uint ReadLong(uint address)
        {
            return GenesisPlusGxThread.GetGenPlusGX().ReadLong(address);
        }

        public override ushort ReadWord(uint address)
        {
            return GenesisPlusGxThread.GetGenPlusGX().ReadWord(address);
        }

        public override void ReadMemory(uint address, uint size, byte[] memory)
        {
            unsafe
            {
                fixed (byte* ptr = memory)
                {
                    GenesisPlusGxThread.GetGenPlusGX().ReadMemory(address, size, ptr);
                }
            }
        }

        public override uint GetZ80Reg(Z80Regs reg)
        {
            return (uint)GenesisPlusGxThread.GetGenPlusGX().GetZ80Reg((GenesisPlusGXInterface.GenesisPlusGX.Z80Regs)reg);
        }

        public override byte ReadZ80Byte(uint address)
        {
            return GenesisPlusGxThread.GetGenPlusGX().ReadZ80Byte(address);
        }

        public override bool AddBreakpoint(uint addr)
        {
            if (GenesisPlusGxThread.GetGenPlusGX() != null)
                return GenesisPlusGxThread.GetGenPlusGX().AddBreakpoint((int)addr) != 0;
            else
                return false;
        }

        public override bool AddWatchpoint(uint fromAddr, uint toAddr)
        {
            if (GenesisPlusGxThread.GetGenPlusGX() != null)
                return GenesisPlusGxThread.GetGenPlusGX().AddWatchpoint((int)fromAddr, (int)toAddr) != 0;
            else
                return false;
        }

        public override void RemoveBreakpoint(uint addr)
        {
            if (GenesisPlusGxThread.GetGenPlusGX() != null)
                GenesisPlusGxThread.GetGenPlusGX().ClearBreakpoint((int)addr);
        }

        public override void RemoveWatchpoint(uint addr)
        {
            if (GenesisPlusGxThread.GetGenPlusGX() != null)
                GenesisPlusGxThread.GetGenPlusGX().ClearWatchpoint((int)addr);
        }

        public override void RemoveAllBreakpoints()
        {
            if (GenesisPlusGxThread.GetGenPlusGX() != null)
                GenesisPlusGxThread.GetGenPlusGX().ClearBreakpoints();
        }

        public override void RemoveAllWatchPoints()
        {
            //TODO
            if (GenesisPlusGxThread.GetGenPlusGX() != null)
                GenesisPlusGxThread.GetGenPlusGX().ClearBreakpoints();
        }

        public override byte GetVDPRegisterValue(int index)
        {
            return GenesisPlusGxThread.GetGenPlusGX().GetVDPRegisterValue(index);
        }

        public override uint GetColor(int index)
        {
            return (uint)GenesisPlusGxThread.GetGenPlusGX().GetColor(index);
        }

        public override void SetVolume(int vol)
        {
            GenesisPlusGxThread.GetGenPlusGX().SetVolume(vol, true);
        }

        public override void PauseAudio(bool pause)
        {
            GenesisPlusGxThread.GetGenPlusGX().PauseAudio(pause);
        }

        public override uint Disassemble(uint address, ref string text)
        {
            uint size = 0;

            unsafe
            {
                fixed (char* cstr = disassembleString)
                {
                    size = GenesisPlusGxThread.GetGenPlusGX().Disassemble(address, (sbyte*)cstr);
                    text = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((System.IntPtr)cstr);
                }
            }

            return size;
        }

        public override uint[] CleanupBreakpoints()
        {
            return GenesisPlusGxThread.GetGenPlusGX().CleanupBreakpoints();
        }
    }
}
