namespace MaterialSkin.Controls
{
    using MaterialSkin.Animations;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Text;
    using System.Linq;
    using System.Windows.Forms;

    public class MaterialRadioButton : RadioButton, IMaterialControl
    {
        public enum Level { Parent, Form };

        #region Private variables
        // animation managers
        private readonly AnimationManager _checkAM;

        private readonly AnimationManager _rippleAM;
        private readonly AnimationManager _hoverAM;

        // size related variables which should be recalculated onsizechanged
        private Rectangle _radioButtonBounds;

        private bool ripple;
        private int _boxOffset;
        private bool hovered = false;

        // size constants
        private const int HEIGHT_RIPPLE = 37;

        private const int HEIGHT_NO_RIPPLE = 20;
        private const int RADIOBUTTON_SIZE = 18;
        private const int RADIOBUTTON_SIZE_HALF = RADIOBUTTON_SIZE / 2;
        private const int RADIOBUTTON_OUTER_CIRCLE_WIDTH = 2;
        private const int RADIOBUTTON_INNER_CIRCLE_SIZE = RADIOBUTTON_SIZE - (2 * RADIOBUTTON_OUTER_CIRCLE_WIDTH);
        private const int TEXT_OFFSET = 26;
        #endregion
        #region Properties
        [Browsable(false)]
        public int Depth { get; set; }

        [Browsable(false)]
        public MaterialSkinManager SkinManager => MaterialSkinManager.Instance;

        [Browsable(false)]
        public MouseState MouseState { get; set; }

        [Browsable(false)]
        public Point MouseLocation { get; set; }

        [Category("Behavior")]
        public bool Ripple
        {
            get { return ripple; }
            set
            {
                ripple = value;
                AutoSize = AutoSize; //Make AutoSize directly set the bounds.

                if (value)
                {
                    Margin = new Padding(0);
                }

                Invalidate();
            }
        }

        [Category("Behavior"),
        Description("Gets or sets the level that specifies which RadioButton controls are affected."),
        DefaultValue(Level.Parent)]
        public Level GroupNameLevel { get; set; }

        [Category("Behavior"),
        Description("Gets or sets the name that specifies which RadioButton controls are mutually exclusive.")]
        public string GroupName { get; set; }
        #endregion

        public MaterialRadioButton()
        {
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer, true);

            _checkAM = new AnimationManager
            {
                AnimationType = AnimationType.EaseInOut,
                Increment = 0.06
            };
            _hoverAM = new AnimationManager(true)
            {
                AnimationType = AnimationType.Linear,
                Increment = 0.10
            };
            _rippleAM = new AnimationManager(false)
            {
                AnimationType = AnimationType.Linear,
                Increment = 0.10,
                SecondaryIncrement = 0.08
            };

            _checkAM.OnAnimationProgress += sender => Invalidate();
            _hoverAM.OnAnimationProgress += sender => Invalidate();
            _rippleAM.OnAnimationProgress += sender => Invalidate();

            TabStopChanged += (sender, e) => TabStop = true;

            CheckedChanged += (sender, args) =>
            {
                if (Ripple)
                    _checkAM.StartNewAnimation(Checked ? AnimationDirection.In : AnimationDirection.Out);
            };

            SizeChanged += OnSizeChanged;

            Ripple = true;
            MouseLocation = new Point(-1, -1);
        }

