using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace MaterialSkin.Controls
{
    public partial class MaterialDropDownColorPicker : DropDownControl, INotifyPropertyChanged
    {
        #region Events
        public new event PropertyChangedEventHandler PropertyChanged;
        [Category("Action")]
        public event EventHandler ColorChanged;
        #endregion

        #region Private variables
        private MaterialColorPicker objColorControl;
        private Color _Color;
        private Rectangle ColorRect;
        #endregion

        #region Property
        public override Color BackColor { get { return Parent == null ? SkinManager.BackdropColor : Parent.BackColor; } set { } }

        [Bindable(true), RefreshProperties(RefreshProperties.All)]
        [Browsable(true)]
        public Color Color
        {
            get { return _Color; }
            set
            {
                _Color = value;
                objColorControl.Value = _Color;
                ColorChanged?.Invoke(this, EventArgs.Empty);
                NotifyPropertyChanged();
            }
        }
        #endregion

        #region Constructor
        public MaterialDropDownColorPicker()
        {
            InitializeComponent();
            objColorControl = new MaterialColorPicker();
            Color = SkinManager.ColorScheme.AccentColor;
            objColorControl.onColorChanged += objDateControl_onColorChanged;
            InitializeDropDown(objColorControl);
            IconToShow = "\ue40a";
            ShowIcon = true;
            UseSmallHint = true;
            drawHintBackground = true;
        }
        #endregion

        #region Override methods
        protected override void PaintSomething(PaintEventArgs e)
        {
            base.PaintSomething(e);
            ColorRect = new Rectangle();
            ColorRect.Location = new Point(1, 1);
            ColorRect.Size = new Size((int)(Width - 18), (int)(Height * 0.9));

            e.Graphics.FillRectangle(new SolidBrush(Color), ColorRect);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }
        #endregion

        #region Eventhandler methods
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void objDateControl_onColorChanged(Color newColor)
        {
            Color = newColor;
            Invalidate();
        }
        #endregion
    }
}
