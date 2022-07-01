using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace MDStudio
{
	[XmlRoot("Root")]
	public class Project
	{
		[XmlElement("ProjectName")]
		public string Name { get; set; }

		[XmlElement]
		public string Author { get; set; }

		[XmlElement("Assembler")]
		public Assembler Assembler { get; set; }
		
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

		// used to exclude certain files from building debugging symbols
		// This is used to get around how AS creates address values on 
		// macro statements and can interfer with how the debugger opperates. 
		[XmlElement("FileToExclude")]
		public string[] FilesToExclude { get; set; }

		public Project() { }

		public bool Write(string PathToProject)
		{
			XmlSerializer xs = new XmlSerializer(typeof(Project));
			string projectFile = $"{PathToProject}\\{Name}.mdproj";
			if (!File.Exists(projectFile))
			{
				if (!Directory.Exists(PathToProject))
				{
					Directory.CreateDirectory(PathToProject);
				}

				File.OpenWrite(projectFile).Close();

			}
			using (TextWriter sw = new StreamWriter(projectFile))
			{
				xs.Serialize(sw, this);
			}
			return true;
		}

		public bool Read(string filename)
		{
			XmlSerializer xs = new XmlSerializer(typeof(Project));
			
			if (File.Exists(filename))
			{
				using (StreamReader sr = new StreamReader(filename))
				{
					try
					{
						Project project;
						project = (Project)xs.Deserialize(sr);

						Name = project.Name;
						Assembler = project.Assembler;
						Author = project.Author;
						MainSourceFile = project.MainSourceFile.Replace("/", "\\");
						SourceFiles = project.SourceFiles?.Select(e => e.Replace("/", "\\")).ToArray();
						PreBuildScript = project.PreBuildScript ?? String.Empty;
						PostBuildScript = project.PostBuildScript ?? String.Empty;
						AdditionalArguments = project.AdditionalArguments ?? String.Empty;
						FilesToExclude = project.FilesToExclude?.Select(e => e.Replace("/", "\\")).ToArray();						
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
	}
}
