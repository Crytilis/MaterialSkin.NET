using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MaterialSkin.NET.Animations;

namespace MaterialSkin.NET.Controls
{
    public partial class DropDownControl : UserControl
    {
        #region Enums
        public enum eDockSide
        {
            Left,
            Right
        }

        public enum eDropState
        {
            Closed,
            Closing,
            Dropping,
            Dropped
        }

        #endregion

        #region Constants
        private const int TEXT_SMALL_SIZE = 18;
        private const int TEXT_SMALL_Y = 4;
        private const int BOTTOM_PADDING = 3;
        private const int RIGHT_PADDING = 7;
        #endregion

        #region Private Variables
        private int LINE_Y;

        private DropDownContainer dropContainer;
        private Control _dropDownItem;
        private bool closedWhileInControl;
        private Size storedSize;
        private eDropState _dropState;
        private Size anchorSize = new Size(121, 21);
        private eDockSide dockSide;

        private Rectangle anchorClientBounds;
        protected bool mousePressed;
        private bool designView = true;
        private string text;
        private readonly AnimationManager _animationManager;

        private bool showIcon = false;
        private string iconToShow = "";
        private Padding iconPadding;
        private Font iconFont;
        private string hint;
        private bool hasHint = false;
        private Padding hintPadding;
        private Font hintFont;
        private bool useSmallHint = false;

        protected bool drawHintBackground = false;

        #endregion

        #region Properties
        protected eDropState DropState => _dropState;

        [Browsable(false)]
        public int Depth { get; set; }
        [Browsable(false)]
        public MaterialSkinManager SkinManager => MaterialSkinManager.Instance;
        [Browsable(false)]
        public MouseState MouseState { get; set; }
        [Browsable(false)]
        public bool DroppedDown { get; set; }