        #region Overrides
        public override Size GetPreferredSize(Size proposedSize)
        {
            Size strSize;

            using (NativeTextRenderer NativeText = new NativeTextRenderer(CreateGraphics()))
            {
                strSize = NativeText.MeasureLogString(Text, SkinManager.getLogFontByType(MaterialSkinManager.fontType.Body1));
            }

            int w = _boxOffset + TEXT_OFFSET + strSize.Width;
            return Ripple ? new Size(w, HEIGHT_RIPPLE) : new Size(w, HEIGHT_NO_RIPPLE);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            // clear the control
            g.Clear(Parent.BackColor);

            int RADIOBUTTON_CENTER = _boxOffset + RADIOBUTTON_SIZE_HALF;
            Point animationSource = new Point(RADIOBUTTON_CENTER, RADIOBUTTON_CENTER);

            double animationProgress = _checkAM.GetProgress();

            int colorAlpha = Enabled ? (int)(animationProgress * 255.0) : SkinManager.CheckBoxOffDisabledColor.A;
            int backgroundAlpha = Enabled ? (int)(SkinManager.CheckboxOffColor.A * (1.0 - animationProgress)) : SkinManager.CheckBoxOffDisabledColor.A;
            float animationSize = (float)(animationProgress * 9f);
            float animationSizeHalf = animationSize / 2;
            int rippleHeight = (HEIGHT_RIPPLE % 2 == 0) ? HEIGHT_RIPPLE - 3 : HEIGHT_RIPPLE - 2;

            Color RadioColor = Color.FromArgb(colorAlpha, Enabled ? SkinManager.ColorScheme.AccentColor : SkinManager.CheckBoxOffDisabledColor);

            // draw hover animation
            if (Ripple)
            {
                double animationValue = _hoverAM.GetProgress();
                int rippleSize = (int)(rippleHeight * (0.7 + (0.3 * animationValue)));

                using (SolidBrush rippleBrush = new SolidBrush(Color.FromArgb((int)(40 * animationValue),
                    !Checked ? (SkinManager.Theme == MaterialSkinManager.Themes.LIGHT ? Color.Black : Color.White) : RadioColor)))
                {
                    g.FillEllipse(rippleBrush, new Rectangle(animationSource.X - rippleSize / 2, animationSource.Y - rippleSize / 2, rippleSize - 1, rippleSize - 1));
                }
            }

            // draw ripple animation
            if (Ripple && _rippleAM.IsAnimating())
            {
                for (int i = 0; i < _rippleAM.GetAnimationCount(); i++)
                {
                    double animationValue = _rippleAM.GetProgress(i);
                    int rippleSize = (_rippleAM.GetDirection(i) == AnimationDirection.InOutIn) ? (int)(rippleHeight * (0.7 + (0.3 * animationValue))) : rippleHeight;

                    using (SolidBrush rippleBrush = new SolidBrush(Color.FromArgb((int)((animationValue * 40)), !Checked ? (SkinManager.Theme == MaterialSkinManager.Themes.LIGHT ? Color.Black : Color.White) : RadioColor)))
                    {
                        g.FillEllipse(rippleBrush, new Rectangle(animationSource.X - rippleSize / 2, animationSource.Y - rippleSize / 2, rippleSize - 1, rippleSize - 1));
                    }
                }
            }

            var width = RADIOBUTTON_SIZE - Padding.Left - Padding.Right;
            var height = RADIOBUTTON_SIZE - Padding.Top - Padding.Bottom;

            var x = (ClientRectangle.Height / 2) - (width / 2);
            var y = (ClientRectangle.Height / 2) - (height / 2);

            // draw radiobutton circle
            using (Pen pen = new Pen(DrawHelper.BlendColor(Parent.BackColor, Enabled ? SkinManager.CheckboxOffColor : SkinManager.CheckBoxOffDisabledColor, backgroundAlpha), 2))
            {
                g.DrawEllipse(pen, new Rectangle(x, y, width, height));
            }

            if (Enabled)
            {
                using (Pen pen = new Pen(RadioColor, 2))
                {
                    g.DrawEllipse(pen, new Rectangle(x, y, width, height));
                }
            }

            if (Checked)
            {
                using (SolidBrush brush = new SolidBrush(RadioColor))
                {
                    g.FillEllipse(brush, new RectangleF(RADIOBUTTON_CENTER - animationSizeHalf, RADIOBUTTON_CENTER - animationSizeHalf, animationSize, animationSize));
                }
            }

            // Text
            using (NativeTextRenderer NativeText = new NativeTextRenderer(g))
            {
                var font = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), Font.SizeInPoints, Font.Style, GraphicsUnit.Point);
                Rectangle textLocation = new Rectangle(_boxOffset + TEXT_OFFSET, 0, Width, Height);
                NativeText.DrawTransparentText(Text, font,
                    Enabled ? SkinManager.TextHighEmphasisColor : SkinManager.TextDisabledOrHintColor,
                    textLocation.Location,
                    textLocation.Size,
                    NativeTextRenderer.TextAlignFlags.Left | NativeTextRenderer.TextAlignFlags.Middle);
            }
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            if (DesignMode) return;

            MouseState = MouseState.OUT;

            GotFocus += (sender, AddingNewEventArgs) =>
            {
                if (Ripple && !hovered)
                {
                    _hoverAM.StartNewAnimation(AnimationDirection.In, new object[] { Checked });
                    hovered = true;
                }
            };

            LostFocus += (sender, args) =>
            {
                if (Ripple && hovered)
                {
                    _hoverAM.StartNewAnimation(AnimationDirection.Out, new object[] { Checked });
                    hovered = false;
                }
            };

            MouseEnter += (sender, args) =>
            {
                MouseState = MouseState.HOVER;
                //if (Ripple && !hovered)
                //{
                //    _hoverAM.StartNewAnimation(AnimationDirection.In, new object[] { Checked });
                //    hovered = true;
                //}
            };

            MouseLeave += (sender, args) =>
            {
                MouseLocation = new Point(-1, -1);
                MouseState = MouseState.OUT;
                //if (Ripple && hovered)
                //{
                //    _hoverAM.StartNewAnimation(AnimationDirection.Out, new object[] { Checked });
                //    hovered = false;
                //}
            };

            MouseDown += (sender, args) =>
            {
                MouseState = MouseState.DOWN;
                if (Ripple)
                {
                    _rippleAM.SecondaryIncrement = 0;
                    _rippleAM.StartNewAnimation(AnimationDirection.InOutIn, new object[] { Checked });
                }
            };

            KeyDown += (sender, args) =>
            {
                if (Ripple && (args.KeyCode == Keys.Space) && _rippleAM.GetAnimationCount() == 0)
                {
                    _rippleAM.SecondaryIncrement = 0;
                    _rippleAM.StartNewAnimation(AnimationDirection.InOutIn, new object[] { Checked });
                }
            };

            MouseUp += (sender, args) =>
            {
                if (Ripple)
                {
                    MouseState = MouseState.HOVER;
                    _rippleAM.SecondaryIncrement = 0.08;
                    _hoverAM.StartNewAnimation(AnimationDirection.Out, new object[] { Checked });
                    hovered = false;
                }
            };

            KeyUp += (sender, args) =>
            {
                if (Ripple && (args.KeyCode == Keys.Space))
                {
                    MouseState = MouseState.HOVER;
                    _rippleAM.SecondaryIncrement = 0.08;
                }
            };

            MouseMove += (sender, args) =>
            {
                MouseLocation = args.Location;
                Cursor = IsMouseInCheckArea() ? Cursors.Hand : Cursors.Default;
            };
        }

        protected override void OnCheckedChanged(EventArgs e)
        {
            base.OnCheckedChanged(e);

            if (Checked)
            {
                List<MaterialRadioButton> arbControls = null;
                switch (GroupNameLevel)
                {
                    case Level.Parent:
                        if (this.Parent != null)
                        {
                            arbControls = GetAll(this.Parent, typeof(MaterialRadioButton)).ToList();
                        }
                        break;
                    case Level.Form:
                        Form form = this.FindForm();
                        if (form != null)
                        {
                            arbControls = GetAll(form, typeof(MaterialRadioButton)).ToList();
                        }
                        break;
                }
                if (arbControls != null)
                {
                    foreach (Control control in arbControls)
                    {
                        if (control != this && (control as MaterialRadioButton).GroupName == this.GroupName)
                        {
                            (control as MaterialRadioButton).Checked = false;
                        }
                    }
                }
            }
        }

        protected override void OnClick(EventArgs e)
        {
            if (!Checked)
                base.OnClick(e);
        }
        #endregion

        #region Private Methods
        private bool IsMouseInCheckArea()
        {
            return ClientRectangle.Contains(MouseLocation);
        }

        private List<MaterialRadioButton> GetAll(Control control, Type type)
        {
            var controls = control.Controls.Cast<Control>();
            return controls.SelectMany(ctrl => GetAll(ctrl, type)).Concat(controls).Where(c => c.GetType() == type).Cast<MaterialRadioButton>().ToList();
        }
        #endregion
        #region EventHandler
        private void OnSizeChanged(object sender, EventArgs eventArgs)
        {
            _boxOffset = Height / 2 - (int)(RADIOBUTTON_SIZE / 2);
            _radioButtonBounds = new Rectangle(_boxOffset, _boxOffset, RADIOBUTTON_SIZE, RADIOBUTTON_SIZE);
        }
        #endregion
    }
}