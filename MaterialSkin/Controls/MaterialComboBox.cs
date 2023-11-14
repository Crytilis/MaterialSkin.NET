namespace MaterialSkin.Controls
{
    using MaterialSkin.Animations;
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;

    public class MaterialComboBox : ComboBox, IMaterialControl
    {
        // For some reason, even when overriding the AutoSize property, it doesn't appear on the properties panel, so we have to create a new one.
        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), Category("Layout"), DefaultValue(false)]
        private bool _AutoResize;

        public bool AutoResize
        {
            get { return _AutoResize; }
            set
            {
                _AutoResize = value;
                recalculateAutoSize();
            }
        }

        private Size size;
        [Category("Material Skin"), Localizable(true), DefaultValue(typeof(Size), "0; 27")]
        public Size CustomSize
        {
            get
            {
                return size.Width == 0 ? base.Size : size;
            }
            set
            {
                size = value;
                setHeightVars();
                Invalidate();
            }
        }

        //Properties for managing the material design properties
        [Browsable(false)]
        public int Depth { get; set; }

        [Browsable(false)]
        public MaterialSkinManager SkinManager => MaterialSkinManager.Instance;

        [Browsable(false)]
        public MouseState MouseState { get; set; }

        [Category("Appearance"), Localizable(true)]
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

        private bool _UseTallSize;

        [Category("Material Skin"), DefaultValue(true), Description("Using a larger size enables the hint to always be visible")]
        public bool UseTallSize
        {
            get { return _UseTallSize; }
            set
            {
                _UseTallSize = value;
                setHeightVars();
                Invalidate();
                if (editBox != null) editBox._UseTallSize = value;
            }
        }

        private bool useCustomHeight;

        [Category("Material Skin"), DefaultValue(true)]
        public bool UseCustomHeight
        {
            get { return useCustomHeight; }
            set
            {
                useCustomHeight = value;
                setHeightVars();
                Invalidate();
                if (editBox != null) editBox._UseTallSize = value;
            }
        }

        [Category("Material Skin"), DefaultValue(true)]
        public bool UseAccent { get; set; }

        private string _hint = string.Empty;

        [Category("Material Skin"), DefaultValue(""), Localizable(true)]
        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                hasHint = !String.IsNullOrEmpty(Hint);
                Invalidate();
                if (editBox != null) editBox.Hint = value;
            }
        }

        private Font hintFont;

        [Category("Material Skin")]
        public Font HintFont
        {
            get => hintFont;
            set
            {
                hintFont = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), value.SizeInPoints, value.Style, GraphicsUnit.Point);
                Invalidate();
            }
        }

        private Padding hintPadding;

        [Category("Material Skin"), Localizable(true)]
        public Padding HintPadding
        {
            get => hintPadding;
            set
            {
                hintPadding = value;
                Invalidate();
            }
        }

        [DefaultValue(null)]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Bindable(true)]
        public new object SelectedValue
        {
            get
            {
                return base.SelectedValue;
            }
            set
            {
                base.SelectedValue = value;
            }
        }

        private int _startIndex;
        public int StartIndex
        {
            get => _startIndex;
            set
            {
                _startIndex = value;
                try
                {
                    if (base.Items.Count > 0)
                    {
                        base.SelectedIndex = value;
                    }
                }
                catch
                {
                }
                Invalidate();
            }
        }

        private const int TEXT_SMALL_SIZE = 18;
        private const int TEXT_SMALL_Y = 4;
        private const int BOTTOM_PADDING = 3;

        #region constants adjusted for dpi
        private int TextSmallSize;
        private int TextSmallY;
        private int BottomPadding;
        #endregion

        private int HEIGHT = 50;
        private int LINE_Y;

        private bool hasHint;

        private readonly AnimationManager _animationManager;

        #region Editable DropDown
        private MaterialTextBox2 editBox;

        public new event EventHandler TextChanged;

        private string _text;
        public override string Text { 
            get
            {
                return _text;
            }
            set
            {
                if (editBox != null) editBox.Text = value;
                _text = value;
                TextChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        #endregion

        public MaterialComboBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            // Material Properties
            Hint = "";
            UseAccent = true;
            UseTallSize = true;
            MaxDropDownItems = 4;

            //Font = SkinManager.getFontByType(MaterialSkinManager.fontType.Subtitle1);
            //HintFont = SkinManager.getFontByType(MaterialSkinManager.fontType.Caption);
            Font = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), 8.25f, FontStyle.Regular, GraphicsUnit.Point);
            HintFont = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), 7.25f, FontStyle.Regular, GraphicsUnit.Point);
            HintPadding = new Padding(10, -5, 0, 0);
            UseCustomHeight = true;
            CustomSize = new Size(Width, 27);
            BackColor = SkinManager.BackgroundColor;
            ForeColor = SkinManager.TextHighEmphasisColor;
            DrawMode = DrawMode.OwnerDrawVariable;
            DropDownStyle = ComboBoxStyle.DropDownList;
            DropDownWidth = Width;

            // Animations
            _animationManager = new AnimationManager(true)
            {
                Increment = 0.08,
                AnimationType = AnimationType.EaseInOut
            };
            _animationManager.OnAnimationProgress += sender => Invalidate();
            _animationManager.OnAnimationFinished += sender => _animationManager.SetProgress(0);
            DropDownClosed += (sender, args) =>
            {
                MouseState = MouseState.OUT;
                if (SelectedIndex < 0 && !Focused) _animationManager.StartNewAnimation(AnimationDirection.Out);
            };
            DropDown += (sender, args) =>
            {
                _animationManager.StartNewAnimation(AnimationDirection.In);
            };
            GotFocus += (sender, args) =>
            {
                _animationManager.StartNewAnimation(AnimationDirection.In);
                Invalidate();
            };
            LostFocus += (sender, args) =>
            {
                MouseState = MouseState.OUT;
                if (SelectedIndex < 0) _animationManager.StartNewAnimation(AnimationDirection.Out);
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
            SelectedIndexChanged += (sender, args) =>
            {
                Invalidate();
            };
            KeyUp += (sender, args) =>
            { 
                if (Enabled && DropDownStyle == ComboBoxStyle.DropDownList && (args.KeyCode == Keys.Delete || args.KeyCode == Keys.Back))
                {
                    SelectedIndex = -1;
                    Invalidate();
                }
            };
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;

            g.Clear(Parent.BackColor);
            g.FillRectangle(Enabled ? Focused ?
                SkinManager.BackgroundFocusBrush : // Focused
                MouseState == MouseState.HOVER ?
                SkinManager.BackgroundHoverBrush : // Hover
                SkinManager.BackgroundAlternativeBrush : // normal
                SkinManager.BackgroundDisabledBrush // Disabled
                , ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, LINE_Y);

            //Set color and brush
            Color SelectedColor = new Color();
            if (UseAccent)
                SelectedColor = SkinManager.ColorScheme.AccentColor;
            else
                SelectedColor = SkinManager.ColorScheme.PrimaryColor;
            SolidBrush SelectedBrush = new SolidBrush(SelectedColor);

            // Create and Draw the arrow
            System.Drawing.Drawing2D.GraphicsPath pth = new System.Drawing.Drawing2D.GraphicsPath();
            PointF TopRight = new PointF(this.Width - 0.5f - SkinManager.FORM_PADDING, (this.Height >> 1) - 2.5f);
            PointF MidBottom = new PointF(this.Width - 4.5f - SkinManager.FORM_PADDING, (this.Height >> 1) + 2.5f);
            PointF TopLeft = new PointF(this.Width - 8.5f - SkinManager.FORM_PADDING, (this.Height >> 1) - 2.5f);
            pth.AddLine(TopLeft, TopRight);
            pth.AddLine(TopRight, MidBottom);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.FillPath((SolidBrush)(Enabled ? DroppedDown || Focused ?
                SelectedBrush : //DroppedDown or Focused
                SkinManager.TextHighEmphasisBrush : //Not DroppedDown and not Focused
                new SolidBrush(DrawHelper.BlendColor(SkinManager.TextHighEmphasisColor, SkinManager.SwitchOffDisabledThumbColor, 197))  //Disabled
                ), pth);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

            // HintText
            bool userTextPresent = SelectedIndex >= 0;
            Rectangle hintRect = new Rectangle(SkinManager.FORM_PADDING, ClientRectangle.Y, Width, LINE_Y);
            float hintTextSize = SelectedIndex >= 0 ? HintFont.SizeInPoints : Font.SizeInPoints;

            // bottom line base
            g.FillRectangle(SkinManager.DividersAlternativeBrush, 0, LINE_Y, Width, 1);

            if (!_animationManager.IsAnimating())
            {
                // No animation
                if (hasHint && UseTallSize && (DroppedDown || Focused || SelectedIndex >= 0))
                {
                    // hint text
                    hintRect = new Rectangle(HintPadding.Left, HintPadding.Top, Width - HintPadding.Left - HintPadding.Right, TEXT_SMALL_SIZE);
                    hintTextSize = SelectedIndex >= 0 ? HintFont.SizeInPoints : Font.SizeInPoints;
                }

                // bottom line
                if (DroppedDown || Focused)
                {
                    g.FillRectangle(SelectedBrush, 0, LINE_Y, Width, 2);
                }
            }
            else
            {
                // Animate - Focus got/lost
                double animationProgress = _animationManager.GetProgress();

                // hint Animation
                if (hasHint && UseTallSize)
                {
                    hintRect = new Rectangle(
                        SkinManager.FORM_PADDING,
                        userTextPresent && !_animationManager.IsAnimating() || SelectedIndex >= 0 ? (TEXT_SMALL_Y) : ClientRectangle.Y + (int)((TEXT_SMALL_Y - ClientRectangle.Y) * animationProgress),
                        Width,
                        userTextPresent && !_animationManager.IsAnimating() || SelectedIndex >= 0 ? (TEXT_SMALL_SIZE) : (int)(LINE_Y + (TEXT_SMALL_SIZE - LINE_Y) * animationProgress));
                    hintTextSize = userTextPresent && !_animationManager.IsAnimating() || SelectedIndex >= 0 ? Font.SizeInPoints : (int)(Font.Size + (8 - Font.Size) * animationProgress);
                }

                // Line Animation
                int LineAnimationWidth = (int)(Width * animationProgress);
                int LineAnimationX = (Width / 2) - (LineAnimationWidth / 2);
                g.FillRectangle(SelectedBrush, LineAnimationX, LINE_Y, LineAnimationWidth, 2);
            }

            // Calc text Rect
            Rectangle textRect = new Rectangle(
                SkinManager.FORM_PADDING,
                hasHint && UseTallSize ? (hintRect.Y + hintRect.Height) - 2 : ClientRectangle.Y,
                ClientRectangle.Width - SkinManager.FORM_PADDING * 3 - 8,
                hasHint && UseTallSize ? LINE_Y - (hintRect.Y + hintRect.Height) : LINE_Y);

            g.Clip = new Region(textRect);

            if (Text != null)
            using (NativeTextRenderer NativeText = new NativeTextRenderer(g))
            {
                // Draw user text
                NativeText.DrawTransparentText(
                    Text,
                    Font,
                    Enabled ? SkinManager.TextHighEmphasisColor : SkinManager.TextDisabledOrHintColor,
                    textRect.Location,
                    textRect.Size,
                    NativeTextRenderer.TextAlignFlags.Left | NativeTextRenderer.TextAlignFlags.Middle);
            }

            g.ResetClip();

            // Draw hint text
            if (hasHint && (UseTallSize || String.IsNullOrEmpty(Text)))
            {
                using (NativeTextRenderer NativeText = new NativeTextRenderer(g))
                {
                    var font = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), hintTextSize, HintFont.Style, GraphicsUnit.Point);

                    NativeText.DrawTransparentText(
                    Hint,
                    font,
                    Enabled ? DroppedDown || Focused ?
                    SelectedColor : // Focus 
                    SkinManager.TextMediumEmphasisColor : // not focused
                    SkinManager.TextDisabledOrHintColor, // Disabled
                    hintRect.Location,
                    hintRect.Size,
                    NativeTextRenderer.TextAlignFlags.Left | NativeTextRenderer.TextAlignFlags.Middle);
                }
            }
        }

        private void CustomMeasureItem(object sender, System.Windows.Forms.MeasureItemEventArgs e)
        {
            e.ItemHeight = HEIGHT - 7;
        }

        private void CustomDrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            //if (e.Index < 0 || e.Index > Items.Count || !Focused) return;

            Graphics g = e.Graphics;
            // Draw the background of the item.
            g.FillRectangle(SkinManager.BackgroundBrush, e.Bounds);

            // Hover
            if (e.State.HasFlag(DrawItemState.Focus)) // Focus == hover
            {
                g.FillRectangle(SkinManager.BackgroundHoverBrush, e.Bounds);
            }
            
            string Text = "";
            if (!string.IsNullOrWhiteSpace(DisplayMember))
            {
                if (!Items[e.Index].GetType().Equals(typeof(DataRowView)))
                {
                    var item = Items[e.Index].GetType().GetProperty(DisplayMember).GetValue(Items[e.Index]);
                    Text = item?.ToString();
                }
                else
                {
                    var table = ((DataRow)Items[e.Index].GetType().GetProperty("Row").GetValue(Items[e.Index]));
                    Text = table[DisplayMember].ToString();
                }
            }
            else
            {
                if (e.Index >= 0 && e.Index < Items.Count)
                {
                    Text = Items[e.Index].ToString();
                }
            }

            using (NativeTextRenderer NativeText = new NativeTextRenderer(g))
            {
                NativeText.DrawTransparentText(
                Text,
                Font,
                SkinManager.TextHighEmphasisNoAlphaColor,
                new Point(e.Bounds.Location.X + SkinManager.FORM_PADDING, e.Bounds.Location.Y),
                new Size(e.Bounds.Size.Width - SkinManager.FORM_PADDING * 2, e.Bounds.Size.Height),
                NativeTextRenderer.TextAlignFlags.Left | NativeTextRenderer.TextAlignFlags.Middle); ;
            }
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            MouseState = MouseState.OUT;
            MeasureItem += CustomMeasureItem;
            DrawItem += CustomDrawItem;
            DrawMode = DrawMode.OwnerDrawVariable;
            
            recalculateAutoSize();
            setHeightVars();

            if (!DesignMode)
            {
                DropDownStyle = ComboBoxStyle.DropDownList;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            recalculateAutoSize();
            setHeightVars();
        }

        private void setHeightVars()
        {
            var defaultHeight = UseTallSize ? 42 : 32;
            HEIGHT = UseCustomHeight ? (CustomSize.Height < 16 ? 16 : CustomSize.Height) : defaultHeight;
            Size = new Size(Size.Width, HEIGHT);
            LINE_Y = HEIGHT - BOTTOM_PADDING;
            ItemHeight = HEIGHT - 7;
            DropDownHeight = ItemHeight * MaxDropDownItems + 2;
        }

        public void recalculateAutoSize()
        {
            if (!AutoResize) return;

            int w = DropDownWidth;
            int padding = SkinManager.FORM_PADDING * 3;
            int vertScrollBarWidth = (Items.Count > MaxDropDownItems) ? SystemInformation.VerticalScrollBarWidth : 0;

            Graphics g = CreateGraphics();
            using (NativeTextRenderer NativeText = new NativeTextRenderer(g))
            {
                var itemsList = this.Items.Cast<object>().Select(item => item.ToString());
                foreach (string s in itemsList)
                {
                    int newWidth = NativeText.MeasureString(s, Font).Width + vertScrollBarWidth + padding;
                    if (w < newWidth) w = newWidth;
                }
            }

            if (Width != w)
            {
                DropDownWidth = w;
                Width = w;
            }
        }
    }
}
