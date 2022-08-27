using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDStudioPlus.Models.Debugging
{
    /// <summary>
    /// Breakpoint keeps track of line number and filename, address, etc.
    /// </summary>
    public class Breakpoint
    {
        public string Filename { get; set; }
        public int Line { get; set; }
        public int OriginalLine { get; set; }
        public uint Address { get; set; }
        public bool IsEnabled { get; set; }
    }
}
