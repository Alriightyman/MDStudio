using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Serialization;
using MDStudioPlus.Debugging;
using MDStudioPlus.ViewModels;

namespace MDStudioPlus
{
    [XmlRoot("Root")]
    public class Project : ICloneable
    {
        #region Serializable Properties
        /// <summary>
        /// Name of the Project
        /// </summary>
        [XmlElement("ProjectName")]
        public string Name { get; set; }

        /// <summary>
        /// Project Author
        /// </summary>
        [XmlElement("Author")]
        public string Author { get; set; }

        /// <summary>
        /// Assembler Version this Project uses
        /// </summary>
        [XmlElement("Assembler")]
        public AssemblerVersion AssemblerVersion 
        {
            get => assemblerVersion;
            set
            {
                assemblerVersion = value;

                if (assemblerVersion == AssemblerVersion.AS) assembler = new AsAssembler(this, Workspace.Instance.ConfigViewModel.AsPath);
            }
        }

        /// <summary>
        /// Main File that will be passed to the assembler
        /// </summary>
        [XmlElement("MainSourceFile")]
        public string MainSourceFile { get; set; }

        /// <summary>
        /// All Source files
        /// </summary>
        [XmlElement("SourceFiles")]
        public string[] SourceFiles { get; set; }

        /// <summary>
        /// Script that runs before the assembler
        /// </summary>
        [XmlElement("PreBuildScript")]
        public string PreBuildScript { get; set; }

        /// <summary>
        /// Script that runs after the assember finishes
        /// </summary>
        [XmlElement("PostBuildScript")]
        public string PostBuildScript { get; set; }

        /// <summary>
        /// Name of the output binary, defaults to Project Name
        /// </summary>
        [XmlElement]
        public string OutputFileName { get; set; }

        /// <summary>
        /// Output binary extension; `.bin` is default
        /// </summary>
        [XmlElement]
        public string OutputExtension { get; set; }

        /// <summary>
        /// Additional aarguments to pass to the assembler
        /// </summary>
        [XmlElement("AdditionalArgs")]
        public string AdditionalArguments { get; set; }

        /// <summary>
        /// files to exclude from debugging symbols
        /// AS, for example, can do some fancy things 
        /// that AS thinks are 'addresses' even though
        /// they are not.
        /// </summary>
        [XmlElement("FileToExclude")]
        public string[] FilesToExclude { get; set; }

        #endregion

        #region Private Fields
        [XmlIgnore]
        private IAssembler assembler;

        [XmlIgnore]
        private AssemblerVersion assemblerVersion;

        [XmlIgnore]
        private StringBuilder processStandardOutput = new StringBuilder();

        [XmlIgnore]
        private StringBuilder processErrorOutput = new StringBuilder();
        #endregion

        #region Public Properties
        [XmlIgnore]
        public uint BuildId { get; set; }

        /// <summary>
        ///  Path to project
        /// </summary>
        [XmlIgnore]
        public string ProjectPath { get; set; }

        /// <summary>
        /// Full Path to project including filename
        /// </summary>
        [XmlIgnore]
        public string FullPath { get; set; }        

        [XmlIgnore]
        public static string Extension => ".mdproj";

        [XmlIgnore]
        public IAssembler Assembler => assembler;

        [XmlIgnore]
        public bool IsBuilt
        {
            get
            {
                // check for symbols
                string symbolPath = $"{ProjectPath}\\{OutputFileName}." + (AssemblerVersion == AssemblerVersion.AS ? "MAP" : "symb");
                if(!File.Exists(symbolPath))
                    return false;

                // check for binary
                string binaryPath = $"{ProjectPath}\\{OutputFileName}.{OutputExtension}";
                if (!File.Exists(binaryPath))
                    return false;

                return true;
            }
        }

        [XmlIgnore]
        public string ErrorPattern
        {
            get
            {
                if(assembler is AsAssembler)
                {
                    //return @"> > > (\w*\.\w*)\(\d+\):\d+: error (...)+";
                    return @"^\s?>\s?>\s?>\s?(\w*.\w*)\((\d+)\):?[\d+]?:\serror\s#?(\d+)?:\s(.+)";
                }
                else
                {
                    // asm68k
                    return @"([\w:\\.]*)\((\d+)\) : Error : (.+)";
                }
            }
        }
        #endregion

        #region Constructors
        // used for serialization/deserialization
        protected Project() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filepath">full path to project</param>
        public Project(string filepath)
        {
            ProjectPath = Path.GetDirectoryName(filepath);
            FullPath = filepath;
        }
        #endregion

