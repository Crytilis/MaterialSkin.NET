using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MaterialSkin.NET.Controls
{
    public class MaterialProgressBar : ProgressBar, IMaterialControl
    {
        #region Private variables
        private Color _color;
        private Brush _brush;
        #endregion
        #region Properties
        [Category("Material Skin")]
        [Browsable(true)]
        public bool UseFixedSize { get; set; }

        [Category("Material Skin")]
        [Browsable(true)]
        public bool UseAccentColor { get; set; }

        [Browsable(false)]
        public int Depth { get; set; }

        [Browsable(false)]
        public MaterialSkinManager SkinManager => MaterialSkinManager.Instance;

        [Browsable(false)]
        public MouseState MouseState { get; set; }

        [Category("Material Skin")]
        public Color BarColor
        {
            get => _color;
            set
            {
                _color = value;
                _brush = new SolidBrush(_color);
            }
        }
        #endregion

        public MaterialProgressBar() : this(MaterialSkinManager.Instance.ColorScheme.PrimaryColor)
        {
        }

        public MaterialProgressBar(Color barColor)
        {
            BarColor = barColor;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }


        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, width, UseFixedSize ? 5 : height, specified);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var doneProgress = (int)(Width * ((double)Value / Maximum));
            Brush b = Enabled
                ? SkinManager.ColorScheme.PrimaryBrush
                : new SolidBrush(DrawHelper.BlendColor( SkinManager.ColorScheme.PrimaryColor, SkinManager.SwitchOffDisabledThumbColor, 197));
            b = Enabled && UseAccentColor
                ? SkinManager.ColorScheme.AccentBrush
                : b;

            e.Graphics.FillRectangle(b, 0, 0, doneProgress, Height);
            e.Graphics.FillRectangle(SkinManager.BackgroundFocusBrush, doneProgress, 0, Width - doneProgress, Height);
        }
    }
}
