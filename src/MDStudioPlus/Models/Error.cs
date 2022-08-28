using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDStudioPlus.Models
{
    public class Error
    {        
        public string Code { get; set; }
        public string Description { get; set; }
        public string Filename { get; set; }
        public string LineNumber { get; set; }        

    }
}
