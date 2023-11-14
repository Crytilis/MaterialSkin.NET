namespace MaterialSkin.Controls
{
    using MaterialSkin.Animations;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    public class MaterialNumericUpDown : NumericUpDown, IMaterialControl
    {
        #region Private variables
        private const int TEXT_SMALL_SIZE = 18;
        private const int TEXT_SMALL_Y = 4;
        private const int BOTTOM_PADDING = 3;
        private const int LEFT_PADDING = 5;
        private const int RIGHT_PADDING = 20;
        private const int ACTIVATION_INDICATOR_HEIGHT = 1;
        private const int FONT_HEIGHT = 20;
        private const int HINT_TEXT_SMALL_SIZE = 18;
        private const int HINT_TEXT_SMALL_Y = 4;
        private const int HELPER_TEXT_HEIGHT = 16;

        private int HEIGHT = 32;
        private int LINE_Y;
        private int left_padding;
        private int right_padding;

        public bool isFocused = false;
        private bool hasHint;
        private bool leaveOnEnterKey;
        private bool _readonly;
        private string hint = string.Empty;

        private readonly AnimationManager animationManager;

        MaterialContextMenuStrip cms = new BaseTextBoxContextMenuStrip();
        ContextMenuStrip lastContextMenuStrip = new ContextMenuStrip();

        protected BaseTextBox baseTextBox = new BaseTextBox();
        private TextBox textbox;
        private Control upDownButtons;
        #endregion

        #region MaterialSkin
        [Browsable(false)]
        public int Depth { get; set; }

        [Browsable(false)]
        public MaterialSkinManager SkinManager => MaterialSkinManager.Instance;

        [Browsable(false)]
        public MouseState MouseState { get; set; }

        [Category("Material Skin"), DefaultValue(true)]
        public bool UseAccent { get; set; }

        [Category("Material Skin"), DefaultValue(false), Description("Select next control which have TabStop property set to True when enter key is pressed.")]
        public bool LeaveOnEnterKey
        {
            get => leaveOnEnterKey;
            set
            {
                leaveOnEnterKey = value;
                if (value)
                {
                    baseTextBox.KeyDown += new KeyEventHandler(LeaveOnEnterKey_KeyDown);
                }
                else
                {
                    baseTextBox.KeyDown -= LeaveOnEnterKey_KeyDown;
                }
                Invalidate();
            }
        }
        #endregion

        #region TextBox properties
        //Unused properties
        [Browsable(false)]
        public override System.Drawing.Image BackgroundImage { get; set; }

        [Browsable(false)]
        public override System.Windows.Forms.ImageLayout BackgroundImageLayout { get; set; }

        [Browsable(false)]
        public string SelectedText { get { return baseTextBox.SelectedText; } set { baseTextBox.SelectedText = value; } }

        [Browsable(false)]
        public int SelectionStart { get { return baseTextBox.SelectionStart; } set { baseTextBox.SelectionStart = value; } }

        [Browsable(false)]
        public int SelectionLength { get { return baseTextBox.SelectionLength; } set { baseTextBox.SelectionLength = value; } }

        [Browsable(false)]
        public int TextLength { get { return baseTextBox.TextLength; } }

        [Browsable(false)]
        public override System.Drawing.Color ForeColor { get; set; }

        public override bool Focused => baseTextBox.Focused;

        public override ContextMenuStrip ContextMenuStrip
        {
            get { return baseTextBox.ContextMenuStrip; }
            set
            {
                if (value != null)
                {
                    //ContextMenuStrip = value;
                    baseTextBox.ContextMenuStrip = value;
                    base.ContextMenuStrip = value;
                }
                else
                {
                    //ContextMenuStrip = cms;
                    baseTextBox.ContextMenuStrip = cms;
                    base.ContextMenuStrip = cms;
                }
                lastContextMenuStrip = base.ContextMenuStrip;
            }
        }

        [Browsable(false)]
        public override Color BackColor { get { return Parent == null ? SkinManager.BackgroundColor : Parent.BackColor; } }

        public override string Text { 
            get { return baseTextBox.Text; }
            set { 
                baseTextBox.Text = value;
                UpdateRects();
            }
        }

        [Category("Appearance")]
        public new HorizontalAlignment TextAlign { get { return baseTextBox.TextAlign; } set { baseTextBox.TextAlign = value; } }

        [Category("Appearance"), Localizable(true)]
        public override Font Font
        {
            get { return base.Font; }
            set
            {
                var font = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), value.SizeInPoints, value.Style, GraphicsUnit.Point);
                if (baseTextBox != null)
                {
                    baseTextBox.Font = font;
                }
                base.Font = font;
                Invalidate();
            }
        }

        [Category("Behavior")]
        public bool ShortcutsEnabled
        {
            get
            { return baseTextBox.ShortcutsEnabled; }
            set
            {
                baseTextBox.ShortcutsEnabled = value;
                if (value == false)
                {
                    baseTextBox.ContextMenuStrip = null;
                    base.ContextMenuStrip = null;
                }
                else
                {
                    baseTextBox.ContextMenuStrip = lastContextMenuStrip;
                    base.ContextMenuStrip = lastContextMenuStrip;
                }
            }
        }

        public new object Tag { get { return baseTextBox.Tag; } set { baseTextBox.Tag = value; } }

        [Category("Behavior")]
        public new bool ReadOnly
        {
            get { return _readonly; }
            set
            {
                _readonly = value;
                if (Enabled == true)
                {
                    baseTextBox.ReadOnly = _readonly;
                }
                this.Invalidate();
            }
        }

        private bool animateReadOnly;

        [Category("Material Skin")]
        [Browsable(true)]
        public bool AnimateReadOnly
        {
            get => animateReadOnly;
            set
            {
                animateReadOnly = value;
                Invalidate();
            }
        }

        public new void Select(int start, int length) => baseTextBox.Select(start, length);

        public void SelectAll() => baseTextBox.SelectAll();

        public void Clear() => baseTextBox.Clear();

        public void Copy() => baseTextBox.Copy();

        public void Cut() => baseTextBox.Cut();

        public void Undo() => baseTextBox.Undo();

        public void Paste() => baseTextBox.Paste();
        #endregion
