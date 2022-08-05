﻿using System.Diagnostics;
using System.Text;

namespace MDStudioPlus
{
    public class AsAssembler : AssemblerBase
    {
        public override AssemblerVersion Assembler => AssemblerVersion.AS;
        
        public AsAssembler(Project project, string assemblerPath) 
                            : base(project, assemblerPath)
        {
        }

        protected override void Build(Process process)
        {
            process.StartInfo.FileName = AssemblerPath;
            process.StartInfo.WorkingDirectory = projectWorkingDirectory + @"\";
            // default ags:
            // -xx : Level 2 for detailed error messages
            // -q : suppress messages
            // -c : variables will be written in a format which permits an easy integration into a C-source file. The extension of the file is H
            // -A : stores the list of global symbols in another, more compact form
            // -L : writes assembler listing into a file
            // -g MAP : This switch instructs AS to create an additional file that contains debug information for the program
            process.StartInfo.Arguments = @"-xx -n -c -A -L -g MAP " + AdditionalFlags + " \"" + fileToAssemble + "\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
        }
    }
}