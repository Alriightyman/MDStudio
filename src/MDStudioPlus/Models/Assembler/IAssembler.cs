using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDStudioPlus
{

    public interface IAssembler
    {
        AssemblerVersion Assembler { get; }
        string AdditionalFlags { get; set; }
        string AssemblerPath { get; set; }

        string[] Assemble(StringBuilder standardOutput, StringBuilder errorOutput);
    }
}