//        # region Forwarding events to baseTextBox

//        public event EventHandler AcceptsTabChanged
//        {
//            add
//            {
//                baseTextBox.AcceptsTabChanged += value;
//            }
//            remove
//            {
//                baseTextBox.AcceptsTabChanged -= value;
//            }
//        }

//        public new event EventHandler AutoSizeChanged
//        {
//            add
//            {
//                baseTextBox.AutoSizeChanged += value;
//            }
//            remove
//            {
//                baseTextBox.AutoSizeChanged -= value;
//            }
//        }

//        public new event EventHandler BackgroundImageChanged
//        {
//            add
//            {
//                baseTextBox.BackgroundImageChanged += value;
//            }
//            remove
//            {
//                baseTextBox.BackgroundImageChanged -= value;
//            }
//        }

//        public new event EventHandler BackgroundImageLayoutChanged
//        {
//            add
//            {
//                baseTextBox.BackgroundImageLayoutChanged += value;
//            }
//            remove
//            {
//                baseTextBox.BackgroundImageLayoutChanged -= value;
//            }
//        }

//        public new event EventHandler BindingContextChanged
//        {
//            add
//            {
//                baseTextBox.BindingContextChanged += value;
//            }
//            remove
//            {
//                baseTextBox.BindingContextChanged -= value;
//            }
//        }

//        public event EventHandler BorderStyleChanged
//        {
//            add
//            {
//                baseTextBox.BorderStyleChanged += value;
//            }
//            remove
//            {
//                baseTextBox.BorderStyleChanged -= value;
//            }
//        }

//        public new event EventHandler CausesValidationChanged
//        {
//            add
//            {
//                baseTextBox.CausesValidationChanged += value;
//            }
//            remove
//            {
//                baseTextBox.CausesValidationChanged -= value;
//            }
//        }

//        public new event UICuesEventHandler ChangeUICues
//        {
//            add
//            {
//                baseTextBox.ChangeUICues += value;
//            }
//            remove
//            {
//                baseTextBox.ChangeUICues -= value;
//            }
//        }

//        public new event EventHandler Click
//        {
//            add
//            {
//                baseTextBox.Click += value;
//            }
//            remove
//            {
//                baseTextBox.Click -= value;
//            }
//        }

