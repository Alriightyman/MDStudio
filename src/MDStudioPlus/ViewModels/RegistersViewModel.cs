using MDStudioPlus.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDStudioPlus.ViewModels
{
    internal class RegistersViewModel : ToolViewModel
    {
        #region 68K Registers
        private ObservableCollection<Register> dataRegisters = new ObservableCollection<Register>();
        public ObservableCollection<Register> DataRegisters
        {
            get => dataRegisters;
            set
            {
                dataRegisters = value;
                RaisePropertyChanged(nameof(DataRegisters));
            }
        }

        private ObservableCollection<Register> addressRegisters = new ObservableCollection<Register>();
        public ObservableCollection<Register> AddressRegisters
        {
            get => addressRegisters;
            set
            {
                addressRegisters = value;
                RaisePropertyChanged(nameof(AddressRegisters));
            }
        }

        private ObservableCollection<Register> stateRegisters = new ObservableCollection<Register>();
        public ObservableCollection<Register> StateRegisters
        {
            get => stateRegisters;
            set
            {
                stateRegisters = value;
                RaisePropertyChanged(nameof(StateRegisters));
            }
        }

        #endregion

        #region Z80 Registers
        private ObservableCollection<Register> z80Registers8Bit = new ObservableCollection<Register>();
        public ObservableCollection<Register> Z80Registers8Bit
        {
            get => z80Registers8Bit;
            set
            {
                z80Registers8Bit = value;
                RaisePropertyChanged(nameof (Z80Registers8Bit));
            }
        }

        private ObservableCollection<Register> z80Registers8BitAlt = new ObservableCollection<Register>();
        public ObservableCollection<Register> Z80Registers8BitAlt
        {
            get => z80Registers8BitAlt;
            set
            {
                z80Registers8BitAlt = value;
                RaisePropertyChanged(nameof(Z80Registers8BitAlt));
            }
        }

        private ObservableCollection<Register> z80Registers16Bit = new ObservableCollection<Register>();
        public ObservableCollection<Register> Z80Registers16Bit
        {
            get => z80Registers16Bit;
            set
            {
                z80Registers16Bit = value;
                RaisePropertyChanged(nameof(Z80Registers16Bit));
            }
        }

        private ObservableCollection<Register> z80RegistersState = new ObservableCollection<Register>();
        public ObservableCollection<Register> Z80RegistersState
        {
            get => z80RegistersState;
            set
            {
                z80RegistersState = value;
                RaisePropertyChanged(nameof(z80RegistersState));
            }
        }

        #endregion

        public RegistersViewModel() : base("Registers")
        {
            IsVisible = false;

            for (int i = 0; i < 8; i++)
            {
                Register d = new Register() { Name = $"D{i}", Value = 0.ToString("X8") };
                DataRegisters.Add(d);
                // Register A7 is used as SP
                Register a = new Register() { Name = (i < 7 ? $"A{i}" :"SP"), Value = 0.ToString("X8") };
                AddressRegisters.Add(a);
            }

            Register sr = new Register() { Name = "SR", Value = 0.ToString("X8") };
            Register pc = new Register() { Name = "PC", Value = 0.ToString("X8") };
            StateRegisters.Add(sr);
            StateRegisters.Add(pc);

            UpdateZ80Registers(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }

        public void UpdateRegisterValues(uint[] dRegs, uint[] aRegs, uint sr, uint pc)
        {
            // 68k Registers
            DataRegisters.Clear();
            AddressRegisters.Clear();
            StateRegisters.Clear();

            for (int i = 0; i < 8; i++)
            {
                DataRegisters.Add(new Register() { Name = $"D{i}", Value = dRegs[i].ToString("X8" )});
                AddressRegisters.Add(new Register() { Name = (i < 7 ? $"A{i}" : "SP"), Value = aRegs[i].ToString("X8") });
            }

            StateRegisters.Add(new Register() { Name = "SR", Value = sr.ToString("X8") });
            StateRegisters.Add(new Register() { Name = "PC", Value = pc.ToString("X8") });
            
        }

        public void UpdateZ80Registers(uint af, uint bc, uint de, uint hl, uint af_alt, uint bc_alt,
                                        uint de_alt, uint hl_alt, uint ix, uint iy, uint sr, uint pc)
        {
            Z80Registers8Bit.Clear();
            Z80Registers8BitAlt.Clear();
            Z80Registers16Bit.Clear();
            Z80RegistersState.Clear();

            // 8 bit
            Z80Registers8Bit.Add(new Register() { Name = "A", Value = (af >> 8).ToString("X2") });
            Z80Registers8Bit.Add(new Register() { Name = "B", Value = (bc >> 8).ToString("X2") });
            Z80Registers8Bit.Add(new Register() { Name = "D", Value = (de >> 8).ToString("X2") });
            Z80Registers8Bit.Add(new Register() { Name = "H", Value = (hl >> 8).ToString("X2") });
            Z80Registers8Bit.Add(new Register() { Name = "F", Value = (af & 8).ToString("X2") });
            Z80Registers8Bit.Add(new Register() { Name = "C", Value = (bc & 8).ToString("X2") });
            Z80Registers8Bit.Add(new Register() { Name = "E", Value = (de & 8).ToString("X2") });
            Z80Registers8Bit.Add(new Register() { Name = "L", Value = (hl & 8).ToString("X2") });

            Z80Registers8BitAlt.Add(new Register() { Name = "A'", Value = (af_alt >> 8).ToString("X2") });
            Z80Registers8BitAlt.Add(new Register() { Name = "B'", Value = (bc_alt >> 8).ToString("X2") });
            Z80Registers8BitAlt.Add(new Register() { Name = "D'", Value = (de_alt >> 8).ToString("X2") });
            Z80Registers8BitAlt.Add(new Register() { Name = "H'", Value = (hl_alt >> 8).ToString("X2") });
            Z80Registers8BitAlt.Add(new Register() { Name = "F'", Value = (af_alt & 8).ToString("X2") });
            Z80Registers8BitAlt.Add(new Register() { Name = "C'", Value = (bc_alt & 8).ToString("X2") });
            Z80Registers8BitAlt.Add(new Register() { Name = "E'", Value = (de_alt & 8).ToString("X2") });
            Z80Registers8BitAlt.Add(new Register() { Name = "L'", Value = (hl_alt & 8).ToString("X2") });

            // 16 bit
            Z80Registers16Bit.Add(new Register() { Name = "IX", Value = ix.ToString("X8") });
            Z80Registers16Bit.Add(new Register() { Name = "IY", Value = iy.ToString("X8") });

            // state
            Z80RegistersState.Add(new Register() { Name = "SR", Value = sr.ToString("X8") });
            Z80RegistersState.Add(new Register() { Name = "PC", Value = pc.ToString("X8") });
        }
    }
}
