using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDStudioPlus
{

    public interface IAssembler
    {
        Assembler Assembler { get; }
        string AdditionalFlags { get; set; }
/*        StringBuilder ProcessStandardOutput { get; }
        StringBuilder ProcessErrorOutput { get; }*/

        string[] Assemble(StringBuilder standardOutput, StringBuilder errorOutput);
    }
}