//        public new event EventHandler ClientSizeChanged
//        {
//            add
//            {
//                baseTextBox.ClientSizeChanged += value;
//            }
//            remove
//            {
//                baseTextBox.ClientSizeChanged -= value;
//            }
//        }

//#if NETFRAMEWORK
//        public new event EventHandler ContextMenuChanged
//        {
//            add
//            {
//                baseTextBox.ContextMenuChanged += value;
//            }
//            remove
//            {
//                baseTextBox.ContextMenuChanged -= value;
//            }
//        }
//#endif

//        public new event EventHandler ContextMenuStripChanged
//        {
//            add
//            {
//                baseTextBox.ContextMenuStripChanged += value;
//            }
//            remove
//            {
//                baseTextBox.ContextMenuStripChanged -= value;
//            }
//        }

//        public new event ControlEventHandler ControlAdded
//        {
//            add
//            {
//                baseTextBox.ControlAdded += value;
//            }
//            remove
//            {
//                baseTextBox.ControlAdded -= value;
//            }
//        }

//        public new event ControlEventHandler ControlRemoved
//        {
//            add
//            {
//                baseTextBox.ControlRemoved += value;
//            }
//            remove
//            {
//                baseTextBox.ControlRemoved -= value;
//            }
//        }

//        public new event EventHandler CursorChanged
//        {
//            add
//            {
//                baseTextBox.CursorChanged += value;
//            }
//            remove
//            {
//                baseTextBox.CursorChanged -= value;
//            }
//        }

//        public new event EventHandler Disposed
//        {
//            add
//            {
//                baseTextBox.Disposed += value;
//            }
//            remove
//            {
//                baseTextBox.Disposed -= value;
//            }
//        }

//        public new event EventHandler DockChanged
//        {
//            add
//            {
//                baseTextBox.DockChanged += value;
//            }
//            remove
//            {
//                baseTextBox.DockChanged -= value;
//            }
//        }

//        public new event EventHandler DoubleClick
//        {
//            add
//            {
//                baseTextBox.DoubleClick += value;
//            }
//            remove
//            {
//                baseTextBox.DoubleClick -= value;
//            }
//        }

//        public new event DragEventHandler DragDrop
//        {
//            add
//            {
//                baseTextBox.DragDrop += value;
//            }
//            remove
//            {
//                baseTextBox.DragDrop -= value;
//            }
//        }

//        public new event DragEventHandler DragEnter
//        {
//            add
//            {
//                baseTextBox.DragEnter += value;
//            }
//            remove
//            {
//                baseTextBox.DragEnter -= value;
//            }
//        }

//        public new event EventHandler DragLeave
//        {
//            add
//            {
//                baseTextBox.DragLeave += value;
//            }
//            remove
//            {
//                baseTextBox.DragLeave -= value;
//            }
//        }

//        public new event DragEventHandler DragOver
//        {
//            add
//            {
//                baseTextBox.DragOver += value;
//            }
//            remove
//            {
//                baseTextBox.DragOver -= value;
//            }
//        }

//        public new event EventHandler EnabledChanged
//        {
//            add
//            {
//                baseTextBox.EnabledChanged += value;
//            }
//            remove
//            {
//                baseTextBox.EnabledChanged -= value;
//            }
//        }

//        public new event EventHandler Enter
//        {
//            add
//            {
//                baseTextBox.Enter += value;
//            }
//            remove
//            {
//                baseTextBox.Enter -= value;
//            }
//        }

//        public new event EventHandler FontChanged
//        {
//            add
//            {
//                baseTextBox.FontChanged += value;
//            }
//            remove
//            {
//                baseTextBox.FontChanged -= value;
//            }
//        }

//        public new event EventHandler ForeColorChanged
//        {
//            add
//            {
//                baseTextBox.ForeColorChanged += value;
//            }
//            remove
//            {
//                baseTextBox.ForeColorChanged -= value;
//            }
//        }

//        public new event GiveFeedbackEventHandler GiveFeedback
//        {
//            add
//            {
//                baseTextBox.GiveFeedback += value;
//            }
//            remove
//            {
//                baseTextBox.GiveFeedback -= value;
//            }
//        }

