using MDStudioPlus.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MDStudioPlus
{
    [XmlRoot("Root")]
    public class Project
    {
        #region Serializable Fields
        [XmlElement("ProjectName")]
        public string Name { get; set; }

        [XmlElement("Author")]
        public string Author { get; set; }

        [XmlElement("Assembler")]
        public Assembler AssemblerVersion { get; set; }

        [XmlElement("MainSourceFile")]
        public string MainSourceFile { get; set; }

        [XmlElement("SourceFiles")]
        public string[] SourceFiles { get; set; }

        [XmlElement("PreBuildScript")]
        public string PreBuildScript { get; set; }

        [XmlElement("PostBuildScript")]
        public string PostBuildScript { get; set; }

        [XmlElement("AdditionalArgs")]
        public string AdditionalArguments { get; set; }
        #endregion

        [XmlIgnore]
        public uint BuildId { get; set; }

        [XmlIgnore]
        public string ProjectPath { get; set; }               


        [XmlIgnore]
        public string FullPath { get; set; }
        
        [XmlIgnore]
        private IAssembler assembler;

        [XmlIgnore]
        public static string Extension => ".mdproj";

        StringBuilder processStandardOutput = new StringBuilder();
        StringBuilder processErrorOutput = new StringBuilder();

        // used for serialization/deserialization
        protected Project() { }

        public Project(string filepath)
        {
            ProjectPath = Path.GetDirectoryName(filepath);
            FullPath = filepath;
        }

        public IList<string> AllFiles()
        {
            return new List<string>(SourceFiles) { MainSourceFile };
        }

        public bool Save()
        {
            XmlSerializer xs = new XmlSerializer(typeof(Project));
            string projectFile = $"{ProjectPath}\\{Name}.mdproj";
            if (!File.Exists(projectFile))
            {
                if (!Directory.Exists(ProjectPath))
                {
                    Directory.CreateDirectory(ProjectPath);
                }

                File.OpenWrite(projectFile).Close();
            }

            using (TextWriter sw = new StreamWriter(projectFile))
            {
                xs.Serialize(sw, this);
            }
            
            return true;
        }

        public bool Load()
        {
            XmlSerializer xs = new XmlSerializer(typeof(Project));

            if (File.Exists(FullPath))
            {
                using (StreamReader sr = new StreamReader(FullPath))
                {
                    try
                    {
                        Project project;
                        project = (Project)xs.Deserialize(sr);

                        Name = project.Name;
                        AssemblerVersion = project.AssemblerVersion;
                        Author = project.Author;
                        MainSourceFile = project.MainSourceFile.Replace("/", "\\");
                        SourceFiles = project.SourceFiles?.Select(e => e.Replace("/", "\\")).ToArray();
                        PreBuildScript = project.PreBuildScript ?? String.Empty;
                        PostBuildScript = project.PostBuildScript ?? String.Empty;
                        AdditionalArguments = project.AdditionalArguments ?? String.Empty;

                        switch (AssemblerVersion)
                        {
                            case Assembler.AS:
                                assembler = new AsAssembler(this, "");
                                break;
                            case Assembler.ASM68K:
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public string[] Build()
        {
            processStandardOutput = new StringBuilder();
            processErrorOutput = new StringBuilder();
            List<string> output = new List<string>();

            output.AddRange(RunScript(PreBuildScript));
            output.AddRange(assembler.Assemble(processStandardOutput, processErrorOutput));
            output.AddRange(RunScript(PostBuildScript));

            return output.ToArray();
        }

        private string[] RunScript(string script)
        {
            if (String.IsNullOrEmpty(script))
            {
                return new string[0];
            }
            
            try
            {
                int timeout = 60 * 1000 * 1000;
                string[] commands = script.Split('\n');


                using (System.Threading.AutoResetEvent outputWaitHandle = new System.Threading.AutoResetEvent(false))
                using (System.Threading.AutoResetEvent errorWaitHandle = new System.Threading.AutoResetEvent(false))
                {
                    using (Process process = new Process())
                    {
                        process.StartInfo.FileName = "cmd.exe";
                        process.StartInfo.WorkingDirectory = ProjectPath + @"\";
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.RedirectStandardInput = true;
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.RedirectStandardError = true;
                        process.StartInfo.CreateNoWindow = true;

                        process.Start();

                        using (StreamWriter sw = process.StandardInput)
                        {
                            foreach (var command in commands)
                            {
                                if (!command.StartsWith("REM",StringComparison.CurrentCulture) && !String.IsNullOrWhiteSpace(command) && !String.IsNullOrEmpty(command))
                                {
                                    if (sw.BaseStream.CanWrite)
                                    {
                                        sw.WriteLine(command);
                                    }
                                }
                            }
                        }

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
                                    if (!String.IsNullOrEmpty(e.Data))
                                    {
                                        processStandardOutput.AppendLine(e.Data);
                                        Workspace.Instance.Output.BuildOutput = processStandardOutput.ToString();
                                    }
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
                                    processErrorOutput.AppendLine(e.Data);
                                }
                            };

                            process.BeginOutputReadLine();
                            process.BeginErrorReadLine();

                            if (!process.WaitForExit(timeout))
                            {
                                processErrorOutput.Append("Process timed out");
                            }
                        }
                        finally
                        {
                            outputWaitHandle.WaitOne(timeout);
                            errorWaitHandle.WaitOne(timeout);
                            Debug.WriteLine(" Script Finished");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("build exception: " + e.Message);
            }

            string[] output;
            if (processErrorOutput.Length > 0)
                output = processErrorOutput.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            else
                output = processStandardOutput.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            return output;
        }
    }
}
