using System;
using System.Collections.Generic;

namespace MDStudioPlus.Debugging
{
    public struct SymbolEntry
    {
        public uint address;
        public string name;
    }

    public class DebugInfo
    {
        public string Filename;
        public int LineTo;
        public int LineFrom;

        public DebugInfo(string filename, int lineFrom, int lineTo)
        {
            Filename = filename;
            LineTo = lineTo;
            LineFrom = lineFrom;
        }
    }

    public interface ISymbols
    {
        Dictionary<string, DebugInfo> AddressToFileLine { get; }
        List<SymbolEntry> Symbols { get; }
        uint GetAddress(string filename, int lineNumber);
        DebugInfo GetFileLine(string filename, uint address);
        DebugInfo GetFileLine(uint address);
        bool Read(string filename, string[] filesToExclude);
    }
}