//        public new event EventHandler GotFocus
//        {
//            add
//            {
//                baseTextBox.GotFocus += value;
//            }
//            remove
//            {
//                baseTextBox.GotFocus -= value;
//            }
//        }

//        public new event EventHandler HandleCreated
//        {
//            add
//            {
//                baseTextBox.HandleCreated += value;
//            }
//            remove
//            {
//                baseTextBox.HandleCreated -= value;
//            }
//        }

//        public new event EventHandler HandleDestroyed
//        {
//            add
//            {
//                baseTextBox.HandleDestroyed += value;
//            }
//            remove
//            {
//                baseTextBox.HandleDestroyed -= value;
//            }
//        }

//        public new event HelpEventHandler HelpRequested
//        {
//            add
//            {
//                baseTextBox.HelpRequested += value;
//            }
//            remove
//            {
//                baseTextBox.HelpRequested -= value;
//            }
//        }

//        public event EventHandler HideSelectionChanged
//        {
//            add
//            {
//                baseTextBox.HideSelectionChanged += value;
//            }
//            remove
//            {
//                baseTextBox.HideSelectionChanged -= value;
//            }
//        }

//        public new event EventHandler ImeModeChanged
//        {
//            add
//            {
//                baseTextBox.ImeModeChanged += value;
//            }
//            remove
//            {
//                baseTextBox.ImeModeChanged -= value;
//            }
//        }

//        public new event InvalidateEventHandler Invalidated
//        {
//            add
//            {
//                baseTextBox.Invalidated += value;
//            }
//            remove
//            {
//                baseTextBox.Invalidated -= value;
//            }
//        }

//        public new event KeyEventHandler KeyDown
//        {
//            add
//            {
//                baseTextBox.KeyDown += value;
//            }
//            remove
//            {
//                baseTextBox.KeyDown -= value;
//            }
//        }

//        public new event KeyPressEventHandler KeyPress
//        {
//            add
//            {
//                baseTextBox.KeyPress += value;
//            }
//            remove
//            {
//                baseTextBox.KeyPress -= value;
//            }
//        }

//        public new event KeyEventHandler KeyUp
//        {
//            add
//            {
//                baseTextBox.KeyUp += value;
//            }
//            remove
//            {
//                baseTextBox.KeyUp -= value;
//            }
//        }

//        public new event LayoutEventHandler Layout
//        {
//            add
//            {
//                baseTextBox.Layout += value;
//            }
//            remove
//            {
//                baseTextBox.Layout -= value;
//            }
//        }

//        public new event EventHandler Leave
//        {
//            add
//            {
//                baseTextBox.Leave += value;
//            }
//            remove
//            {
//                baseTextBox.Leave -= value;
//            }
//        }

//        public new event EventHandler LocationChanged
//        {
//            add
//            {
//                baseTextBox.LocationChanged += value;
//            }
//            remove
//            {
//                baseTextBox.LocationChanged -= value;
//            }
//        }

//        public new event EventHandler LostFocus
//        {
//            add
//            {
//                baseTextBox.LostFocus += value;
//            }
//            remove
//            {
//                baseTextBox.LostFocus -= value;
//            }
//        }

//        public new event EventHandler MarginChanged
//        {
//            add
//            {
//                baseTextBox.MarginChanged += value;
//            }
//            remove
//            {
//                baseTextBox.MarginChanged -= value;
//            }
//        }

//        public event EventHandler ModifiedChanged
//        {
//            add
//            {
//                baseTextBox.ModifiedChanged += value;
//            }
//            remove
//            {
//                baseTextBox.ModifiedChanged -= value;
//            }
//        }

//        public new event EventHandler MouseCaptureChanged
//        {
//            add
//            {
//                baseTextBox.MouseCaptureChanged += value;
//            }
//            remove
//            {
//                baseTextBox.MouseCaptureChanged -= value;
//            }
//        }

//        public new event MouseEventHandler MouseClick
//        {
//            add
//            {
//                baseTextBox.MouseClick += value;
//            }
//            remove
//            {
//                baseTextBox.MouseClick -= value;
//            }
//        }

