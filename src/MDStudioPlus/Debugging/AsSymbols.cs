using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MDStudioPlus.Debugging
{
    public class AsSymbols : ISymbols
    {
        private struct AddressEntry
        {
            public uint Address;
            public int LineFrom;
            public int LineTo;
        }

        private struct FilenameSection
        {
            public string Filename;
            public List<AddressEntry> Addresses;
        }

        private struct SymbolSegment
        {
            public string Name;
            public string Type;
            public string Value;
            public int DateSize;
            public int Unused;
        }

        public Dictionary<string, DebugInfo> AddressToFileLine => Addr2FileLine;
        public List<SymbolEntry> Symbols { get; private set; } = new List<SymbolEntry>();

        private List<FilenameSection> FilenameSections = new List<FilenameSection>();
        private Dictionary<string, DebugInfo> Addr2FileLine;
        private string projectPath;

        public AsSymbols(string projectPath)
        {
            this.projectPath = projectPath;
        }

        public uint GetAddress(string filename, int lineNumber)
        {
            string path = System.IO.Path.GetFullPath(filename).ToUpper();
            int sectionIdx = FilenameSections.FindIndex(element => element.Filename == path);
            if (sectionIdx >= 0)
            {
                int addressIdx = FilenameSections[sectionIdx].Addresses.FindIndex(element => (lineNumber >= element.LineFrom && lineNumber <= element.LineTo));
                if (addressIdx >= 0)
                {
                    return FilenameSections[sectionIdx].Addresses[addressIdx].Address;
                }
            }

            return 0;
        }

        /// <summary>
        /// NOTE: This is extremely buggy and likely to fail
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public DebugInfo GetFileLine(uint address)
        {
            KeyValuePair<string, DebugInfo> info = Addr2FileLine.FirstOrDefault(t => t.Key.Split(':')[2] == $"{address}");
            if (info.Value != null)
            {
                return info.Value;
            }
            else
            {
                return new DebugInfo("", -1, -1);
            }
        }

        // TODO: Needs to also consider either the section index or the filename
        // otherwise, it can potentially get the wrong line valuse from the wrong
        // source file.
        public DebugInfo GetFileLine(string filename, uint address)
        {
            KeyValuePair<string, DebugInfo> info = Addr2FileLine.FirstOrDefault(t => t.Value.Filename == filename.ToUpper() && t.Key == $"{filename.ToUpper()}:{address}");
            if (info.Value != null)
            {
                return info.Value;
            }
            else
            {
                return new DebugInfo("", -1,  -1 );
            }
        }

        public bool Read(string filename, string[] filesToExclude = null)
        {
            string fileContents = File.ReadAllText(filename);

            if (fileContents.Length > 0)
            {
                List<int> indexes = new List<int>();
                List<string> segments = new List<string>();

                // get index of segments from file 
                int index = fileContents.IndexOf("Segment");
                indexes.Add(index);         // should be zero.

                int currentIndex = 0;
                while (currentIndex != -1)
                {
                    currentIndex = fileContents.IndexOf("Symbols in Segment", currentIndex);

                    // -1 means that an index couldn't be found.
                    if (currentIndex == -1)
                    {
                        break;
                    }

                    // index found, add it to the list and increment it for the next search
                    indexes.Add(currentIndex++);
                }

                // add the file size as the last index (might remove this)
                indexes.Add(fileContents.Length);

                // collect the segments
                for (int i = 0; i < indexes.Count - 1; i++)
                {
                    segments.Add(fileContents.Substring(indexes[i], indexes[i + 1] - indexes[i]));
                }

                int currentLine = 1;

                // parse filenames and address
                string[] filenameSection = segments[0].Split(new string[] { "File" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var data in filenameSection)
                {
                    string[] segmentdata = data.Split('\n', '\r');
                    currentLine = ReadFilenameData(segmentdata, currentLine, filesToExclude);
                }

                // parse through the symbol data
                for (int i = 1; i < segments.Count; i++)
                {
                    string[] symbolSegData = segments[i].Split('\n', '\r');
                    currentLine = ReadSymbolData(symbolSegData, currentLine);
                }
            }

            //Build address to file/line map
            Addr2FileLine = new Dictionary<string, DebugInfo>();

            foreach (FilenameSection section in FilenameSections)
            {
                foreach (AddressEntry address in section.Addresses)
                {
                    if (!Addr2FileLine.ContainsKey($"{section.Filename}:{address.Address}"))
                    {
                        Addr2FileLine[$"{section.Filename}:{address.Address}"] = new DebugInfo(section.Filename, address.LineFrom, address.LineTo);
                    }
                }
            }

            return true;
        }

        public void Clear()
        {
            FilenameSections?.Clear();
            Symbols?.Clear();
            Addr2FileLine?.Clear();
        }

        private int ReadSymbolData(string[] data, int currentLine)
        {
            int currentIndex = 0;

            while (currentIndex < data.Length)
            {
                string line = data[currentIndex++];
                if (line.Contains("Symbols in Segment"))
                {
                    // don't care about these symbols for now
                    if (line.Contains("NOTHING"))
                    {
                        return currentLine;
                    }

                    continue;
                }
                else if (line == "" || line == " ")
                {
                    continue;
                }
                else
                {
                    var symbolInfo = line.Split(' ', '\t').Where(l => l != String.Empty).ToArray();

                    var symbol = new SymbolEntry();
                    symbol.name = symbolInfo[0];

                    // AS' MAP file, for some reason, writes out RAM addresses as 64 bit integers. So, we parse
                    // the 64 bit value, and then cast it back to a 32 bit value. AND by 0xFFFFFFFF, 
                    // probably doesn't matter, but I do it remove the extra junk.
                    var value = UInt64.Parse(symbolInfo[2], System.Globalization.NumberStyles.HexNumber);
                    symbol.address = (uint)(value & 0xFFFFFFFF);
                    Symbols.Add(symbol);
                }
            }
            return currentLine;
        }

        private int ReadFilenameData(string[] data, int currentLine, string[] filesToExclude)
        {
            int currentIndex = 0;
            FilenameSection filenameSection = new FilenameSection();
            List<AddressEntry> addresses = new List<AddressEntry>();

            while (currentIndex < data.Length)
            {
                string line = data[currentIndex++];
                if (line.Contains("Segment"))
                {
                    return currentLine;
                }
                else if (line == "" || line == " ")
                {
                    continue; // ignore this
                }
                else if (line.Contains("\\") || line.Contains("/") || Path.GetExtension(line) == ".asm")
                {

                    int fileIndex = line.IndexOf(' ') + 1;

                    // sigh.. sometimes the filename is the full path sometimes its just the filename.
                    // since we rely on full paths, we need to catch this and adjust it
                    string filename = line.Substring(fileIndex);
                    if (!File.Exists(filename))
                    {
                        filename = $"{projectPath}\\{filename}";
                    }

                    filename = filename.ToUpper().Replace("/", "\\");

                    if (filesToExclude != null && filesToExclude.Contains(filename))
                    {
                        return currentLine;
                    }

                    int sectionIdx = FilenameSections.FindIndex(element => element.Filename == filename);
                    if (sectionIdx >= 0)
                    {
                        filenameSection = FilenameSections[sectionIdx];
                        currentLine = filenameSection.Addresses[filenameSection.Addresses.Count - 1].LineTo;
                    }
                    else
                    {
                        // Get File name
                        filenameSection.Filename = filename.ToUpper();

                        // reset line counter
                        currentLine = 1;
                    }
                    currentIndex++;
                }
                else
                {
                    var addressPairs = line.Split(' ', '\t').Where(l => l != "");
                    foreach (var pair in addressPairs)
                    {
                        AddressEntry address = new AddressEntry();
                        string[] split = pair.Split(':');
                        address.LineTo = Convert.ToInt32(split[0]);

                        // Parsing as a 64 bit value, just in case. 
                        var value = UInt64.Parse(split[1], System.Globalization.NumberStyles.HexNumber);
                        address.Address = (uint)(value & 0xFFFFFFFF);

                        address.LineFrom = currentLine;
                        currentLine = address.LineTo;
                        addresses.Add(address);
                    }
                }
            }

            if (filenameSection.Addresses == null)
            {
                filenameSection.Addresses = new List<AddressEntry>();
            }

            filenameSection.Addresses.AddRange(addresses);

            if (!FilenameSections.Any(element => element.Filename == filenameSection.Filename))
            {
                FilenameSections.Add(filenameSection);
            }

            return currentLine;
        }

    }
}
