using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDStudioPlus.Models
{
    class MemoryItem
    {
        public ObservableCollection<string> Value { get; set; }
        public string Index { get; set; }
    }
}
