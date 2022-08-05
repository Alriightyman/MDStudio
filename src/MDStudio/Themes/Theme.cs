using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MDStudio
{
    public class Themes
    {
        public enum Theme { Light, Dark}
        public static Theme CurrentTheme { get; set; } = Theme.Light;

        public static Color BackColor => CurrentTheme == Theme.Light ? SystemColors.Control : Color.FromArgb(255, 45, 45, 45);
        public static Color ForeColor => CurrentTheme == Theme.Light ? SystemColors.ControlText : Color.White;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr,
        ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        internal static bool UseImmersiveDarkMode(IntPtr handle, bool enabled)
        {
            if (IsWindows10OrGreater(17763))
            {
                var attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
                if (IsWindows10OrGreater(18985))
                {
                    attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
                }

                int useImmersiveDarkMode = enabled ? 1 : 0;
                return DwmSetWindowAttribute(handle, attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;
            }

            return false;
        }

        private static bool IsWindows10OrGreater(int build = -1)
        {
            return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;
        }
    

        public struct Setting
        {
            public Color BackgroundColor;
            public Color Color;
            public bool Bold;
            public bool Italic;
        }

        public static Dictionary<string, Setting> EnvironmentColorsLight = new Dictionary<string, Setting>()
        {
            { "Default", new Setting() {Bold = false, Italic = false, Color = SystemColors.WindowText, BackgroundColor = SystemColors.Window } },
            { "Selection", new Setting() {Bold = false, Italic = false, Color = SystemColors.HighlightText, BackgroundColor = SystemColors.Highlight } },
            { "SelectionInactive", new Setting() {Bold = false, Italic = false, Color = Color.FromArgb(255,204,228,247), BackgroundColor = Color.FromArgb(255,102,174,231) } },
            { "VRuler", new Setting() {Bold = false, Italic = false, Color = SystemColors.ControlLight, BackgroundColor = SystemColors.Window } },
            { "InvalidLines", new Setting() {Bold = false, Italic = false, Color = Color.Red, BackgroundColor = Color.WhiteSmoke } },
            { "CaretMarker", new Setting() {Bold = false, Italic = false, Color = Color.FromArgb(255,255,255,204), BackgroundColor = Color.WhiteSmoke } },
            { "CaretLine", new Setting() {Bold = false, Italic = false, Color = SystemColors.ControlLight, BackgroundColor = SystemColors.Window } },
            { "LineNumbers", new Setting() {Bold = false, Italic = false, Color = SystemColors.ControlDark, BackgroundColor = SystemColors.Window } },
            { "FoldLine", new Setting() {Bold = false, Italic = false, Color = SystemColors.ControlDark, BackgroundColor = Color.WhiteSmoke } },
            { "FoldMarker", new Setting() {Bold = false, Italic = false, Color = SystemColors.WindowText, BackgroundColor = SystemColors.Window } },
            { "SelectedFoldLine", new Setting() {Bold = false, Italic = false, Color = SystemColors.WindowText, BackgroundColor = Color.WhiteSmoke } },
            { "EOLMarkers", new Setting() {Bold = false, Italic = false, Color = SystemColors.ControlLight, BackgroundColor = SystemColors.Window } },
            { "SpaceMarkers", new Setting() {Bold = false, Italic = false, Color = SystemColors.ControlLight, BackgroundColor = SystemColors.Window } },
            { "TabMarkers", new Setting() {Bold = false, Italic = false, Color = SystemColors.ControlLight, BackgroundColor = SystemColors.Window } },
        };

        public static Dictionary<string, Setting> EnvironmentColorsDark = new Dictionary<string, Setting>()
        {
            { "Default", new Setting() {Bold = false, Italic = false, Color = SystemColors.WindowText, BackgroundColor = SystemColors.Window } },
            { "Selection", new Setting() {Bold = false, Italic = false, Color = SystemColors.HighlightText, BackgroundColor = SystemColors.Highlight } },
            { "SelectionInactive", new Setting() {Bold = false, Italic = false, Color = Color.FromArgb(255,204,228,247), BackgroundColor = Color.FromArgb(255,102,174,231) } },
            { "VRuler", new Setting() {Bold = false, Italic = false, Color = SystemColors.ControlLight, BackgroundColor = SystemColors.Window } },
            { "InvalidLines", new Setting() {Bold = false, Italic = false, Color = Color.Red, BackgroundColor = Color.WhiteSmoke } },
            { "CaretMarker", new Setting() {Bold = false, Italic = false, Color = Color.FromArgb(255,255,255,204), BackgroundColor = Color.WhiteSmoke } },
            { "CaretLine", new Setting() {Bold = false, Italic = false, Color = SystemColors.ControlLight, BackgroundColor = SystemColors.Window } },
            { "LineNumbers", new Setting() {Bold = false, Italic = false, Color = SystemColors.ControlDark, BackgroundColor = SystemColors.Window } },
            { "FoldLine", new Setting() {Bold = false, Italic = false, Color = SystemColors.ControlDark, BackgroundColor = Color.WhiteSmoke } },
            { "FoldMarker", new Setting() {Bold = false, Italic = false, Color = SystemColors.WindowText, BackgroundColor = SystemColors.Window } },
            { "SelectedFoldLine", new Setting() {Bold = false, Italic = false, Color = SystemColors.WindowText, BackgroundColor = Color.WhiteSmoke } },
            { "EOLMarkers", new Setting() {Bold = false, Italic = false, Color = SystemColors.ControlLight, BackgroundColor = SystemColors.Window } },
            { "SpaceMarkers", new Setting() {Bold = false, Italic = false, Color = SystemColors.ControlLight, BackgroundColor = SystemColors.Window } },
            { "TabMarkers", new Setting() {Bold = false, Italic = false, Color = SystemColors.ControlLight, BackgroundColor = SystemColors.Window } },
        };
    }
}
