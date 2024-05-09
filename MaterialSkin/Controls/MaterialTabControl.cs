using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MaterialSkin.Controls
{
    public class MaterialTabControl : TabControl, IMaterialControl
    {
        public MaterialTabControl()
        {
            Multiline = true;
        }

        [Browsable(false)]
        public int Depth { get; set; }

        [Browsable(false)]
        public MaterialSkinManager SkinManager => MaterialSkinManager.Instance;

        [Browsable(false)]
        public MouseState MouseState { get; set; }

        private bool _showHeader = false;
        [Category("Material Skin"), DefaultValue(false), Browsable(true)]
        public bool ShowHeader
        {
            get => _showHeader;
            set
            {
                _showHeader = value;
                Invalidate();
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x1328 && !DesignMode && !ShowHeader) m.Result = (IntPtr)1;
            else base.WndProc(ref m);

            if (m.Msg == 0xF)
            {
                using (Graphics g = Graphics.FromHwnd(m.HWnd))
                {
                    //Replace the outside white borders:
                    if (Parent != null)
                    {
                        g.SetClip(new Rectangle(0, 0, Width - 2, Height - 1), CombineMode.Exclude);
                        using (SolidBrush sb = new SolidBrush(Parent.BackColor))
                            g.FillRectangle(sb, new Rectangle(0,
                            ItemSize.Height + 2,
                            Width,
                            Height - (ItemSize.Height + 2)));
                    }

                    //Replace the inside white borders:
                    if (SelectedTab != null)
                    {
                        g.ResetClip();
                        Rectangle r = SelectedTab.Bounds;
                        g.SetClip(r, CombineMode.Exclude);
                        using (SolidBrush sb = new SolidBrush(SelectedTab.BackColor))
                            g.FillRectangle(sb, new Rectangle(r.Left - 3,
                                                              r.Top - 1,
                                                              r.Width + 4,
                                                              r.Height + 3));
                    }
                }
            }
        }
        
        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);

            e.Control.BackColor = System.Drawing.Color.White;
        }
    }
}
