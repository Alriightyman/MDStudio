﻿using MDStudioPlus.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDStudioPlus
{
    public abstract class AssemblerBase : IAssembler
    {
        public virtual Assembler Assembler => Assembler.None;

        public string AdditionalFlags { get; set; }

/*        public virtual StringBuilder ProcessStandardOutput { get; protected set; }

        public virtual StringBuilder ProcessErrorOutput { get; protected set; }*/

        protected string assemblerPath;
        protected string projectWorkingDirectory;
        protected string fileToAssemble;

        public AssemblerBase(Project project, string assemblerPath)
        {
            this.assemblerPath = assemblerPath;
            AdditionalFlags = project.AdditionalArguments;
            projectWorkingDirectory = project.ProjectPath;
            fileToAssemble = project.MainSourceFile;
        }

        protected abstract void Build(Process process);

        public string[] Assemble(StringBuilder standardOutput, StringBuilder errorOutput)
        {
            try
            {
                int timeout = 60 * 1000 * 1000;

                using (System.Threading.AutoResetEvent outputWaitHandle = new System.Threading.AutoResetEvent(false))
                using (System.Threading.AutoResetEvent errorWaitHandle = new System.Threading.AutoResetEvent(false))
                {
                    using (Process process = new Process())
                    {
                        Build(process);

                        Debug.WriteLine("Assembler: {0} {1}", process.StartInfo.FileName, process.StartInfo.Arguments);
                        standardOutput.AppendLine($"Assembling: {process.StartInfo.FileName} {process.StartInfo.Arguments}\n");
                        Workspace.Instance.Output.BuildOutput = standardOutput.ToString();

                        process.Start();

                        try
                        {
                            process.OutputDataReceived += (sender, e) =>
                            {
                                if (e.Data == null)
                                {
                                    outputWaitHandle.Set();
                                }
                                else
                                {
                                    standardOutput.AppendLine(e.Data);
                                    Workspace.Instance.Output.BuildOutput = standardOutput.ToString();
                                }
                            };
                            process.ErrorDataReceived += (sender, e) =>
                            {
                                if (e.Data == null)
                                {
                                    errorWaitHandle.Set();
                                }
                                else
                                {
                                    errorOutput.AppendLine(e.Data);
                                }
                            };

                            process.BeginOutputReadLine();
                            process.BeginErrorReadLine();

                            if (!process.WaitForExit(timeout))
                            {
                                errorOutput.Append("Process timed out");
                            }
                        }
                        finally
                        {
                            outputWaitHandle.WaitOne(timeout);
                            errorWaitHandle.WaitOne(timeout);

                            standardOutput.AppendLine("\n");
                            Workspace.Instance.Output.BuildOutput = standardOutput.ToString();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("build exception: " + e.Message);
            }

            string[] output;
            if (errorOutput.Length > 0)
                output = errorOutput.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            else
                output = standardOutput.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            return output;
        }
    }
}