        [Category("Appearance")]
        public override Font Font
        {
            get { return base.Font; }
            set
            {
                var font = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), value.SizeInPoints, value.Style, GraphicsUnit.Point);
                base.Font = font;
                Invalidate();
            }
        }


        [Category("Material Skin")]
        public bool UseAccent { get; set; }

        [Category("Material Skin"), DefaultValue(""), Localizable(true)]
        public string Hint
        {
            get { return hint; }
            set
            {
                hint = value;
                hasHint = !String.IsNullOrEmpty(hint);
                Invalidate();
            }
        }

        [Category("Material Skin"), Localizable(true)]
        public Padding HintPadding
        {
            get => hintPadding;
            set {
                hintPadding = value;
                Invalidate();
            }
        }
        
        [Category("Material Skin"), DefaultValue(typeof(Font), "Roboto, 7pt")]
        public Font HintFont
        {
            get => hintFont;
            set
            {
                hintFont = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), value.SizeInPoints, value.Style, GraphicsUnit.Point);
                Invalidate();
            }
        }
        [Category("Material Skin"), DefaultValue(false)]
        public bool UseSmallHint
        {
            get { return useSmallHint; }
            set
            {
                useSmallHint = value;
                Invalidate();
            }
        }
        
        [Category("Material Skin")]
        public bool ShowIcon
        {
            get => showIcon;
            set
            {
                showIcon = value;
                Invalidate();
            }
        }
        [Category("Material Skin"), DefaultValue(""), Localizable(true)]
        public string IconToShow
        {
            get => iconToShow;
            set
            {
                iconToShow = value;
                Invalidate();
            }
        }
        [Category("Material Skin"), DefaultValue(typeof(Padding), "0;-2;35;0")]
        public Padding IconPadding
        {
            get => iconPadding;
            set {
                iconPadding = value;
                Invalidate();
            }
        }
        [Category("Material Skin"), DefaultValue(typeof(Font), "Roboto, 10pt")]
        public Font IconFont
        {
            get => iconFont;
            set
            {
                iconFont = new Font(SkinManager.GetFont(MaterialSkinManager.CustomFontFamily.Material_Icons).FontFamily, value.SizeInPoints, value.Style, GraphicsUnit.Point);
                Invalidate();
            }
        }

        public override string Text
        {
            get => text;
            set
            {
                text = value;
                this.Invalidate();
            }
        }
        public Size AnchorSize
        {
            get => anchorSize;
            set
            {
                anchorSize = value;
                this.Invalidate();
            }
        }

        public eDockSide DockSide
        {
            get => dockSide;
            set => dockSide = value;
        }
        public Rectangle AnchorClientBounds => anchorClientBounds;
        protected virtual bool CanDrop
        {
            get
            {
                if (dropContainer != null)
                    return false;

                if (dropContainer == null && closedWhileInControl)
                {
                    closedWhileInControl = false;
                    return false;
                }

                return !closedWhileInControl;
            }
        }
        [DefaultValue(false)]
        protected bool DesignView
        {
            get => designView;
            set
            {
                if (designView == value) return;

                designView = value;
                if (designView)
                {
                    this.Size = storedSize;
                }
                else
                {
                    storedSize = this.Size;
                    this.Size = anchorSize;
                }

            }
        }
        #endregion

        #region Events
        public event EventHandler PropertyChanged;
        
        public event EventHandler DropDown;


        protected void OnPropertyChanged()
        {
            if (PropertyChanged != null)
                PropertyChanged(null, null);
        }
        #endregion

        #region Constructor
        public DropDownControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            InitializeComponent();

            Font = SkinManager.getFontByType(MaterialSkinManager.fontType.Subtitle1);

            this.storedSize = this.Size;
            this.BackColor = Color.White;
            this.Text = this.Name;
            MouseState = MouseState.OUT;

            #region Animation
            _animationManager = new AnimationManager(true)
            {
                Increment = 0.08,
                AnimationType = AnimationType.EaseInOut
            };
            _animationManager.OnAnimationProgress += sender => Invalidate();
            _animationManager.OnAnimationFinished += sender => _animationManager.SetProgress(0);
            #endregion

            #region Events
            GotFocus += (sender, args) =>
            {
                _animationManager.StartNewAnimation(AnimationDirection.In);
                Invalidate();
            };
            LostFocus += (sender, args) =>
            {
                MouseState = MouseState.OUT;
                if (!DroppedDown) _animationManager.StartNewAnimation(AnimationDirection.Out);
            };

            MouseEnter += (sender, args) =>
            {
                MouseState = MouseState.HOVER;
                Invalidate();
            };
            MouseLeave += (sender, args) =>
            {
                MouseState = MouseState.OUT;
                Invalidate();
            };
            #endregion
        }

        public void InitializeDropDown(Control dropDownItem)
        {
            if (_dropDownItem != null)
                throw new Exception("The drop down item has already been implemented!");
            designView = false;
            _dropState = eDropState.Closed;
            this.Size = anchorSize;
            this.anchorClientBounds = new Rectangle(2, 2, anchorSize.Width - 21, anchorSize.Height - 4);
            //removes the dropDown item from the controls list so it 
            //won't be seen until the drop-down window is active
            if (this.Controls.Contains(dropDownItem))
                this.Controls.Remove(dropDownItem);
            _dropDownItem = dropDownItem;

            HintFont = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), 7, FontStyle.Regular, GraphicsUnit.Point);
            HintPadding = new Padding(10,-2,0,0);
            IconFont = new Font(SkinManager.GetFont(MaterialSkinManager.CustomFontFamily.Material_Icons).FontFamily, 8, FontStyle.Regular, GraphicsUnit.Point);
            IconToShow = "";
            ShowIcon = false;
        }
        #endregion

        #region Overrides
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (designView)
                storedSize = this.Size;
            anchorSize.Width = this.Width;
            if (!designView)
            {
                anchorSize.Height = this.Height;
                this.anchorClientBounds = new Rectangle(2, 2, anchorSize.Width - 21, anchorSize.Height - 4);
            }

            setHeightVars();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            MouseState = MouseState.DOWN;
            mousePressed = true;
            _animationManager.StartNewAnimation(AnimationDirection.In);
            Invalidate();
            OpenDropDown();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            MouseState = MouseState.HOVER;
            mousePressed = false;
            Invalidate();
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            if (DesignMode) return;

            setHeightVars();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(SkinManager.BackdropColor);
            var brush = Enabled ? Focused ?
                SkinManager.BackgroundFocusBrush : // Focused
                MouseState == MouseState.HOVER ?
                SkinManager.BackgroundHoverBrush : // Hover
                SkinManager.BackgroundAlternativeBrush : // normal
                SkinManager.BackgroundDisabledBrush; // Disabled

            g.FillRectangle(Enabled ? Focused ?
                SkinManager.BackgroundFocusBrush : // Focused
                MouseState == MouseState.HOVER ?
                SkinManager.BackgroundHoverBrush : // Hover
                SkinManager.BackgroundAlternativeBrush : // normal
                SkinManager.BackgroundDisabledBrush // Disabled
                , ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, LINE_Y);

            PaintSomething(e);

            //Set color and brush
            Color SelectedColor = new Color();
            if (UseAccent)
                SelectedColor = SkinManager.ColorScheme.AccentColor;
            else
                SelectedColor = SkinManager.ColorScheme.PrimaryColor;
            SolidBrush SelectedBrush = new SolidBrush(SelectedColor);

            #region Arrow
            // Create and Draw the arrow
            GraphicsPath pth = new GraphicsPath();
            PointF TopRight = new PointF(this.Width - 0.5f - RIGHT_PADDING, (this.Height >> 1) - 2.5f);
            PointF MidBottom = new PointF(this.Width - 4.5f - RIGHT_PADDING, (this.Height >> 1) + 2.5f);
            PointF TopLeft = new PointF(this.Width - 8.5f - RIGHT_PADDING, (this.Height >> 1) - 2.5f);
            pth.AddLine(TopLeft, TopRight);
            pth.AddLine(TopRight, MidBottom);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.FillPath((SolidBrush)(Enabled ? DroppedDown || Focused ?
                SelectedBrush : //DroppedDown or Focused
                SkinManager.TextHighEmphasisBrush : //Not DroppedDown and not Focused
                new SolidBrush(DrawHelper.BlendColor(SkinManager.TextHighEmphasisColor, SkinManager.SwitchOffDisabledThumbColor, 197))  //Disabled
                ), pth);
            g.SmoothingMode = SmoothingMode.None;
            #endregion

            #region Icon
            if (ShowIcon && IconToShow != null && !string.IsNullOrEmpty(IconToShow))
            {
                using (NativeTextRenderer NativeText = new NativeTextRenderer(g))
                {
                    var iconSIze = NativeText.MeasureString(iconToShow, IconFont);

                    Rectangle suffixRect = new Rectangle(
                        Width - IconPadding.Right,
                        ClientRectangle.Top + IconPadding.Top,
                        iconSIze.Width,
                        ClientRectangle.Height);

                    // Draw Icon
                    NativeText.DrawTransparentText(
                    IconToShow,
                    IconFont,
                    Enabled ? SkinManager.TextMediumEmphasisColor : SkinManager.TextDisabledOrHintColor,
                    suffixRect.Location,
                    suffixRect.Size,
                    NativeTextRenderer.TextAlignFlags.Right | NativeTextRenderer.TextAlignFlags.Middle);
                }
            }
            #endregion
            #region Hint
            // HintText
            Size hintFontSize = new Size(12,12);
            
            if(!string.IsNullOrEmpty(Hint))
            using (NativeTextRenderer NativeText = new NativeTextRenderer(CreateGraphics()))
            {
                hintFontSize = NativeText.MeasureString(Hint, HintFont);
            }
            
            bool userTextPresent = !string.IsNullOrEmpty(Text);
            bool isFocused = this.Focused;
            Rectangle hintRect = new Rectangle(HintPadding.Left, HintPadding.Top, Width - HintPadding.Left - HintPadding.Right, hintFontSize.Height);

            // Draw hint text
            if(hasHint && (isFocused || userTextPresent || UseSmallHint))
            {
                if(drawHintBackground)
                {
                    Size hintBackgroundSize = new Size(hintRect.Location.X + (hintFontSize.Width + 4) , hintRect.Location.Y + (hintFontSize.Height * 1));
                    g.FillRectangle(SkinManager.BackdropBrush, 0, 0, hintBackgroundSize.Width, hintBackgroundSize.Height);
                    g.FillRectangle(Enabled ? Focused ?
                        SkinManager.BackgroundFocusBrush :  // Focused
                        MouseState == MouseState.HOVER ?
                        SkinManager.BackgroundHoverBrush :  // Hover
                        SkinManager.BackgroundAlternativeBrush :       // normal
                        SkinManager.BackgroundDisabledBrush // Disabled
                        , 0, 0, hintBackgroundSize.Width, hintBackgroundSize.Height);
                }
                using (NativeTextRenderer NativeText = new NativeTextRenderer(g))
                {
                    NativeText.DrawTransparentText(
                    Hint,
                    HintFont,
                    Enabled ? !userTextPresent && !isFocused ? isFocused ? UseAccent ?
                    SkinManager.ColorScheme.AccentColor :   // Focus Accent
                    SkinManager.ColorScheme.PrimaryColor :  // Focus Primary
                    SkinManager.TextMediumEmphasisColor :   // not focused
                    SkinManager.TextMediumEmphasisColor :   // error state (there is no error state)
                    SkinManager.TextDisabledOrHintColor,    // Disabled
                    hintRect.Location,
                    hintRect.Size,
                    NativeTextRenderer.TextAlignFlags.Left | NativeTextRenderer.TextAlignFlags.Middle);
                }
            }
            #endregion

            // bottom line base
            g.FillRectangle(SkinManager.DividersAlternativeBrush, 0, LINE_Y, Width, 1);

            if (!_animationManager.IsAnimating())
            {
                // bottom line
                if (DroppedDown || Focused)
                {
                    g.FillRectangle(SelectedBrush, 0, LINE_Y, Width, 2);
                }
            }
            else
            {
                if (DropState == eDropState.Dropped)
                {
                    g.FillRectangle(SelectedBrush, 0, LINE_Y, Width, 2);
                }
                else
                {
                    // Animate - Focus got/lost
                    double animationProgress = _animationManager.GetProgress();
                    // Line Animation
                    int LineAnimationWidth = (int)(Width * animationProgress);
                    int LineAnimationX = (Width / 2) - (LineAnimationWidth / 2);
                    g.FillRectangle(SelectedBrush, LineAnimationX, LINE_Y, LineAnimationWidth, 2);
                }
            }

            // Calc text Rect
            var textRect = new System.Drawing.Rectangle(
                SkinManager.FORM_PADDING,
                ClientRectangle.Y,
                ClientRectangle.Width - 32, ClientRectangle.Height);
            g.Clip = new Region(textRect);

            using (NativeTextRenderer NativeText = new NativeTextRenderer(g))
            {
                var font = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), Font.SizeInPoints, Font.Style, GraphicsUnit.Point);
                // Draw user text
                NativeText.DrawTransparentText(
                    Text,
                    font,
                    Enabled ? SkinManager.TextHighEmphasisColor : SkinManager.TextDisabledOrHintColor,
                    textRect.Location,
                    textRect.Size,
                    NativeTextRenderer.TextAlignFlags.Left | NativeTextRenderer.TextAlignFlags.Middle);
            }

            g.ResetClip();
        }
        #endregion

        #region Private Methods
        protected virtual void PaintSomething(PaintEventArgs e)
        {
        }
        private void setHeightVars()
        {
            LINE_Y = Height - BOTTOM_PADDING;
        }

        private void ParentForm_Move(object sender, EventArgs e)
        {
            dropContainer.DropDownForm.Bounds = GetDropDownBounds();
        }

        private void dropContainer_DropStateChange(DropDownControl.eDropState state)
        {
            _dropState = state;
        }
        private void dropContainer_Closed(object sender, FormClosedEventArgs e)
        {
            if (!dropContainer.DropDownForm.IsDisposed)
            {
                dropContainer.DropDownForm.DropStateChange -= dropContainer_DropStateChange;
                dropContainer.DropDownForm.FormClosed -= dropContainer_Closed;
                this.ParentForm.Move -= ParentForm_Move;
                dropContainer.DropDownForm.Dispose();
            }
            dropContainer = null;
            closedWhileInControl = (this.RectangleToScreen(this.ClientRectangle).Contains(Cursor.Position));
            _dropState = eDropState.Closed;
            this.Invalidate();
        }
        private System.Windows.Forms.VisualStyles.ComboBoxState getState()
        {
            if (mousePressed || dropContainer != null)
                return System.Windows.Forms.VisualStyles.ComboBoxState.Pressed;
            else
                return System.Windows.Forms.VisualStyles.ComboBoxState.Normal;
        }
        #endregion

        #region Public Methods
        protected void OpenDropDown()
        {
            if (_dropDownItem == null)
                throw new NotImplementedException("The drop down item has not been initialized!  Use the InitializeDropDown() method to do so.");

            if (!CanDrop) return;

            dropContainer = new DropDownContainer(_dropDownItem);
            dropContainer.DropDownForm.Bounds = GetDropDownBounds();
            dropContainer.DropDownForm.DropStateChange += new DropDownForm.DropWindowArgs(dropContainer_DropStateChange);
            dropContainer.DropDownForm.FormClosed += new FormClosedEventHandler(dropContainer_Closed);
            this.ParentForm.Move += new EventHandler(ParentForm_Move);
            _dropState = eDropState.Dropping;
            dropContainer.Show();
            _dropState = eDropState.Dropped;
            Invalidate();
            DropDown?.Invoke(this, EventArgs.Empty);
        }

        protected virtual Rectangle GetDropDownBounds()
        {
            Size inflatedDropSize = new Size(_dropDownItem.Width, _dropDownItem.Height + 2);
            var screenBounds = dockSide == eDockSide.Left ?
                new Rectangle(this.Parent.PointToScreen(new Point(this.Bounds.X, this.Bounds.Bottom)), inflatedDropSize)
                : new Rectangle(this.Parent.PointToScreen(new Point(this.Bounds.Right - _dropDownItem.Width, this.Bounds.Bottom)), inflatedDropSize);
            Rectangle workingArea = Screen.GetWorkingArea(screenBounds);
            //make sure we're completely in the top-left working area
            if (screenBounds.X < workingArea.X) screenBounds.X = workingArea.X;
            if (screenBounds.Y < workingArea.Y) screenBounds.Y = workingArea.Y;

            //make sure we're not extended past the working area's right /bottom edge
            if (screenBounds.Right > workingArea.Right && workingArea.Width > screenBounds.Width)
                screenBounds.X = workingArea.Right - screenBounds.Width;
            if (screenBounds.Bottom > workingArea.Bottom && workingArea.Height > screenBounds.Height)
                screenBounds.Y = workingArea.Bottom - screenBounds.Height;

            return screenBounds;
        }

        public void CloseDropDown()
        {

            if (dropContainer != null)
            {
                _dropState = eDropState.Closing;
                dropContainer.DropDownForm.Freeze = false;
                dropContainer.DropDownForm.Close();
            }
        }

        public void FreezeDropDown(bool remainVisible)
        {
            if (dropContainer != null)
            {
                dropContainer.DropDownForm.Freeze = true;
                if (!remainVisible)
                    dropContainer.DropDownForm.Visible = false;
            }
        }

        public void UnFreezeDropDown()
        {
            if (dropContainer != null)
            {
                dropContainer.DropDownForm.Freeze = false;
                if (!dropContainer.DropDownForm.Visible)
                    dropContainer.DropDownForm.Visible = true;
            }
        }

        #endregion

        #region SubClasses

        internal sealed class DropDownContainer
        {
            private Timer _timer;
            private Rectangle originalRect;
            private readonly AnimationManager _animationManager;
            public DropDownForm DropDownForm;


            public DropDownContainer(Control dropDownItem)
            {
                DropDownForm = new DropDownForm(dropDownItem);
                originalRect = dropDownItem.ClientRectangle;
                DropDownForm.Height = 0;

                #region Animation
                _animationManager = new AnimationManager(true)
                {
                    Increment = 0.2,
                    AnimationType = AnimationType.EaseInOut
                };
                _animationManager.OnAnimationProgress += sender =>
                {
                    if (_animationManager.IsAnimating())
                    {
                        // Animate - Focus got/lost
                        double animationProgress = _animationManager.GetProgress();

                        DropDownForm.Height += (int)(originalRect.Height * animationProgress);
                        if (DropDownForm.Height > originalRect.Height)
                        {
                            DropDownForm.Height = originalRect.Height;
                            _animationManager.SetProgress(0);
                        }
                        DropDownForm.Invalidate();
                    }
                };
                _animationManager.OnAnimationFinished += sender => {
                    _animationManager.SetProgress(0);
                    DropDownForm.Height = originalRect.Height;
                };
                #endregion
            }

            public void Show()
            {
                _animationManager.StartNewAnimation(AnimationDirection.In);
                _timer = new Timer();
                Size formSize = DropDownForm.Size;

                DropDownForm.Size = new Size(formSize.Width, 0);
                DropDownForm.Visible = true;

            }
        }

        internal sealed class DropDownForm : Form, IMessageFilter
        {
            public bool Freeze;
            [Browsable(false)]
            public MaterialSkinManager SkinManager => MaterialSkinManager.Instance;

            public DropDownForm(Control dropDownItem)
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

                this.FormBorderStyle = FormBorderStyle.None;
                dropDownItem.Location = new Point(0, 0);
                this.Controls.Add(dropDownItem);
                this.StartPosition = FormStartPosition.Manual;
                this.ShowInTaskbar = false;
                this.Size = dropDownItem.Size;
                Application.AddMessageFilter(this);
            }

            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);
                if (Controls.Count > 0)
                {
                    if (Width > Controls[0].Width) Width = Controls[0].Width;
                    if (Height > Controls[0].Width) Height = Controls[0].Height;
                }
            }

            public bool PreFilterMessage(ref Message m)
            {
                if (!Freeze && this.Visible && (Form.ActiveForm == null || !Form.ActiveForm.Equals(this)))
                {
                    OnDropStateChange(eDropState.Closing);
                    this.Close();
                }


                return false;
            }

            public delegate void DropWindowArgs(eDropState state);
            public event DropWindowArgs DropStateChange;
            protected void OnDropStateChange(eDropState state)
            {
                if (DropStateChange != null)
                    DropStateChange(state);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                e.Graphics.FillRectangle(new SolidBrush(SkinManager.BackdropColor), ClientRectangle);
            }

            protected override void OnClosing(CancelEventArgs e)
            {
                Application.RemoveMessageFilter(this);
                this.Controls.RemoveAt(0); //prevent the control from being disposed
                base.OnClosing(e);
            }
        }
        #endregion
    }
}