        #region Public Methods


        /// <summary>
        /// Gets all the source files from this project
        /// </summary>
        /// <param name="fullPath">if true, returns full path, if false, returns relative paths</param>
        /// <returns>A list of all files contained in the project</returns>
        public IList<string> AllFiles(bool fullPath = false)
        {
            List<string> files;
            if (fullPath)
            {
                files = new List<string>();

                files.Add($"{ProjectPath}\\{MainSourceFile}");
                foreach (var file in SourceFiles)
                {
                    files.Add($"{ProjectPath}\\{file}");
                }
            }
            else
            {
                if (SourceFiles != null)
                {
                    files = new List<string>(SourceFiles) { MainSourceFile };
                }
                else
                {
                    files = new List<string>() { MainSourceFile };
                }
            }
            return files;
        }
        
        /// <summary>
        /// Saves the project
        /// </summary>
        /// <returns>true, if successful and false if error </returns>
        public bool Save()
        {
            try
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
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Loads the project
        /// </summary>
        /// <returns>true if successful, false if file doesn't exists</returns>
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
                        SourceFiles = project.SourceFiles?.Select(e =>e.Replace("/", "\\")).ToArray();
                        PreBuildScript = project.PreBuildScript ?? String.Empty;
                        PostBuildScript = project.PostBuildScript ?? String.Empty;
                        AdditionalArguments = project.AdditionalArguments ?? String.Empty;
                        FilesToExclude = project.FilesToExclude ?? new string[0];
                        OutputFileName = project.OutputFileName ?? project.Name;
                        OutputExtension = project.OutputExtension ?? "bin";

                        switch (AssemblerVersion)
                        {
                            case AssemblerVersion.AS:
                                assembler = new AsAssembler(this, Workspace.Instance.ConfigViewModel.AsPath);
                                break;
                            case AssemblerVersion.ASM68K:
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

        /// <summary>
        /// Builds this project
        /// </summary>
        /// <returns>The output of from this build</returns>
        public string[] Build()
        {
            processStandardOutput = new StringBuilder();
            processErrorOutput = new StringBuilder();
            List<string> output = new List<string>();

            output.AddRange(RunScript(PreBuildScript));
            output.AddRange(assembler.Assemble(processStandardOutput, processErrorOutput));
            output.AddRange(RunScript(PostBuildScript));

            RenameBuildOutput();

            return output.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ISymbols LoadSymbols(ISymbols symbols)
        {
            if (symbols == null) symbols = AssemblerVersion == AssemblerVersion.AS ? new AsSymbols(ProjectPath) : null;

            string symbolPath = $"{ProjectPath}\\{OutputFileName}." + (AssemblerVersion == AssemblerVersion.AS ? "MAP" : "symb");
            try
            {
                if (symbols.Read(symbolPath, FilesToExclude))
                {
                    return symbols;
                }
            }
            catch (FileNotFoundException e)
            {
                Debug.WriteLine(e.Message);
            }
            return null;
        }
        #endregion

        #region Private Methods

        // Runs a pre/post build script
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

                        // run each line as a command
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
                                        Application.Current.Dispatcher.Invoke(new Action(() =>
                                        {
                                            // send to build output window
                                            Workspace.Instance.Output.BuildOutput = processStandardOutput.ToString();
                                        }));
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

                                    // send to Error window
                                    Workspace.Instance.Errors.Update(e.Data);
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

        private void RenameBuildOutput()
        {
            string filename = Path.GetFileNameWithoutExtension(MainSourceFile);
            string fileExtension = (AssemblerVersion == AssemblerVersion.AS ? "MAP" : "symb");
            if (OutputFileName.ToLower() != filename.ToLower())
            {
                File.Delete($"{ProjectPath}\\{OutputFileName}.{OutputExtension}");
                File.Delete($"{ProjectPath}\\{OutputFileName}.{fileExtension}");

                try
                {
                    string binaryFile = $"{ProjectPath}\\{filename}.{OutputExtension}";
                    string debugFile = $"{ProjectPath}\\{filename}.{fileExtension}";
                    if (File.Exists(binaryFile))
                    {
                        // rename binary
                        File.Move(binaryFile, $"{ProjectPath}\\{OutputFileName}.{OutputExtension}");
                    }

                    if (File.Exists(debugFile))
                    {
                        // rename symbol file
                        File.Move(debugFile, $"{ProjectPath}\\{OutputFileName}.{fileExtension}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        /// <inheritdoc/>
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}
