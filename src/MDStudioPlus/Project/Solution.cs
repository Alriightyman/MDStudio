using MDStudioPlus.Debugging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace MDStudioPlus
{
    [XmlRoot("Root")]
    public class Solution
    {
        // serializable items
        [XmlElement("SolutionName")]
        public string Name { get; set; }
        [XmlIgnore]
        public string SolutionPath { get; set; }
        [XmlElement("Projects")]
        public List<string> ProjectFiles { get; set; }

        // non serializable items
        [XmlIgnore]
        public static string Extension => ".mdsln";

        [XmlIgnore]
        public string FullPath { get; private set; }

        [XmlIgnore]
        public List<Project> Projects { get; private set; } = new List<Project>();

        [XmlIgnore]
        public Project CurrentlySelectedProject 
        { 
            get; 
            set; 
        }

        [XmlIgnore]
        public string BinaryPath { get; private set; }

        
        protected Solution() { }

        public Solution(string filepath)
        {
            Name = Path.GetFileNameWithoutExtension(filepath);
            SolutionPath = Path.GetDirectoryName(filepath);
            FullPath = filepath;
        }

        public bool Save()
        {
            XmlSerializer xs = new XmlSerializer(typeof(Solution));
            string projectFile = $"{SolutionPath}\\{Name}.mdsln";
            if (!File.Exists(projectFile))
            {
                if (!Directory.Exists(SolutionPath))
                {
                    Directory.CreateDirectory(SolutionPath);
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
            StringBuilder sb = new StringBuilder();
            try
            {
                sb.AppendLine($"Loading Solution from {FullPath}");
                XmlSerializer xs = new XmlSerializer(typeof(Solution));
                if (File.Exists(FullPath))
                {
                    using (StreamReader sr = new StreamReader(FullPath))
                    {
                        try
                        {
                            Solution solution;
                            solution = (Solution)xs.Deserialize(sr);
                            sb.AppendLine($"Solution {solution.Name} Loaded");
                            Name = solution.Name;
                            ProjectFiles = solution.ProjectFiles?.Select(e => e.Replace("/", "\\")).ToList();
                            sb.AppendLine($"Project List Count: {ProjectFiles.Count}");
                            var projList = new List<Project>();
                            // projects are relative paths
                            foreach (var project in ProjectFiles)
                            {
                                sb.AppendLine($"Loading Project {project}");
                                var newProj = LoadProject($"{SolutionPath}\\{project}");
                                if (newProj != null)
                                {
                                    sb.AppendLine($"Project {project} Loaded.");
                                    Projects.Add(newProj);
                                }
                                else
                                {
                                    sb.AppendLine($"Project {project} failed to load");
                                }
                            }

                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                        }
                    }

                    CurrentlySelectedProject = Projects.First();

                    return true;
                }
            }
            catch(Exception e)
            {
               File.WriteAllText($"{SolutionPath}\\mdstuido.log", sb.ToString());
            }

            return false;
        }

        public int Build()
        {
            var projList = Projects.ToList();
            List<string> output = new List<string>();

            for (int i = 0; i < projList.Count; i++)
            {
                var proj = Projects.FirstOrDefault(p => p.BuildId == i);

                output.AddRange(proj?.Build());
                BinaryPath = $"{SolutionPath}\\{proj.OutputFileName}.bin";
            }

            int errorCount = 0;
            foreach (string line in output)
            {
                string patternError = Projects.FirstOrDefault()?.ErrorPattern;
                Match matchError = Regex.Match(line, patternError);
                if (matchError.Success)
                {
                    errorCount++;
                }
            }

            return errorCount;
        }
        
        public string GetFullPath(string partialPath)
        {
            string fullPath = partialPath;
            foreach( var project in Projects)
            {
                var file = project.AllFiles().Where(f => f == partialPath).FirstOrDefault();
                if (file != null)
                {
                    fullPath = $"{project.ProjectPath}\\{partialPath}";
                    break;
                }
            }

            return fullPath;
        }

        public ISymbols GetDebugSymbols()
        {
            ISymbols symbols = null;
            foreach ( var project in Projects)
            {
                symbols = project.LoadSymbols(symbols);
            }

            return symbols;
        }


        private Project LoadProject(string filepath)
        {            
            Project project = new Project(filepath);
            if (project.Load())
            {
                return project;
            }

            return null;
        }


    }
}
