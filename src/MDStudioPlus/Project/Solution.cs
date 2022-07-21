using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        public List<Project> Projects = new List<Project>();

        [XmlIgnore]
        public string FullPath;

        [XmlIgnore]
        public static string Extension => ".mdsln";

        [XmlIgnore]
        public Project CurrentlySelectedProject { get; private set; }

        [XmlIgnore]
        private string lastProjectName;

        [XmlIgnore]
        public string BinaryPath
        {
            get => lastProjectName;
            set => lastProjectName = value;
        }
        
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
            XmlSerializer xs = new XmlSerializer(typeof(Solution));
            if (File.Exists(FullPath))
            {
                using (StreamReader sr = new StreamReader(FullPath))
                {
                    try
                    {
                        Solution solution;
                        solution = (Solution)xs.Deserialize(sr);

                        Name = solution.Name;
                        ProjectFiles = solution.ProjectFiles?.Select(e => e.Replace("/", "\\")).ToList();
                        var projList = new List<Project>();
                        // projects are relative paths
                        foreach (var project in ProjectFiles)
                        {
                            var newProj = LoadProject($"{SolutionPath}\\{project}");
                            if(newProj != null)
                            {
                                Projects.Add(newProj);
                            }
                        }
                        
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }

                CurrentlySelectedProject = Projects.First();

                return true;
            }
            else
            {
                return false;
            }
        }

        public int Build()
        {
            var projList = Projects.ToList();
            List<string> output = new List<string>();

            for (int i = 0; i < projList.Count; i++)
            {
                var proj = Projects.FirstOrDefault(p => p.BuildId == i);

                output.AddRange(proj?.Build());
                BinaryPath = $"{SolutionPath}\\{proj.Name}.bin";
            }

            int errorCount = 0;
            foreach (string line in output)
            {
                string patternError = @"> > > (\w*\.\w*)\(\d+\):\d+: error (...)+";//@"> > >([\w:\\.]*)\((\d+)\): error (.+)";
                Match matchError = Regex.Match(line, patternError);
                if (matchError.Success)
                {
                    errorCount++;
                }
            }

            return errorCount;
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
