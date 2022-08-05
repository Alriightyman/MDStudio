using MDStudioPlus.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDStudioPlus.ViewModels
{
    class MemoryViewModel : ToolViewModel
    {
        private int numberCols = 16;

        public MemoryViewModel() : base("Memory Viewer")
        {
            memory = new ObservableCollection<MemoryItem>();
            for(int i = 0; i < (0x10000 / numberCols); i++)
            {
                Memory.Add(new MemoryItem() { Value = new ObservableCollection<string>(){ "0", "0", "0", "0", "0", "0", "0", "0",
                                                                    "0","0","0","0","0","0","0","0"}, Index = (i*16).ToString("X4") });
            }
        }

        private ObservableCollection<MemoryItem> memory;
        public ObservableCollection<MemoryItem> Memory
        {
            get => memory;
            set
            {
                memory = value;
                RaisePropertyChanged(nameof(Memory));
            }
        }

        /// <summary>
        /// Updates the values in the Memory Viewer
        /// </summary>
        /// <param name="mem"></param>
        public void UpdateMemory(byte[] mem)
        {
            // only update these values when showing
            if (IsSelected)
            {
                int index = 0;
                for (int row = 0; row < (mem.Length / numberCols); row++)
                {
                    int col = 0;
                    int memoryIndex = index;
                    string[] str = new string[numberCols];
                    for (col = 0; col < numberCols; col++, index++)
                    {
                            str[col] = mem[row * numberCols + col].ToString("X2");
                    }

                    Memory[row] = new MemoryItem() { Index = memoryIndex.ToString("X4"), Value = new ObservableCollection<string>(str) };

                }
            }
        }
    }
}