//        public new event MouseEventHandler MouseDoubleClick
//        {
//            add
//            {
//                baseTextBox.MouseDoubleClick += value;
//            }
//            remove
//            {
//                baseTextBox.MouseDoubleClick -= value;
//            }
//        }

//        public new event MouseEventHandler MouseDown
//        {
//            add
//            {
//                baseTextBox.MouseDown += value;
//            }
//            remove
//            {
//                baseTextBox.MouseDown -= value;
//            }
//        }

//        public new event EventHandler MouseEnter
//        {
//            add
//            {
//                baseTextBox.MouseEnter += value;
//            }
//            remove
//            {
//                baseTextBox.MouseEnter -= value;
//            }
//        }

//        public new event EventHandler MouseHover
//        {
//            add
//            {
//                baseTextBox.MouseHover += value;
//            }
//            remove
//            {
//                baseTextBox.MouseHover -= value;
//            }
//        }

//        public new event EventHandler MouseLeave
//        {
//            add
//            {
//                baseTextBox.MouseLeave += value;
//            }
//            remove
//            {
//                baseTextBox.MouseLeave -= value;
//            }
//        }

//        public new event MouseEventHandler MouseMove
//        {
//            add
//            {
//                baseTextBox.MouseMove += value;
//            }
//            remove
//            {
//                baseTextBox.MouseMove -= value;
//            }
//        }

//        public new event MouseEventHandler MouseUp
//        {
//            add
//            {
//                baseTextBox.MouseUp += value;
//            }
//            remove
//            {
//                baseTextBox.MouseUp -= value;
//            }
//        }

//        public new event MouseEventHandler MouseWheel
//        {
//            add
//            {
//                baseTextBox.MouseWheel += value;
//            }
//            remove
//            {
//                baseTextBox.MouseWheel -= value;
//            }
//        }

//        public new event EventHandler Move
//        {
//            add
//            {
//                baseTextBox.Move += value;
//            }
//            remove
//            {
//                baseTextBox.Move -= value;
//            }
//        }

//        public event EventHandler MultilineChanged
//        {
//            add
//            {
//                baseTextBox.MultilineChanged += value;
//            }
//            remove
//            {
//                baseTextBox.MultilineChanged -= value;
//            }
//        }

//        public new event EventHandler PaddingChanged
//        {
//            add
//            {
//                baseTextBox.PaddingChanged += value;
//            }
//            remove
//            {
//                baseTextBox.PaddingChanged -= value;
//            }
//        }

//        public new event PaintEventHandler Paint
//        {
//            add
//            {
//                baseTextBox.Paint += value;
//            }
//            remove
//            {
//                baseTextBox.Paint -= value;
//            }
//        }

//        public new event EventHandler ParentChanged
//        {
//            add
//            {
//                baseTextBox.ParentChanged += value;
//            }
//            remove
//            {
//                baseTextBox.ParentChanged -= value;
//            }
//        }

//        public new event PreviewKeyDownEventHandler PreviewKeyDown
//        {
//            add
//            {
//                baseTextBox.PreviewKeyDown += value;
//            }
//            remove
//            {
//                baseTextBox.PreviewKeyDown -= value;
//            }
//        }

//        public new event QueryAccessibilityHelpEventHandler QueryAccessibilityHelp
//        {
//            add
//            {
//                baseTextBox.QueryAccessibilityHelp += value;
//            }
//            remove
//            {
//                baseTextBox.QueryAccessibilityHelp -= value;
//            }
//        }

//        public new event QueryContinueDragEventHandler QueryContinueDrag
//        {
//            add
//            {
//                baseTextBox.QueryContinueDrag += value;
//            }
//            remove
//            {
//                baseTextBox.QueryContinueDrag -= value;
//            }
//        }

//        public event EventHandler ReadOnlyChanged
//        {
//            add
//            {
//                baseTextBox.ReadOnlyChanged += value;
//            }
//            remove
//            {
//                baseTextBox.ReadOnlyChanged -= value;
//            }
//        }

//        public new event EventHandler RegionChanged
//        {
//            add
//            {
//                baseTextBox.RegionChanged += value;
//            }
//            remove
//            {
//                baseTextBox.RegionChanged -= value;
//            }
//        }

//        public new event EventHandler Resize
//        {
//            add
//            {
//                baseTextBox.Resize += value;
//            }
//            remove
//            {
//                baseTextBox.Resize -= value;
//            }
//        }

