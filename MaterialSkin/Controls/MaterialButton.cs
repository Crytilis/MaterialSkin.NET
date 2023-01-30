namespace MaterialSkin.Controls
{
    using MaterialSkin.Animations;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Drawing.Text;
    using System.Globalization;
    using System.Windows.Forms;

    /// <summary>
    /// Defines the <see cref="MaterialButton" />
    /// </summary>
    public class MaterialButton : Button, IMaterialControl
    {

        private const int ICON_SIZE = 24;
        private const int MINIMUMWIDTH = 64;
        private const int MINIMUMWIDTHICONONLY = 36; //64;
        private const int HEIGHTDEFAULT = 36;
        private const int HEIGHTDENSE = 32;

        // icons
        private TextureBrush iconsBrushes;

        /// <summary>
        /// Gets or sets the Depth
        /// </summary>
        [Browsable(false)]
        public int Depth { get; set; }

        /// <summary>
        /// Gets the SkinManager
        /// </summary>
        [Browsable(false)]
        public MaterialSkinManager SkinManager => MaterialSkinManager.Instance;

        /// <summary>
        /// Gets or sets the MouseState
        /// </summary>
        [Browsable(false)]
        public MouseState MouseState { get; set; }

        public enum MaterialButtonType
        {
            Text,
            Outlined,
            Contained
        }

        public enum MaterialButtonColorType
        {
            Primary,
            Secondary,
            Tertiary,
            Success,
            Info,
            Warning,
            Danger
        }

        public enum MaterialButtonDensity
        {
            Default,
            Dense
        }

        [Browsable(false)]
        public Color NoAccentTextColor { get; set; }

        [Category("Material Skin")]
        public bool UseAccentColor
        {
            get => useAccentColor;
            set { useAccentColor = value; Invalidate(); }
        }

        [Category("Material Skin")]
        /// <summary>
        /// Gets or sets a value indicating whether HighEmphasis
        /// </summary>
        public bool HighEmphasis
        {
            get => highEmphasis;
            set { highEmphasis = value; Invalidate(); }
        }

        [DefaultValue(true)]
        [Category("Material Skin")]
        [Description("Draw Shadows around control")]
        public bool DrawShadows
        {
            get => drawShadows;
            set { drawShadows = value; Invalidate(); }
        }

        [Category("Material Skin")]
        [Description("Sets the type of the button")]
        public MaterialButtonType Type
        {
            get => type;
            set { type = value; preProcessIcons(); Invalidate(); }
        }

        [Category("Material Skin"), DefaultValue(MaterialButtonColorType.Primary), Description("Sets button color, works only when UseAccentColor is false")]
        public MaterialButtonColorType ColorType
        {
            get => colorType;
            set { colorType = value; preProcessIcons(); Invalidate(); }
        }

        [Category("Material Skin")]
        /// <summary>
        /// Gets or sets a value indicating button density
        /// </summary>
        public MaterialButtonDensity Density
        {
            get => _density;
            set
            {
                _density = value;
                if (_density == MaterialButtonDensity.Dense)
                    Size = new Size(Size.Width, HEIGHTDENSE);
                else
                    Size = new Size(Size.Width, HEIGHTDEFAULT);
                Invalidate();
            }
        }

        public enum CharacterCasingEnum
        {
            Normal,
            Lower,
            Upper,
            Title
        }

        public CharacterCasingEnum _cc;
        [Category("Behavior"), DefaultValue(CharacterCasingEnum.Upper), Description("Change capitalization of Text property")]
        public CharacterCasingEnum CharacterCasing
        {
            get => _cc;
            set
            {
                _cc = value;
                Invalidate();
            }
        }
        protected override void InitLayout()
        {
            base.InitLayout();
            Invalidate();
            LocationChanged += (sender, e) => { if (DrawShadows) Parent?.Invalidate(); };
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (drawShadows && Parent != null) AddShadowPaintEvent(Parent, drawShadowOnParent);
            if (_oldParent != null) RemoveShadowPaintEvent(_oldParent, drawShadowOnParent);
            _oldParent = Parent;
        }

        private Control _oldParent;

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Parent == null) return;
            if (Visible)
                AddShadowPaintEvent(Parent, drawShadowOnParent);
            else
                RemoveShadowPaintEvent(Parent, drawShadowOnParent);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        private bool _shadowDrawEventSubscribed = false;

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

        private readonly AnimationManager _hoverAnimationManager = null;
        private readonly AnimationManager _focusAnimationManager = null;
        private readonly AnimationManager _animationManager = null;

        /// <summary>
        /// Defines the _textSize
        /// </summary>
        private SizeF _textSize;

        /// <summary>
        /// Defines the _icon
        /// </summary>
        private Image _icon;

        private bool drawShadows;
        private bool highEmphasis;
        private bool useAccentColor;
        private MaterialButtonType type;
        private MaterialButtonColorType colorType;
        private MaterialButtonDensity _density;
        private int radius;

        [Category("Material Skin")]
        /// <summary>
        /// Gets or sets the Icon
        /// </summary>
        public Image Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                preProcessIcons();

                if (AutoSize)
                {
                    Refresh();
                }

                Invalidate();
            }
        }

        [Category("Material Skin"), DefaultValue(4), Description("Sets the border radius in px")]
        public int Radius
        {
            get => radius;
            set
            {
                if (value <= 0)
                    value = 4;

                if ((Math.Min(Width, Height) / 2) < value)
                    value = (Math.Min(Width, Height) / 2);

                radius = value;

                Invalidate();
            }
        }

        [DefaultValue(true)]
        public override bool AutoSize
        {
            get => base.AutoSize;
            set => base.AutoSize = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialButton"/> class.
        /// </summary>
        public MaterialButton()
        {
            DrawShadows = true;
            HighEmphasis = true;
            UseAccentColor = false;
            Type = MaterialButtonType.Contained;
            ColorType = MaterialButtonColorType.Primary;
            Density = MaterialButtonDensity.Default;
            NoAccentTextColor = Color.Empty;
            CharacterCasing = CharacterCasingEnum.Upper;

            _animationManager = new AnimationManager(false)
            {
                Increment = 0.03,
                AnimationType = AnimationType.EaseOut
            };
            _hoverAnimationManager = new AnimationManager
            {
                Increment = 0.12,
                AnimationType = AnimationType.Linear
            };
            _focusAnimationManager = new AnimationManager
            {
                Increment = 0.12,
                AnimationType = AnimationType.Linear
            };
            SkinManager.ColorSchemeChanged += sender =>
            {
                preProcessIcons();
            };

            SkinManager.ThemeChanged += sender =>
            {
                preProcessIcons();
            };

            _hoverAnimationManager.OnAnimationProgress += sender => Invalidate();
            _focusAnimationManager.OnAnimationProgress += sender => Invalidate();
            _animationManager.OnAnimationProgress += sender => Invalidate();

            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            AutoSize = true;
            Margin = new Padding(4, 6, 4, 6);
            Padding = new Padding(0);
            Radius = radius <= 0 ? 4 : radius;
        }

        /// <summary>
        /// Gets or sets the Text
        /// </summary>
        public override string Text
        {
            get => base.Text;
            set
            {
                base.Text = value;
                if (!string.IsNullOrEmpty(value))
                    _textSize = CreateGraphics().MeasureString(value.ToUpper(), SkinManager.getFontByType(MaterialSkinManager.fontType.Button));
                else
                {
                    _textSize.Width = 0;
                    _textSize.Height = 0;
                }

                if (AutoSize)
                {
                    Refresh();
                }

                Invalidate();
            }
        }

        private void drawShadowOnParent(object sender, PaintEventArgs e)
        {
            if (Parent == null)
            {
                RemoveShadowPaintEvent((Control)sender, drawShadowOnParent);
                return;
            }

            if (!DrawShadows || Type != MaterialButtonType.Contained || Parent == null) return;

            // paint shadow on parent
            Graphics gp = e.Graphics;
            Rectangle rect = new Rectangle(Location, ClientRectangle.Size);
            gp.SmoothingMode = SmoothingMode.AntiAlias;
            DrawHelper.DrawSquareShadow(gp, rect, Radius);
        }

        private void preProcessIcons()
        {
            if (Icon == null) return;

            int newWidth, newHeight;
            //Resize icon if greater than ICON_SIZE
            if (Icon.Width > ICON_SIZE || Icon.Height > ICON_SIZE)
            {
                //calculate aspect ratio
                float aspect = Icon.Width / (float)Icon.Height;

                //calculate new dimensions based on aspect ratio
                newWidth = (int)(ICON_SIZE * aspect);
                newHeight = (int)(newWidth / aspect);

                //if one of the two dimensions exceed the box dimensions
                if (newWidth > ICON_SIZE || newHeight > ICON_SIZE)
                {
                    //depending on which of the two exceeds the box dimensions set it as the box dimension and calculate the other one based on the aspect ratio
                    if (newWidth > newHeight)
                    {
                        newWidth = ICON_SIZE;
                        newHeight = (int)(newWidth / aspect);
                    }
                    else
                    {
                        newHeight = ICON_SIZE;
                        newWidth = (int)(newHeight * aspect);
                    }
                }
            }
            else
            {
                newWidth = Icon.Width;
                newHeight = Icon.Height;
            }

            Bitmap IconResized = new Bitmap(Icon, newWidth, newHeight);

            // Calculate lightness and color
            float l = (SkinManager.Theme == MaterialSkinManager.Themes.LIGHT & (highEmphasis == false | Enabled == false | Type != MaterialButtonType.Contained)) ? 0f : 1.5f;

            // Create matrices
            float[][] matrixGray = {
                    new float[] {   0,   0,   0,   0,  0}, // Red scale factor
                    new float[] {   0,   0,   0,   0,  0}, // Green scale factor
                    new float[] {   0,   0,   0,   0,  0}, // Blue scale factor
                    new float[] {   0,   0,   0, Enabled ? .7f : .3f,  0}, // alpha scale factor
                    new float[] {   l,   l,   l,   0,  1}};// offset


            ColorMatrix colorMatrixGray = new ColorMatrix(matrixGray);

            ImageAttributes grayImageAttributes = new ImageAttributes();

            // Set color matrices
            grayImageAttributes.SetColorMatrix(colorMatrixGray, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            // Image Rect
            Rectangle destRect = new Rectangle(0, 0, ICON_SIZE, ICON_SIZE);

            // Create a pre-processed copy of the image (GRAY)
            Bitmap bgray = new Bitmap(destRect.Width, destRect.Height);
            using (Graphics gGray = Graphics.FromImage(bgray))
            {
                gGray.DrawImage(IconResized,
                    new Point[] {
                                new Point(0, 0),
                                new Point(destRect.Width, 0),
                                new Point(0, destRect.Height),
                    },
                    destRect, GraphicsUnit.Pixel, grayImageAttributes);
            }

            // added processed image to brush for drawing
            TextureBrush textureBrushGray = new TextureBrush(bgray)
            {
                WrapMode = System.Drawing.Drawing2D.WrapMode.Clamp
            };

            // Translate the brushes to the correct positions
            Rectangle iconRect = new Rectangle(8, (Height / 2 - ICON_SIZE / 2), ICON_SIZE, ICON_SIZE);

            textureBrushGray.TranslateTransform(iconRect.X + iconRect.Width / 2 - IconResized.Width / 2,
                                                iconRect.Y + iconRect.Height / 2 - IconResized.Height / 2);

            iconsBrushes = textureBrushGray;
        }

        /// <summary>
        /// The OnPaint
        /// </summary>
        /// <param name="pevent">The pevent<see cref="PaintEventArgs"/></param>
        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;

            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            double hoverAnimProgress = _hoverAnimationManager.GetProgress();
            double focusAnimProgress = _focusAnimationManager.GetProgress();

            g.Clear(Parent.BackColor);

            // button rectand path
            RectangleF buttonRectF = new RectangleF(ClientRectangle.Location, ClientRectangle.Size);
            buttonRectF.X -= 0.5f;
            buttonRectF.Y -= 0.5f;
            GraphicsPath buttonPath = DrawHelper.CreateRoundRect(buttonRectF, Radius);

            // button shadow (blend with form shadow)
            DrawHelper.DrawSquareShadow(g, ClientRectangle, Radius);

            if (Type == MaterialButtonType.Contained)
            {
                // draw button rect
                // Disabled
                if (!Enabled)
                {
                    using (SolidBrush disabledBrush = new SolidBrush(DrawHelper.BlendColor(Parent.BackColor, SkinManager.BackgroundDisabledColor, SkinManager.BackgroundDisabledColor.A)))
                    {
                        g.FillPath(disabledBrush, buttonPath);
                    }
                }
                // High emphasis
                else if (HighEmphasis)
                {
                    Brush usedBrush = SkinManager.ColorScheme.AccentBrush;

                    if (!UseAccentColor)
                    {
                        switch (ColorType)
                        {
                            case MaterialButtonColorType.Primary:
                                usedBrush = SkinManager.ColorScheme.PrimaryBrush;
                                break;
                            case MaterialButtonColorType.Secondary:
                                usedBrush = SkinManager.ColorScheme.DarkPrimaryBrush;
                                break;
                            case MaterialButtonColorType.Tertiary:
                                usedBrush = SkinManager.ColorScheme.LightPrimaryBrush;
                                break;
                            case MaterialButtonColorType.Success:
                                usedBrush = new SolidBrush(SkinManager.ColorScheme.SuccessColor);
                                break;
                            case MaterialButtonColorType.Info:
                                usedBrush = new SolidBrush(SkinManager.ColorScheme.InfoColor);
                                break;
                            case MaterialButtonColorType.Warning:
                                usedBrush = new SolidBrush(SkinManager.ColorScheme.WarningColor);
                                break;
                            case MaterialButtonColorType.Danger:
                                usedBrush = new SolidBrush(SkinManager.ColorScheme.DangerColor);
                                break;
                            default:
                                usedBrush = SkinManager.ColorScheme.PrimaryBrush;
                                break;
                        }
                    }

                    g.FillPath(usedBrush, buttonPath);
                }
                // Normal
                else
                {
                    using (SolidBrush normalBrush = new SolidBrush(SkinManager.BackgroundColor))
                    {
                        g.FillPath(normalBrush, buttonPath);
                    }
                }
            }
            else
            {
                g.Clear(Parent.BackColor);
            }

            #region get hover/focus color
            Color hoverFocusColor = Color.Transparent;
            if (UseAccentColor)
            {
                if (HighEmphasis && Type == MaterialButtonType.Contained)
                {
                    // Contained with Emphasis - with accent
                    hoverFocusColor = SkinManager.ColorScheme.AccentColor.Lighten(0.5f);
                }
                else
                {
                    // Not Contained Or Low Emphasis - with accent
                    hoverFocusColor = SkinManager.ColorScheme.AccentColor;
                }
            }
            else
            {
                if (Type == MaterialButtonType.Contained && HighEmphasis)
                {
                    // Contained with Emphasis without accent
                    hoverFocusColor = GetColorByType().Lighten(0.5f);
                }
                else
                {
                    // Normal or Emphasis without accent
                    hoverFocusColor = GetColorByType();
                }
            }
            #endregion

            //Hover
            if (hoverAnimProgress > 0)
            {
                using (SolidBrush hoverBrush = new SolidBrush(Color.FromArgb(
                    (int)(HighEmphasis && Type == MaterialButtonType.Contained ? hoverAnimProgress * 80 : hoverAnimProgress * SkinManager.BackgroundHoverColor.A),
                    hoverFocusColor.RemoveAlpha())))
                {
                    g.FillPath(hoverBrush, buttonPath);
                }
            }

            //Focus
            if (focusAnimProgress > 0)
            {
                using (SolidBrush focusBrush = new SolidBrush(Color.FromArgb(
                    (int)(HighEmphasis && Type == MaterialButtonType.Contained ? focusAnimProgress * 80 : focusAnimProgress * SkinManager.BackgroundFocusColor.A),
                    hoverFocusColor.RemoveAlpha())))
                {
                    g.FillPath(focusBrush, buttonPath);
                }
            }

            if (Type == MaterialButtonType.Outlined)
            {
                Color outColor = SkinManager.DividersColor;
                if (Enabled)
                {
                    if (UseAccentColor)
                        outColor = SkinManager.ColorScheme.AccentColor;
                    else if (highEmphasis)
                    {
                        outColor = GetColorByType();
                    }

                    if (outColor == SkinManager.DividersColor)
                        outColor = SkinManager.DividersAlternativeColor;
                }

                using (Pen outlinePen = new Pen(outColor, 1))
                {
                    buttonRectF.X += 0.5f;
                    buttonRectF.Y += 0.5f;
                    g.DrawPath(outlinePen, buttonPath);
                }
            }

            //Ripple
            if (_animationManager.IsAnimating())
            {
                //g.Clip = new Region(buttonRectF);
                g.Clip = new Region(buttonPath);
                for (int i = 0; i < _animationManager.GetAnimationCount(); i++)
                {
                    double animationValue = _animationManager.GetProgress(i);
                    Point animationSource = _animationManager.GetSource(i);

                    Color rippleColor;
                    if (Type == MaterialButtonType.Contained && HighEmphasis)
                    {
                        if (UseAccentColor)
                        {
                            // Emphasis with accent
                            rippleColor = SkinManager.ColorScheme.AccentColor.Lighten(0.5f);
                        }
                        else
                        {
                            // Emphasis
                            rippleColor = GetColorByType();
                        }
                    }
                    else
                    {
                        if (UseAccentColor)
                        {
                            // Normal with accent
                            rippleColor = SkinManager.ColorScheme.AccentColor;
                        }
                        else
                        {
                            if (SkinManager.Theme == MaterialSkinManager.Themes.LIGHT)
                            {
                                rippleColor = GetColorByType();
                            }
                            else
                            {
                                rippleColor = GetColorByType().Lighten(0.5f);
                            }
                        }
                    }

                    using (Brush rippleBrush = new SolidBrush(Color.FromArgb((int)(100 - (animationValue * 100)), // Alpha animation
                                                              rippleColor))) // Normal
                    {
                        int rippleSize = (int)(animationValue * Width * 2);
                        g.FillEllipse(rippleBrush, new Rectangle(animationSource.X - rippleSize / 2, animationSource.Y - rippleSize / 2, rippleSize, rippleSize));
                    }
                }
                g.ResetClip();
            }

            //Text
            Rectangle textRect = ClientRectangle;
            if (Icon != null)
            {
                textRect.Width -= 8 + ICON_SIZE + 4 + 8; // left padding + icon width + space between Icon and Text + right padding
                textRect.X += 8 + ICON_SIZE + 4; // left padding + icon width + space between Icon and Text
            }
            /*
            Color textColor = Enabled ? (HighEmphasis ? (Type == MaterialButtonType.Text || Type == MaterialButtonType.Outlined) ?
                UseAccentColor ? SkinManager.ColorScheme.AccentColor : // Outline or Text and accent and emphasis
                NoAccentTextColor == Color.Empty ?
                SkinManager.ColorScheme.PrimaryColor :  // Outline or Text and emphasis
                NoAccentTextColor : // User defined Outline or Text and emphasis
                SkinManager.ColorScheme.TextColor : // Contained and Emphasis
                SkinManager.TextHighEmphasisColor) : // Cointained and accent
                SkinManager.TextDisabledOrHintColor; // Disabled
            */
            Color textColor = SkinManager.TextDisabledOrHintColor;
            if (Enabled)
            {
                if (HighEmphasis)
                {
                    if ((Type == MaterialButtonType.Text || Type == MaterialButtonType.Outlined))
                    {
                        if (UseAccentColor)
                        {
                            textColor = SkinManager.ColorScheme.AccentColor;
                            // Outline or Text and accent and emphasis
                        }
                        else
                        {
                            if (NoAccentTextColor == Color.Empty)
                            {
                                //Outline or Text and emphasis no accent
                                textColor = GetColorByType();
                            }
                            else
                            {
                                //User defined Outline or Text and emphasis
                                textColor = NoAccentTextColor;
                            }
                        }
                    }
                    else
                    {
                        //Contained and Emphasis
                        textColor = SkinManager.ColorScheme.TextColor;
                    }
                }
                else
                {
                    //Cointained and accent
                    textColor = SkinManager.TextHighEmphasisColor;
                }
            }

            using (NativeTextRenderer NativeText = new NativeTextRenderer(g))
            {
                NativeText.DrawMultilineTransparentText(
                    CharacterCasing == CharacterCasingEnum.Upper ? base.Text.ToUpper() : CharacterCasing == CharacterCasingEnum.Lower ? base.Text.ToLower() :
                        CharacterCasing == CharacterCasingEnum.Title ? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(base.Text.ToLower()) : base.Text,
                    SkinManager.getLogFontByType(MaterialSkinManager.fontType.Button),
                    textColor,
                    textRect.Location,
                    textRect.Size,
                    NativeTextRenderer.TextAlignFlags.Center | NativeTextRenderer.TextAlignFlags.Middle);
            }

            //Icon
            Rectangle iconRect = new Rectangle(8, (Height / 2) - (ICON_SIZE / 2), ICON_SIZE, ICON_SIZE);

            if (string.IsNullOrEmpty(Text))
            {
                // Center Icon
                iconRect.X += 2;
            }

            if (Icon != null)
            {
                g.FillRectangle(iconsBrushes, iconRect);
            }
        }

        private Color GetColorByType()
        {
            switch (ColorType)
            {
                case MaterialButtonColorType.Primary:
                    return SkinManager.ColorScheme.PrimaryColor;
                case MaterialButtonColorType.Secondary:
                    return SkinManager.ColorScheme.DarkPrimaryColor;
                case MaterialButtonColorType.Tertiary:
                    return SkinManager.ColorScheme.LightPrimaryColor;
                case MaterialButtonColorType.Success:
                    return SkinManager.ColorScheme.SuccessColor;
                case MaterialButtonColorType.Info:
                    return SkinManager.ColorScheme.InfoColor;
                case MaterialButtonColorType.Warning:
                    return SkinManager.ColorScheme.WarningColor;
                case MaterialButtonColorType.Danger:
                    return SkinManager.ColorScheme.DangerColor;
                default:
                    return SkinManager.ColorScheme.LightPrimaryColor;
            }
        }

        /// <summary>
        /// The GetPreferredSize
        /// </summary>
        /// <returns>The <see cref="Size"/></returns>
        private Size GetPreferredSize()
        {
            return GetPreferredSize(Size);
        }

        /// <summary>
        /// The GetPreferredSize
        /// </summary>
        /// <param name="proposedSize">The proposedSize<see cref="Size"/></param>
        /// <returns>The <see cref="Size"/></returns>
        public override Size GetPreferredSize(Size proposedSize)
        {
            Size s = base.GetPreferredSize(proposedSize);

            // Provides extra space for proper padding for content
            int extra = 16;

            if (Icon != null)
            {
                // 24 is for icon size
                // 4 is for the space between icon & text
                extra += ICON_SIZE + 4;
            }

            if (AutoSize)
            {
                s.Width = (int)Math.Ceiling(_textSize.Width);
                s.Width += extra;
                s.Height = HEIGHTDEFAULT;
            }
            else
            {
                s.Width += extra;
                s.Height = HEIGHTDEFAULT;
            }
            if (Icon != null && Text.Length == 0 && s.Width < MINIMUMWIDTHICONONLY) s.Width = MINIMUMWIDTHICONONLY;
            else if (s.Width < MINIMUMWIDTH) s.Width = MINIMUMWIDTH;

            return s;
        }

        /// <summary>
        /// The OnCreateControl
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            // before checking DesignMode property, as long as we need see Icon in proper position
            Resize += (sender, args) => { preProcessIcons(); Invalidate(); };

            if (DesignMode)
            {
                return;
            }

            MouseState = MouseState.OUT;
            MouseEnter += (sender, args) =>
            {
                MouseState = MouseState.HOVER;
                _hoverAnimationManager.StartNewAnimation(AnimationDirection.In);
                Invalidate();
            };
            MouseLeave += (sender, args) =>
            {
                MouseState = MouseState.OUT;
                _hoverAnimationManager.StartNewAnimation(AnimationDirection.Out);
                Invalidate();
            };
            MouseDown += (sender, args) =>
            {
                if (args.Button == MouseButtons.Left)
                {
                    MouseState = MouseState.DOWN;

                    _animationManager.StartNewAnimation(AnimationDirection.In, args.Location);
                    Invalidate();
                }
            };
            MouseUp += (sender, args) =>
            {
                MouseState = MouseState.HOVER;

                Invalidate();
            };

            GotFocus += (sender, args) =>
            {
                _focusAnimationManager.StartNewAnimation(AnimationDirection.In);
                Invalidate();
            };
            LostFocus += (sender, args) =>
            {
                MouseState = MouseState.OUT;
                _focusAnimationManager.StartNewAnimation(AnimationDirection.Out);
                Invalidate();
            };

            PreviewKeyDown += (object sender, PreviewKeyDownEventArgs e) =>
            {
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
                {
                    _animationManager.StartNewAnimation(AnimationDirection.In, new Point(ClientRectangle.Width >> 1, ClientRectangle.Height >> 1));
                    Invalidate();
                }
            };
        }
    }
}
