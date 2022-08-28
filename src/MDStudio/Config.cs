using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using static MDStudio.Themes;

namespace MDStudio
{
    [XmlRoot("Root")]
    public class Config
    {
        public string TargetName { get; set; }

        [XmlElement("AssemblerPath")]
        public string AsPath { get; set; }
        public string Asm68kPath { get; set; }
        public string[] AssemblerIncludePaths { get; set; }
        public int EmuResolution { get; set; }
        public int EmuRegion { get; set; }
        public bool Pal = false;
        public bool AutoOpenLastProject = true;
        public string LastProject { get; set; }

        // Index into SDL_Keycode.Keycode enum
        public int KeycodeUp { get; set; }
        public int KeycodeDown { get; set; }
        public int KeycodeLeft { get; set; }
        public int KeycodeRight { get; set; }
        public int KeycodeA { get; set; }
        public int KeycodeB { get; set; }
        public int KeycodeC { get; set; }
        public int KeycodeStart { get; set; }

        [XmlElement("Others")]
        public string MegaUSBPath { get; set; }

        [XmlElement("Theme")]
        public Theme Theme { get; set; }

        public Config()
        {
            TargetName = typeof(TargetDGen).FullName;
        }

        public void Read()
        {
            XmlSerializer xs = new XmlSerializer(typeof(Config));
            string path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\mdstudioOld";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                if (File.Exists(path + @"\config.xml"))
                {
                    StreamReader sr = new StreamReader(path + @"\config.xml");
                    try
                    {
                        Config config;

                        config = (Config)xs.Deserialize(sr);

                        TargetName = config.TargetName;
                        Asm68kPath = config.Asm68kPath;
                        AsPath = config.AsPath;
                        AssemblerIncludePaths = config.AssemblerIncludePaths;
                        EmuResolution = config.EmuResolution;
                        EmuRegion = config.EmuRegion;
                        Pal = config.Pal;
                        AutoOpenLastProject = config.AutoOpenLastProject;
                        LastProject = config.LastProject;
                        KeycodeUp = config.KeycodeUp;
                        KeycodeDown = config.KeycodeDown;
                        KeycodeLeft = config.KeycodeLeft;
                        KeycodeRight = config.KeycodeRight;
                        KeycodeA = config.KeycodeA;
                        KeycodeB = config.KeycodeB;
                        KeycodeC = config.KeycodeC;
                        KeycodeStart = config.KeycodeStart;

                        MegaUSBPath = config.MegaUSBPath;

                        Theme = config.Theme;

                        sr.Close();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }

        }
        public void Save()
        {
            XmlSerializer xs = new XmlSerializer(typeof(Config));
            string path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\mdstudioOld";

            try
            {
                StreamWriter sw = new StreamWriter(path + @"\config.xml");
                xs.Serialize(sw, this);

                sw.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }
    }
}