//        public new event EventHandler RightToLeftChanged
//        {
//            add
//            {
//                baseTextBox.RightToLeftChanged += value;
//            }
//            remove
//            {
//                baseTextBox.RightToLeftChanged -= value;
//            }
//        }

//        public new event EventHandler SizeChanged
//        {
//            add
//            {
//                baseTextBox.SizeChanged += value;
//            }
//            remove
//            {
//                baseTextBox.SizeChanged -= value;
//            }
//        }

//        public new event EventHandler StyleChanged
//        {
//            add
//            {
//                baseTextBox.StyleChanged += value;
//            }
//            remove
//            {
//                baseTextBox.StyleChanged -= value;
//            }
//        }

//        public new event EventHandler SystemColorsChanged
//        {
//            add
//            {
//                baseTextBox.SystemColorsChanged += value;
//            }
//            remove
//            {
//                baseTextBox.SystemColorsChanged -= value;
//            }
//        }

//        public new event EventHandler TabIndexChanged
//        {
//            add
//            {
//                baseTextBox.TabIndexChanged += value;
//            }
//            remove
//            {
//                baseTextBox.TabIndexChanged -= value;
//            }
//        }

//        public new event EventHandler TabStopChanged
//        {
//            add
//            {
//                baseTextBox.TabStopChanged += value;
//            }
//            remove
//            {
//                baseTextBox.TabStopChanged -= value;
//            }
//        }

//        public event EventHandler TextAlignChanged
//        {
//            add
//            {
//                baseTextBox.TextAlignChanged += value;
//            }
//            remove
//            {
//                baseTextBox.TextAlignChanged -= value;
//            }
//        }

//        public new event EventHandler TextChanged
//        {
//            add
//            {
//                baseTextBox.TextChanged += value;
//            }
//            remove
//            {
//                baseTextBox.TextChanged -= value;
//            }
//        }

//        public new event EventHandler Validated
//        {
//            add
//            {
//                baseTextBox.Validated += value;
//            }
//            remove
//            {
//                baseTextBox.Validated -= value;
//            }
//        }

//        public new event CancelEventHandler Validating
//        {
//            add
//            {
//                baseTextBox.Validating += value;
//            }
//            remove
//            {
//                baseTextBox.Validating -= value;
//            }
//        }

