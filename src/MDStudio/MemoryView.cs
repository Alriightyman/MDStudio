using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel.Design;

namespace MDStudio
{
    public partial class MemoryView : Form
    {
        private int vscrollPosition = 0;
        public MemoryView()
        {
            InitializeComponent();
            m_ByteViewer.SetDisplayMode(DisplayMode.Hexdump);
            
            // find the scrollbar and setup the scroll event
            foreach (var control in m_ByteViewer.Controls)
            {
                if (control is VScrollBar vScroll)
                {
                    vScroll.Scroll += VScroll_Scroll;
                }
            }
        }

        private void VScroll_Scroll(object sender, ScrollEventArgs e)
        {
            if (m_ByteViewer.GetBytes() != null)
            {
                vscrollPosition = e.NewValue;
                
            }
        }

        public void SetRamMemory(byte[] mem)
        {
            m_ByteViewer.SetBytes(mem);
            m_ByteViewer.SetStartLine(vscrollPosition);
        }

        private void m_ByteViewer_MouseHover(object sender, EventArgs e)
        {
            Point pos = m_ByteViewer.PointToClient(Cursor.Position);
            var count = m_ByteViewer.ColumnCount;
            var style = m_ByteViewer.ColumnStyles;           
        }
    }
}
