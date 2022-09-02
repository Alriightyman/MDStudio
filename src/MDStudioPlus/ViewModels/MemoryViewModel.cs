using MDStudioPlus.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MDStudioPlus.ViewModels
{
    class MemoryViewModel : ToolViewModel
    {
        //private int numberCols = 16;
        public const string ToolContentId = "MemoryTool";

        private RelayCommand<string> textChangedCommand;

        public MemoryViewModel() : base("Memory Viewer")
        {
            ContentId = ToolContentId;
        }

        private MemoryStream memoryStream;
        private BinaryReader reader;
        public BinaryReader Reader
        {
            get { return reader; }
            set {
                reader = value;
                RaisePropertyChanged(nameof(Reader));
            }
        }

        long offset = 0;
        public long Offset
        {
            get => offset;
            set
            {
                offset = value;
                RaisePropertyChanged(nameof(Offset));
            }
        }

        public string HeaderText
        {
            get
            {
                string text = "\t   ";

                for (int col = 0; col < this.ColumnCount; col++)
                {
                    text += $"${col.ToString("X2")} ";
                }

                return text;
            }
        }



        public ICommand TextChangedCommand
        {
            get { 
                
                if(textChangedCommand == null)
                {
                    textChangedCommand = new RelayCommand<string>((p) => GotoAddress(p));
                }
                
                return textChangedCommand; }
        }

        private void GotoAddress(string text)
        {
            if (text == String.Empty)
            {
                Offset = 0;
                return;
            }

            int hex;
            if(Int32.TryParse(text, System.Globalization.NumberStyles.HexNumber, null, out hex))
            {
                Offset = hex;
            }
        }

        public int ColumnCount
        {
            get;
            set;
        } = 16;


        /// <summary>
        /// Updates the values in the Memory Viewer
        /// </summary>
        /// <param name="mem"></param>
        public void UpdateMemory(byte[] mem)
        {
            // only update these values when showing
            if (IsVisible)
            {
                memoryStream?.Dispose();
                memoryStream = new MemoryStream(mem);
                Reader?.Dispose();
                Reader = new BinaryReader(memoryStream);                
            }
        }        
    }
}