//        public new event EventHandler VisibleChanged
//        {
//            add
//            {
//                baseTextBox.VisibleChanged += value;
//            }
//            remove
//            {
//                baseTextBox.VisibleChanged -= value;
//            }
//        }
//        # endregion

        public MaterialNumericUpDown()
        {
            // Material Properties
            UseAccent = true;

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable | ControlStyles.OptimizedDoubleBuffer | ControlStyles.DoubleBuffer, true);
            Font = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), 8.25f, FontStyle.Regular, GraphicsUnit.Point);
            BackColor = SkinManager.BackgroundColor;
            ForeColor = SkinManager.TextHighEmphasisColor;

            #region Setup properties and TextBox
            baseTextBox = new BaseTextBox()
            {
                BorderStyle = BorderStyle.None,
                Font = base.Font,
                ForeColor = SkinManager.TextHighEmphasisColor,
                Multiline = false,
                Location = new Point(LEFT_PADDING, HEIGHT / 2 - FONT_HEIGHT / 2),
                Width = Width - (LEFT_PADDING + RIGHT_PADDING),
                Height = FONT_HEIGHT
            };

            Binding binding = new Binding("Text", this, "Value");
            baseTextBox.DataBindings.Add(binding);

            Enabled = true;
            ReadOnly = false;
            Size = new Size(250, HEIGHT);

            if (!Controls.Contains(baseTextBox) && !DesignMode)
            {
                Controls.Add(baseTextBox);
            }

            baseTextBox.GotFocus += (sender, args) =>
            {
                if (Enabled)
                {
                    isFocused = true;
                    animationManager.StartNewAnimation(AnimationDirection.In);
                }
                else
                    base.Focus();
                UpdateRects();
            };
            baseTextBox.LostFocus += (sender, args) =>
            {
                isFocused = false;
                animationManager.StartNewAnimation(AnimationDirection.Out);
                UpdateRects();
            };

            baseTextBox.TextChanged += new EventHandler(Redraw);
            baseTextBox.BackColorChanged += new EventHandler(Redraw);

            cms.Opening += ContextMenuStripOnOpening;
            cms.OnItemClickStart += ContextMenuStripOnItemClickStart;
            ContextMenuStrip = cms;
            #endregion

            #region base Controls
            // get the updown buttons
            upDownButtons = base.Controls[0];
            if (upDownButtons == null || upDownButtons.GetType().FullName != "System.Windows.Forms.UpDownBase+UpDownButtons")
            {
                throw new ArgumentNullException(this.GetType().FullName + ": Can't find internal UpDown buttons field.");
            }

            textbox = base.Controls[1] as TextBox;
            if (textbox == null || textbox.GetType().FullName != "System.Windows.Forms.UpDownBase+UpDownEdit")
            {
                throw new ArgumentNullException(this.GetType().FullName + ": Can't find internal TextBox field.");
            }

            textbox.Visible = false;
            //base.Controls.RemoveAt(1);
            #endregion

            #region Animations
            animationManager = new AnimationManager(true)
            {
                Increment = 0.06,
                AnimationType = AnimationType.EaseInOut,
                InterruptAnimation = false
            };
            animationManager.OnAnimationProgress += sender => Invalidate();
            LostFocus += (sender, args) =>
            {
                MouseState = MouseState.OUT;
            };
            GotFocus += (sender, args) =>
            {
                animationManager.StartNewAnimation(AnimationDirection.In);
                Invalidate();
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
            KeyUp += (sender, args) =>
            {
                if (Enabled)
                {
                    Invalidate();
                }
            };
            #endregion
        }

        #region Overrides
        protected override void OnPaint(PaintEventArgs pevent)
        {
            var g = pevent.Graphics;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            g.Clear(Parent.BackColor);
            SolidBrush backBrush = new SolidBrush(DrawHelper.BlendColor(Parent.BackColor, SkinManager.BackgroundAlternativeColor, SkinManager.BackgroundAlternativeColor.A));

            //backColor
            g.FillRectangle(
                !Enabled ? SkinManager.BackgroundDisabledBrush : // Disabled
                isFocused ? SkinManager.BackgroundFocusBrush :  // Focused
                MouseState == MouseState.HOVER && (!ReadOnly || (ReadOnly && !AnimateReadOnly)) ? SkinManager.BackgroundHoverBrush : // Hover
                backBrush, // Normal
                ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, LINE_Y);

            baseTextBox.BackColor = !Enabled ? ColorHelper.RemoveAlpha(SkinManager.BackgroundDisabledColor, BackColor) : //Disabled
                isFocused ? DrawHelper.BlendColor(BackColor, SkinManager.BackgroundFocusColor, SkinManager.BackgroundFocusColor.A) : //Focused
                MouseState == MouseState.HOVER && (!ReadOnly || (ReadOnly && !AnimateReadOnly)) ? DrawHelper.BlendColor(BackColor, SkinManager.BackgroundHoverColor, SkinManager.BackgroundHoverColor.A) : // Hover
                DrawHelper.BlendColor(BackColor, SkinManager.BackgroundAlternativeColor, SkinManager.BackgroundAlternativeColor.A); // Normal

            // bottom line base
            g.FillRectangle(SkinManager.DividersAlternativeBrush, 0, LINE_Y, Width, 1);

            if (ReadOnly == false || (ReadOnly && AnimateReadOnly))
            {
                if (!animationManager.IsAnimating())
                {
                    // bottom line
                    g.FillRectangle(isFocused ? UseAccent ? SkinManager.ColorScheme.AccentBrush : SkinManager.ColorScheme.PrimaryBrush : SkinManager.DividersBrush, 0, LINE_Y, Width, isFocused ? 2 : 1);
                }
                else
                {
                    // Animate - Focus got/lost
                    double animationProgress = animationManager.GetProgress();

                    // Line Animation
                    int LineAnimationWidth = (int)(Width * animationProgress);
                    int LineAnimationX = (Width / 2) - (LineAnimationWidth / 2);
                    g.FillRectangle(UseAccent ? SkinManager.ColorScheme.AccentBrush : SkinManager.ColorScheme.PrimaryBrush, LineAnimationX, LINE_Y, LineAnimationWidth, 2);
                }
            }
        }

        private void Redraw(object sencer, EventArgs e)
        {
            SuspendLayout();
            Invalidate();
            ResumeLayout(false);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (DesignMode)
                return;

            if (upDownButtons.Bounds.Contains(e.Location))
            {
                Cursor = Cursors.Hand;
            }
            else
            {
                Cursor = Cursors.IBeam;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (DesignMode)
                return;

            baseTextBox?.Focus();
            base.OnMouseDown(e);
        }
        protected override void OnMouseEnter(EventArgs e)
        {
            if (DesignMode)
                return;

            base.OnMouseEnter(e);
            MouseState = MouseState.HOVER;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (DesignMode)
                return;

            if (this.ClientRectangle.Contains(this.PointToClient(Control.MousePosition)))
                return;
            else
            {
                base.OnMouseLeave(e);
                MouseState = MouseState.OUT;
                Invalidate();
            }
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            MouseState = MouseState.OUT;
            setHeightVars();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateRects();
            setHeightVars();
        }

        public new bool Focus()
        {
            return baseTextBox.Focus();
        }

        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);
            baseTextBox.Focus();
        }

        #endregion

        #region Private methods
        private void UpdateRects()
        {
            left_padding = LEFT_PADDING;
            right_padding = RIGHT_PADDING;

            baseTextBox.Location = new Point(left_padding, (LINE_Y - ACTIVATION_INDICATOR_HEIGHT) / 2 - FONT_HEIGHT / 2);
            baseTextBox.Width = Width - (left_padding + right_padding);
            baseTextBox.Height = FONT_HEIGHT;

            using (NativeTextRenderer NativeText = new NativeTextRenderer(CreateGraphics()))
            {
                var textToMesure = string.IsNullOrEmpty(Text) ? "0" : Text;
                var newFontHeight = NativeText.MeasureString(textToMesure, Font).Height;
                baseTextBox.Location = new Point(left_padding, (LINE_Y + ACTIVATION_INDICATOR_HEIGHT) / 2 - newFontHeight / 2);
                baseTextBox.Height = newFontHeight;
            }
            baseTextBox.Width = Width - (left_padding + right_padding);
        }

        private void setHeightVars()
        {
            HEIGHT = 32;
            Size = new Size(Size.Width, HEIGHT);
            LINE_Y = HEIGHT - BOTTOM_PADDING;

            HEIGHT = Height < 10 ? 10 : Height;

            Size = new Size(Width, HEIGHT);
            LINE_Y = HEIGHT - ACTIVATION_INDICATOR_HEIGHT;
        }
        #endregion

        #region Events
        private void ContextMenuStripOnItemClickStart(object sender, ToolStripItemClickedEventArgs toolStripItemClickedEventArgs)
        {
            switch (toolStripItemClickedEventArgs.ClickedItem.Text)
            {
                case "Undo":
                    Undo();
                    break;
                case "Cut":
                    Cut();
                    break;
                case "Copy":
                    Copy();
                    break;
                case "Paste":
                    Paste();
                    break;
                case "Delete":
                    SelectedText = string.Empty;
                    break;
                case "Select All":
                    SelectAll();
                    break;
            }
        }

        private void ContextMenuStripOnOpening(object sender, CancelEventArgs cancelEventArgs)
        {
            var strip = sender as BaseTextBoxContextMenuStrip;
            if (strip != null)
            {
                strip.undo.Enabled = baseTextBox.CanUndo && !ReadOnly;
                strip.cut.Enabled = !string.IsNullOrEmpty(SelectedText) && !ReadOnly;
                strip.copy.Enabled = !string.IsNullOrEmpty(SelectedText);
                strip.paste.Enabled = Clipboard.ContainsText() && !ReadOnly;
                strip.delete.Enabled = !string.IsNullOrEmpty(SelectedText) && !ReadOnly;
                strip.selectAll.Enabled = !string.IsNullOrEmpty(Text);
            }
        }

        private void LeaveOnEnterKey_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                SendKeys.Send("{TAB}");
            }
        }
        #endregion

    }
}
