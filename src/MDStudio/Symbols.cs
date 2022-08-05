using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Data                         ;
using System.IO;

namespace MDStudio
{
    namespace Debugging
    {
        public struct SymbolEntry
        {
            public uint address;
            public string name;
        }

        public interface ISymbols
        {
            Dictionary<uint, Tuple<string, int, int>> AddressToFileLine { get; }
            List<SymbolEntry> Symbols { get; }
            uint GetAddress(string filename, int lineNumber);
            Tuple<string, int, int> GetFileLine(uint address);
            bool Read(string filename, string[] filesToExclude);
        }

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

            public Dictionary<uint, Tuple<string, int, int>> AddressToFileLine => Addr2FileLine;
            public List<SymbolEntry> Symbols { get; private set; } = new List<SymbolEntry>();
            private List<FilenameSection> FilenameSections = new List<FilenameSection>();
            private Dictionary<uint, Tuple<string, int, int>> Addr2FileLine;

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

            public Tuple<string, int, int> GetFileLine(uint address)
            {
                if (Addr2FileLine.ContainsKey(address))
                {
                    return Addr2FileLine[address];
                }
                else
                {
                    return new Tuple<string, int, int>("", -1, -1);
                }
            }

            public bool Read(string filename,string[] filesToExclude = null)
            {
                // clear symbol information first
                Clear();

                string fileContents = System.IO.File.ReadAllText(filename);

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
                Addr2FileLine = new Dictionary<uint, Tuple<string, int, int>>();

                foreach (FilenameSection section in FilenameSections)
                {
                    foreach (AddressEntry address in section.Addresses)
                    {
                        if (!Addr2FileLine.ContainsKey(address.Address))
                        {
                            Addr2FileLine[address.Address] = new Tuple<string, int, int>(section.Filename, address.LineFrom, address.LineTo);
                        }
                    }
                }

                return true;
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
                        string filename = line.Substring(fileIndex).ToUpper().Replace("/","\\");

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
                            address.LineTo = Convert.ToInt32(split[0]) - 1;

                            // Parsing as a 64 bit value, just in case. 
                            var value = UInt64.Parse(split[1], System.Globalization.NumberStyles.HexNumber);
                            address.Address = (uint)(value & 0xFFFFFFFF);

                            address.LineFrom = currentLine - 1;
                            currentLine = address.LineTo;
                            addresses.Add(address);
                        }
                    }
                }

                if(filenameSection.Addresses == null)
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

            private void Clear()
            {
                FilenameSections?.Clear();
                Symbols?.Clear();
                Addr2FileLine?.Clear();
            }
        }

        public class Asm68kSymbols : ISymbols
        {
            public uint GetAddress(string filename, int lineNumber)
            {
                //TODO: Slow
                string path = System.IO.Path.GetFullPath(filename).ToUpper();
                int sectionIdx = m_Filenames.FindIndex(element => element.filename == path);
                if (sectionIdx >= 0)
                {
                    int addressIdx = m_Filenames[sectionIdx].addresses.FindIndex(element => (lineNumber >= element.lineFrom && lineNumber <= element.lineTo));
                    if (addressIdx >= 0)
                    {
                        return m_Filenames[sectionIdx].addresses[addressIdx].address;
                    }
                }

                return 0;
            }

            public Tuple<string, int, int> GetFileLine(uint address)
            {
                if (m_Addr2FileLine.ContainsKey(address))
                {
                    return m_Addr2FileLine[address];
                }
                else
                {
                    return new Tuple<string, int, int>("", -1, -1);
                }
            }

            public Dictionary<uint, Tuple<string, int, int>> AddressToFileLine { get { return m_Addr2FileLine; } }

            private struct AddressEntry
            {
                public uint address;
                public byte flags;
                public int lineFrom;
                public int lineTo;
            }

            private struct FilenameSection
            {
                public string filename;
                public List<AddressEntry> addresses;
            }

            public List<SymbolEntry> Symbols { get; private set; }
            private List<FilenameSection> m_Filenames;
            private Dictionary<uint, Tuple<string, int, int>> m_Addr2FileLine;
            private string m_AssembledFile;

            private enum ChunkId : byte
            {
                Equate = 0x01,              // EQU name
                Symbol = 0x2,               // Symbol table entry
                Address = 0x80,             // An address of next line
                AddressWithCount = 0x82,    // An address with line count
                Filename = 0x88,            // A filename with start address and line count
                EndOfSection = 0x8A,        // End of section
            }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            private struct FileHeader
            {
                public uint unknown1;
                public uint unknown2;
            }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            private struct FilenameHeader
            {
                public byte firstLine;
                public byte flags;
                public byte unknown;
                public short length;
            }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            private struct ChunkHeader
            {
                public uint payload;
                public ChunkId chunkId;
            }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            private struct SymbolChunk
            {
                public uint address;
                public byte flags;
                public byte stringLen;
            }

            private int Serialise<T>(ref IntPtr bytes, out T value) where T : struct
            {
                value = (T)Marshal.PtrToStructure(bytes, typeof(T));
                bytes += Marshal.SizeOf<T>();
                return Marshal.SizeOf<T>();
            }

            private int Serialise(ref IntPtr bytes, int length, out string value)
            {
                var byteArray = new byte[length + 1];
                System.Runtime.InteropServices.Marshal.Copy(bytes, byteArray, 0, length);
                value = System.Text.Encoding.Default.GetString(byteArray, 0, length);
                byteArray[length] = 0;
                bytes += length;
                return length;
            }

            public bool Read(string filename, string[] filesToExclude = null)
            {
                //try
                {
                    Symbols = new List<SymbolEntry>();
                    m_Filenames = new List<FilenameSection>();
                    byte[] data = System.IO.File.ReadAllBytes(filename);

                    if (data.Length > 0)
                    {
                        GCHandle pinnedData = GCHandle.Alloc(data, GCHandleType.Pinned);
                        IntPtr stream = pinnedData.AddrOfPinnedObject();

                        FilenameHeader filenameHeader = new FilenameHeader();
                        ChunkHeader chunkHeader = new ChunkHeader();
                        FilenameSection filenameSection = new FilenameSection();
                        AddressEntry addressEntry = new AddressEntry();
                        SymbolChunk symbolChunk = new SymbolChunk();
                        SymbolEntry symbolEntry = new SymbolEntry();
                        string readString;

                        int bytesRead = 0;
                        int totalBytes = data.Length;
                        bool continuing = false;
                        // Symbol lines are 1-based, text editor lines are 0-based
                        int currentLine = 1;

                        //Read file header
                        FileHeader fileHeader = new FileHeader();
                        bytesRead += Serialise(ref stream, out fileHeader);

                        void CheckSize(int chunkLength)
                        {
                            if ((bytesRead + chunkLength) > totalBytes)
                            {
                                throw new Exception("Bad chunk length or malformed file");
                            }
                        }
                        StringBuilder builder = new StringBuilder();
                        using (StringWriter fs = new StringWriter(builder))
                        {
                            //Iterate over chunks
                            while (bytesRead < data.Length)
                            {
                                //Read chunk header
                                bytesRead += Serialise(ref stream, out chunkHeader);

                                //What is it?
                                switch (chunkHeader.chunkId)
                                {
                                    case ChunkId.Filename:
                                        {
                                            //Read filename header
                                            bytesRead += Serialise(ref stream, out filenameHeader);
                                            filenameHeader.length = Endian.Swap(filenameHeader.length);

                                            //Read string
                                            CheckSize(filenameHeader.length);
                                            bytesRead += Serialise(ref stream, filenameHeader.length, out readString);

                                            readString = readString.Trim();

                                            fs.WriteLine(readString);
                                            fs.WriteLine($"\tFirst Line: {filenameHeader.firstLine}");                                            

                                            if (filenameHeader.flags == 0x1)
                                            {
                                                //This is the filename passed for assembly
                                                m_AssembledFile = readString;
                                            }
                                            else
                                            {
                                                //If filename already exists, continue adding data to it
                                                int sectionIdx = m_Filenames.FindIndex(element => element.filename == readString);
                                                if (sectionIdx >= 0)
                                                {
                                                    //Continue
                                                    filenameSection = m_Filenames[sectionIdx];

                                                    //Fetch line counter
                                                    currentLine = filenameSection.addresses[filenameSection.addresses.Count - 1].lineTo;
                                                    continuing = true;
                                                }
                                                else
                                                {
                                                    //This is the first address in a filename chunk
                                                    filenameSection = new FilenameSection();
                                                    filenameSection.addresses = new List<AddressEntry>();

                                                    try
                                                    {
                                                        string pathSanitised = System.IO.Path.GetFullPath(readString).ToUpper();
                                                        filenameSection.filename = pathSanitised;
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        Console.WriteLine("Exception caught sanitising symbol path \'" + readString + "\': " + e.Message);
                                                    }

                                                    continuing = false;
                                                    //Reset line counter
                                                    currentLine = 1;
                                                }

                                                //Chunk payload contains address
                                                addressEntry.address = chunkHeader.payload;
                                                if (continuing)
                                                {
                                                    addressEntry.lineFrom = currentLine;
                                                    addressEntry.lineTo = currentLine+1;
                                                    currentLine = currentLine + 1;
                                                }
                                                else
                                                {
                                                    addressEntry.lineFrom = currentLine - 1;
                                                    addressEntry.lineTo = filenameHeader.firstLine - 1;
                                                    currentLine = filenameHeader.firstLine;
                                                }

                                                filenameSection.addresses.Add(addressEntry);

                                                fs.WriteLine($"\t{addressEntry.address}:{addressEntry.lineFrom}:{addressEntry.lineTo}");
                                                //Next
                                                currentLine++;

                                                //Add to filename list
                                                m_Filenames.Add(filenameSection);
                                            }

                                            break;
                                        }

                                    case ChunkId.Address:
                                        {
                                            //Chunk payload contains address for a single line
                                            addressEntry.address = chunkHeader.payload;

                                            //Set line range
                                            addressEntry.lineFrom = currentLine - 1;
                                            addressEntry.lineTo = currentLine - 1;

                                            //Next
                                            currentLine++;

                                            //Add
                                            filenameSection.addresses.Add(addressEntry);
                                            fs.WriteLine($"\t{addressEntry.address}:{addressEntry.lineFrom}:{addressEntry.lineTo}");
                                            break;
                                        }

                                    case ChunkId.AddressWithCount:
                                        {
                                            //Chunk payload contains address for a rage of lines
                                            addressEntry.address = chunkHeader.payload;

                                            //Read line count
                                            byte lineCount = 0;
                                            bytesRead += Serialise(ref stream, out lineCount);

                                            //Set line range
                                            addressEntry.lineFrom = currentLine - 1;
                                            addressEntry.lineTo = currentLine + (lineCount - 1) - 1;
                                            fs.WriteLine($"\t{addressEntry.address}:{addressEntry.lineFrom}:{addressEntry.lineTo}");
                                            //Next
                                            currentLine += lineCount;

                                            //Add
                                            filenameSection.addresses.Add(addressEntry);

                                            break;
                                        }

                                    case ChunkId.Equate:
                                        {
                                            //Read equate string length
                                            byte stringLength = 0;
                                            bytesRead += Serialise(ref stream, out stringLength);

                                            //Read string
                                            string str;
                                            CheckSize(stringLength);
                                            bytesRead += Serialise(ref stream, stringLength, out str);

                                            Console.WriteLine($"EQU: {str}");

                                            break;
                                        }

                                    case ChunkId.Symbol:
                                        {
                                            //Read symbol string length
                                            byte stringLength = 0;
                                            bytesRead += Serialise(ref stream, out stringLength);

                                            //Read string
                                            CheckSize(stringLength);
                                            bytesRead += Serialise(ref stream, stringLength, out symbolEntry.name);

                                            //Payload contains address
                                            symbolEntry.address = chunkHeader.payload;
                                            fs.WriteLine($"\t{symbolEntry.name}:{symbolEntry.address}");
                                            Symbols.Add(symbolEntry);

                                            break;
                                        }

                                    case ChunkId.EndOfSection:
                                        //Payload contains section size
                                        break;

                                    default:
                                        short mysteryWord = 0;
                                        bytesRead += Serialise(ref stream, out mysteryWord);
                                        break;
                                }
                            }
                        }
                        pinnedData.Free();

                        File.WriteAllText("D:\\mapfile.txt", builder.ToString());

                        //Build address to file/line map
                        m_Addr2FileLine = new Dictionary<uint, Tuple<string, int, int>>();

                        foreach (FilenameSection section in m_Filenames)
                        {
                            foreach (AddressEntry address in section.addresses)
                            {
                                if (!m_Addr2FileLine.ContainsKey(address.address))
                                {
                                    m_Addr2FileLine[address.address] = new Tuple<string, int, int>(section.filename, address.lineFrom, address.lineTo);
                                }
                            }
                        }

                        return true;
                    }
                }
                //catch (Exception e)
                //{
                //    Console.WriteLine(e.Message);
                //}

                return false;
            }
        }
    }
}
