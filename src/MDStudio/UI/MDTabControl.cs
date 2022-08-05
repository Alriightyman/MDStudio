using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MDStudio;
using static MDStudio.Themes;

namespace MDStudio.UI
{
    public class MDTabControl : TabControl
    {
        public Theme Theme { get; set; } = Theme.Light;

        public MDTabControl() : base()
        {
            this.DrawMode = TabDrawMode.OwnerDrawFixed;
            SetStyle(ControlStyles.UserPaint, true);           
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);           

            // Draw the border
             ControlPaint.DrawBorder(e.Graphics, ClientRectangle,
                       Themes.BackColor, 30, ButtonBorderStyle.Solid,
                       Themes.BackColor, 30, ButtonBorderStyle.Solid,
                       Themes.BackColor, 30, ButtonBorderStyle.Solid,
                       Themes.BackColor, 30, ButtonBorderStyle.Solid);

            // Draw tabs but draw the selected tab last
            var last = this.SelectedTab;
            foreach (TabPage tab in TabPages)
            {
                // if this is the last tab, skip it. 
                if (tab == last)
                {
                    continue;
                }
                OnDrawItem(new DrawItemEventArgs(e.Graphics, this.Font, tab.Bounds, this.TabPages.IndexOf(tab), DrawItemState.None, Themes.ForeColor, Themes.BackColor));
            }

            if (last != null)
            {
                OnDrawItem(new DrawItemEventArgs(e.Graphics, this.Font, last.Bounds, this.SelectedIndex, DrawItemState.Selected, Themes.ForeColor, Themes.BackColor));
            }
        }        

        // Following code was adapted from: https://github.com/r-aghaei/TabControlWithCloseButtonAndAddButton
        // I am not using the "Add" button feature so that code has been removed. 
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
        private const int TCM_SETMINTABWIDTH = 0x1300 + 49;
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            SendMessage(this.Handle, TCM_SETMINTABWIDTH, IntPtr.Zero, (IntPtr)16);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            for (var i = 0; i < this.TabPages.Count; i++)
            {
                var tabRect = this.GetTabRect(i);

                // handle middle mouse button removal
                if (e.Button == MouseButtons.Middle)
                {
                    if (tabRect.Contains(e.Location))
                    {
                        this.TabPages.RemoveAt(i);
                        break;
                    }
                }

                tabRect.Inflate(-2, -2);
                var closeImage = Properties.Resources.Close;
                var imageRect = new Rectangle(
                    (tabRect.Right - closeImage.Width),
                    tabRect.Top + (tabRect.Height - closeImage.Height) / 2,
                    closeImage.Width,
                    closeImage.Height);

                if (imageRect.Contains(e.Location))
                {
                    this.TabPages.RemoveAt(i);
                    break;
                }
            }
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            base.OnDrawItem(e);

            // draw tab control
            TabPage tabPage = null;
            Rectangle tabRect = new Rectangle();

            tabPage = this.TabPages[e.Index];
            tabRect = this.GetTabRect(e.Index);

            // draw tab color
            Color color = e.State == DrawItemState.Selected ? ControlPaint.Light(Themes.BackColor) : Themes.BackColor;
            e.Graphics.FillRectangle(new SolidBrush(color), tabRect);

            // draw tab border
            if (e.State == DrawItemState.Selected)
            {                       
                ControlPaint.DrawBorder(e.Graphics, tabRect, 
                                        color, 1, ButtonBorderStyle.Outset,
                                        color, 1, ButtonBorderStyle.Outset,
                                        color, 1, ButtonBorderStyle.Outset,
                                        color, 0, ButtonBorderStyle.Outset);
            }
            else
            {
                ControlPaint.DrawBorder(e.Graphics, tabRect,
                                    ControlPaint.Light(color),1, ButtonBorderStyle.Solid,
                                    ControlPaint.Light(color),1, ButtonBorderStyle.Solid,
                                    ControlPaint.Light(color),1, ButtonBorderStyle.Solid,
                                    ControlPaint.Light(color),0, ButtonBorderStyle.Solid);
            }


            // draw X image (close icon)
            tabRect.Inflate(-2, -2);
            
            var closeImage = Properties.Resources.Close;
            e.Graphics.DrawImage(closeImage,
                (tabRect.Right - closeImage.Width),
                tabRect.Top + (tabRect.Height - closeImage.Height) / 2);

            // draw text
            TextRenderer.DrawText(e.Graphics, tabPage.Text, tabPage.Font,
                tabRect, Themes.ForeColor, TextFormatFlags.Left);

            DrawScrollBars(e, tabPage);
        }

        private void DrawScrollBars(DrawItemEventArgs e, TabPage tab)
        {
            var c = tab.Controls;
        }
    }
}
