using AvalonDock.Themes;
using MDStudioPlus.Targets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MDStudioPlus
{
    [XmlRoot("Root")]
    public class Config
    {
        public string TargetName { get; set; }
        public bool AutoOpenLastProject = true;
        public string LastProject { get; set; }

        public string AsPath { get; set; }
        public string Asm68kPath { get; set; }
        public string[] AssemblerIncludePaths { get; set; }
        
        public string EmuResolution { get; set; }
        public int EmuRegion { get; set; }
        public bool Pal = false;

        // Index into SDL_Keycode.Keycode enum
        public int KeycodeUp { get; set; } = (int)SDL_Keycode.Keycode.SDLK_UP;
        public int KeycodeDown { get; set; } = (int)SDL_Keycode.Keycode.SDLK_DOWN;
        public int KeycodeLeft { get; set; } = (int)SDL_Keycode.Keycode.SDLK_LEFT;
        public int KeycodeRight { get; set; } = (int)SDL_Keycode.Keycode.SDLK_RIGHT;
        public int KeycodeA { get; set; } = (int)SDL_Keycode.Keycode.SDLK_a;
        public int KeycodeB { get; set; } = (int)SDL_Keycode.Keycode.SDLK_s;
        public int KeycodeC { get; set; } = (int)SDL_Keycode.Keycode.SDLK_d;
        public int KeycodeStart { get; set; } = (int)SDL_Keycode.Keycode.SDLK_SPACE;
        public string MegaUSBPath { get; set; }        
        public string Theme { get; set; }
        public Config()
        {
            TargetName = typeof(TargetDGen).FullName;
        }

        public void Read()
        {
            XmlSerializer xs = new XmlSerializer(typeof(Config));
            string path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\mdstudio";

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
            string path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\mdstudio";

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
