using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

#pragma warning disable CA1416

namespace MaterialSkin.Controls
{
    public sealed class MaterialFlowPanel : FlowLayoutPanel, IMaterialControl, ICustomSkinManager
    {
        #region HiddenDesignerProps

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Point AutoScrollOffset { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        // ReSharper disable once UnusedMember.Local
        private new Size AutoScrollMinSize { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        // ReSharper disable once UnusedMember.Local
        private new Padding AutoScrollMargin { get; set; }

        #endregion

        #region PrivateProps

        private bool _autoScroll;
        private int _prevVScrollValue;
        private int _prevHScrollValue;
        private bool _drawShadows;
        private bool _shadowDrawEventSubscribed;
        private bool _isStandalone;
        private readonly MaterialScrollBar _vScrollBar;
        private readonly MaterialScrollBar _hScrollBar;
        private Control _oldParent;
        private MaterialSkinManager _skinManager;
        private Color _accentColor = MaterialSkinManager.Instance.ColorScheme.AccentColor;
        private Color _primaryColor = MaterialSkinManager.Instance.ColorScheme.PrimaryColor;
        private Color _textColor = MaterialSkinManager.Instance.ColorScheme.TextColor;

        #endregion

        #region PublicProps

        [Browsable(false)]
        public int Depth { get; set; }

        [Browsable(false)]
        public MaterialSkinManager SkinManager
        {
            get => _skinManager ?? MaterialSkinManager.Instance;
            set
            {
                _skinManager = value;
                UpdateInternalComponentSkinManager();
            }
        }

        public MaterialSkinManager CustomSkinManager
        {
            get => SkinManager;
            set => SkinManager = value;
        }

        [Browsable(false)]
        public MouseState MouseState { get; set; }

        #endregion

        #region DesignerProps

        [Category("Layout")]
        public new bool AutoScroll
        {
            get => _autoScroll;
            set
            {
                _autoScroll = value;
                AdjustScrollBars();
            }
        }

        [Category("Material Skin"), DisplayName("Use Standalone")]
        [Description("Determines if the material component uses global or local instance of SkinManager.")]
        public bool UseStandalone
        {
            get => _isStandalone;
            set
            {
                _isStandalone = value;
                SkinManager = _isStandalone ? MaterialSkinManager.ControlInstance : MaterialSkinManager.Instance;
            }
        }


        [DefaultValue(true)]
        [Category("Material Skin"), DisplayName("Draw Shadows")]
        [Description("Draw Shadows around control")]
        public bool DrawShadows
        {
            get => _drawShadows;
            set
            {
                _drawShadows = value;
                Invalidate();
            }
        }

        #endregion

        #region Constructor

        public MaterialFlowPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            SuspendLayout();
            _vScrollBar = new MaterialScrollBar
            {
                HighlightOnWheel = false,
                LargeChange = 10,
                Maximum = 100,
                Minimum = 0,
                MouseWheelBarPartitions = 10,
                Orientation = MaterialScrollOrientation.Vertical,
                ScrollbarSize = SystemInformation.VerticalScrollBarWidth,
                SmallChange = 1,
                UseBarColor = false,
                UseAccentColor = true,
                Visible = false
            };
            _hScrollBar = new MaterialScrollBar
            {
                HighlightOnWheel = false,
                LargeChange = 10,
                Maximum = 100,
                Minimum = 0,
                MouseWheelBarPartitions = 10,
                Orientation = MaterialScrollOrientation.Horizontal,
                ScrollbarSize = SystemInformation.HorizontalScrollBarHeight,
                SmallChange = 1,
                UseBarColor = false,
                UseAccentColor = true,
                Visible = false
            };

            Controls.Add(_vScrollBar);
            Controls.Add(_hScrollBar);
            ResumeLayout(false);
            PerformLayout();

            AdjustScrollBars();

            BackColor = SkinManager.BackgroundColor;
            ForeColor = SkinManager.TextHighEmphasisColor;
            DrawShadows = true;
            Padding = new Padding(24, 64, 24, 16);
            Margin = new Padding( 3, 16,  3, 16);
            Size = new Size(480, 200);
            _hScrollBar.ValueChanged += HScrollBarOnValueChanged;
            _vScrollBar.ValueChanged += VScrollBarOnValueChanged;
        }

        #endregion

        #region Methods

        private void UpdateInternalComponentSkinManager()
        {
            foreach (Control control in Controls)
            {
                if (control is ICustomSkinManager customSkinManagerControl)
                {
                    customSkinManagerControl.CustomSkinManager = SkinManager;
                }
            }
        }

        private void SetupSkinManager()
        {
            if (!UseStandalone)
            {
                SkinManager = MaterialSkinManager.Instance;
            }
            else
            {
                SkinManager.ColorScheme = new ColorScheme(_primaryColor, _primaryColor, _primaryColor, _accentColor, TextShade.White);
            }
        }

        private void VScrollBarOnValueChanged(object sender, int newValue)
        {
            var deltaY = _vScrollBar.Value - _prevVScrollValue; // Calculate the vertical delta
            foreach (Control ctrl in Controls)
            {
                if (ctrl != _vScrollBar && ctrl != _hScrollBar) // Avoid moving the scrollbars
                {
                    ctrl.Top -= deltaY; // Adjust the Top position
                }
            }
            _prevVScrollValue = _vScrollBar.Value; // Update the previous value
        }

        private void HScrollBarOnValueChanged(object sender, int newValue)
        {
            var deltaX = _hScrollBar.Value - _prevHScrollValue; // Calculate the horizontal delta
            foreach (Control ctrl in Controls)
            {
                if (ctrl != _vScrollBar && ctrl != _hScrollBar) // Avoid moving the scrollbars
                {
                    ctrl.Left -= deltaX; // Adjust the Left position
                }
            }
            _prevHScrollValue = _hScrollBar.Value; // Update the previous value
        }

        private void DrawShadowOnParent(object sender, PaintEventArgs e)
        {
            if (Parent == null)
            {
                RemoveShadowPaintEvent((Control)sender, DrawShadowOnParent);
                return;
            }

            if (!_drawShadows || Parent == null) return;

            // paint shadow on parent
            var gp = e.Graphics;
            var rect = new Rectangle(Location, ClientRectangle.Size);
            gp.SmoothingMode = SmoothingMode.AntiAlias;
            DrawHelper.DrawSquareShadow(gp, rect);
        }


        private void AddShadowPaintEvent(Control control, PaintEventHandler shadowPaintEvent)
        {
            if (_shadowDrawEventSubscribed) return;
            control.Paint += shadowPaintEvent;
            control.Invalidate();
            _shadowDrawEventSubscribed = true;
        }

        private void RemoveShadowPaintEvent(Control control, PaintEventHandler shadowPaintEvent)
        {
            if (!_shadowDrawEventSubscribed) return;
            control.Paint -= shadowPaintEvent;
            control.Invalidate();
            _shadowDrawEventSubscribed = false;
        }

        private void AdjustScrollBars()
        {
            // Ensure that scrollbars are initialized before adjusting their properties.
            if (_vScrollBar == null || _hScrollBar == null)
                return;

            var contentSize = GetContentSize();
            
            if (AutoScroll)
            {
                _vScrollBar.Visible = contentSize.Height > ClientSize.Height;
                _hScrollBar.Visible = contentSize.Width > ClientSize.Width;
            }

            if (_hScrollBar.Visible)
            {
                _hScrollBar.Minimum = 0;
                _hScrollBar.SmallChange = Width / 20;
                _hScrollBar.LargeChange = Width / 10;

                _hScrollBar.Maximum = contentSize.Width - ClientSize.Width;

                if (_vScrollBar.Visible)
                {
                    _hScrollBar.Maximum += _vScrollBar.Width;
                }

                _hScrollBar.Maximum += _hScrollBar.LargeChange;
            }

            if (!_vScrollBar.Visible) return;
            _vScrollBar.Minimum = 0;
            _vScrollBar.SmallChange = Height / 20;
            _vScrollBar.LargeChange = Height / 10;

            _vScrollBar.Maximum = contentSize.Height - ClientSize.Height;

            if (_hScrollBar.Visible)
            {
                _hScrollBar.Maximum += _vScrollBar.Width;
            }

            _vScrollBar.Maximum += _vScrollBar.LargeChange;
        }

        private Size GetContentSize()
        {
            // Initialize variables to track maximum width and height.
            var maxWidth = 0;
            var maxHeight = 0;

            // Loop through all child controls within the container.
            foreach (Control control in Controls)
            {
                if (control == _vScrollBar ||  control == _hScrollBar) continue;
                // Get the control's rightmost and bottommost edges.
                var rightEdge = control.Right;
                var bottomEdge = control.Bottom;

                // Update the maximum width and height based on control positions.
                maxWidth = Math.Max(maxWidth, rightEdge);
                maxHeight = Math.Max(maxHeight, bottomEdge);
            }

            // Return the size based on the calculated maximum values.
            return new Size(maxWidth, maxHeight);
        }

        #endregion

        #region Overrides

        protected override void WndProc(ref Message m)
        {
            const int wmMousewheel = 0x020A;
            if (m.Msg == wmMousewheel)
            {
                // Use long to safely accommodate all possible values of WParam.
                var wParamLong = m.WParam.ToInt64();
                // Extract wheelDelta safely, considering the sign and avoiding overflow.
                var wheelDelta = (short)((wParamLong >> 16) & 0xffff);

                // Determine the direction of scroll
                var scrollUp = wheelDelta > 0;

                // Vertical scroll
                if (_vScrollBar.Visible)
                {
                    _vScrollBar.Value = scrollUp ? 
                        Math.Max(_vScrollBar.Minimum, _vScrollBar.Value - _vScrollBar.SmallChange) :
                        Math.Min(_vScrollBar.Maximum - _vScrollBar.LargeChange + 1, _vScrollBar.Value + _vScrollBar.SmallChange);
                }

                // Horizontal scroll (optional: could implement a shift key condition to scroll horizontally)
                else if (_hScrollBar.Visible)
                {
                    _hScrollBar.Value = scrollUp ? 
                        Math.Max(_hScrollBar.Minimum, _hScrollBar.Value - _hScrollBar.SmallChange) :
                        Math.Min(_hScrollBar.Maximum - _hScrollBar.LargeChange + 1, _hScrollBar.Value + _hScrollBar.SmallChange);
                }

                return; // Skip default processing to prevent scrolling the parent container
            }
            base.WndProc(ref m);
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            Font = SkinManager.GetFontByType(MaterialSkinManager.FontType.Body1);
        }

        protected override void InitLayout()
        {
            LocationChanged += (sender, e) => { Parent?.Invalidate(); };
            ForeColor = SkinManager.TextHighEmphasisColor;
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            AdjustScrollBars();
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (Parent != null) AddShadowPaintEvent(Parent, DrawShadowOnParent);
            if (_oldParent != null) RemoveShadowPaintEvent(_oldParent, DrawShadowOnParent);
            _oldParent = Parent;
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Parent == null) return;
            if (Visible)
            {
                AddShadowPaintEvent(Parent, DrawShadowOnParent);
            }
            else
            {
                RemoveShadowPaintEvent(Parent, DrawShadowOnParent);
            }
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            BackColor = SkinManager.BackgroundColor;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            
            // Set scrollBars size and location
            _vScrollBar.Height = Height;
            _vScrollBar.Location = new Point(Width - _vScrollBar.Width, 0);

            _hScrollBar.Width = Width;
            _hScrollBar.Location = new Point(0, Height - _hScrollBar.Height);
            AdjustScrollBars();
            if (Parent == null) return;
            RemoveShadowPaintEvent(Parent, DrawShadowOnParent);
            AddShadowPaintEvent(Parent, DrawShadowOnParent);
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            if (e.Control is ICustomSkinManager customSkinManagerControl)
            {
                customSkinManagerControl.CustomSkinManager = SkinManager;
            }
            AdjustScrollBars();
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            base.OnControlRemoved(e);
            AdjustScrollBars();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (DesignMode)
                return;

            Cursor = Cursors.Arrow;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Parent != null) e.Graphics.Clear(Parent.BackColor);
            DrawHelper.DrawSquareShadow(e.Graphics, ClientRectangle);
        }

        #endregion

        #region Subclasses

        public class MaterialColors
        {

        }

        #endregion
    }
}
