﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using MaterialSkin.Animations;

namespace MaterialSkin.Controls
{
    public class MaterialCheckbox : CheckBox, IMaterialControl
    {
        #region Public properties
        [Browsable(false)]
        public int Depth { get; set; }

        [Browsable(false)]
        public MaterialSkinManager SkinManager => MaterialSkinManager.Instance;

        [Browsable(false)]
        public MouseState MouseState { get; set; }

        [Browsable(false)]
        public Point MouseLocation { get; set; }

        private bool _ripple;

        [Category("Material Skin")]
        public bool Ripple
        {
            get { return _ripple; }
            set
            {
                _ripple = value;
                AutoSize = AutoSize; //Make AutoSize directly set the bounds.

                if (value)
                {
                    Margin = new Padding(0);
                }

                Invalidate();
            }
        }

        int checkboxSize = 12;
        [Category("Material Skin"), DefaultValue(12)]
        public int CheckboxSize { 
            get => checkboxSize;
            set
            {
                checkboxSize = value;
                Invalidate();
            }
        }

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

        [Browsable(true)]
        public bool ReadOnly { get; set; }
        #endregion

        #region Private fields
        private readonly AnimationManager _checkAM;
        private readonly AnimationManager _rippleAM;
        private readonly AnimationManager _hoverAM;
        private const int HEIGHT_RIPPLE = 37;
        private const int HEIGHT_NO_RIPPLE = 20;
        private const int TEXT_OFFSET = 26;
        private const int CHECKBOX_SIZE = 18;
        private const int CHECKBOX_SIZE_HALF = CHECKBOX_SIZE / 2;
        private int _boxOffset;
        private Point[] CheckmarkLine = { new Point(3, 8), new Point(7, 12), new Point(14, 5) };
        private bool hovered = false;
        private CheckState _oldCheckState;
        #endregion

        #region Constructor
        public MaterialCheckbox()
        {
            _checkAM = new AnimationManager
            {
                AnimationType = AnimationType.EaseInOut,
                Increment = 0.05
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
            CheckedChanged += (sender, args) =>
            {
                if (Ripple)
                    _checkAM.StartNewAnimation(Checked ? AnimationDirection.In : AnimationDirection.Out);
            };
            _checkAM.OnAnimationProgress += sender => Invalidate();
            _hoverAM.OnAnimationProgress += sender => Invalidate();
            _rippleAM.OnAnimationProgress += sender => Invalidate();

            Ripple = true;
            Height = HEIGHT_RIPPLE;
            MouseLocation = new Point(-1, -1);
        }
        #endregion

        #region Overridden events
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            var offset = Ripple ? HEIGHT_RIPPLE : HEIGHT_NO_RIPPLE;

            _boxOffset = offset / 2 - 9;
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            var baseSize = base.GetPreferredSize(proposedSize);

            Size strSize;

            using (NativeTextRenderer NativeText = new NativeTextRenderer(CreateGraphics()))
            {
                strSize = NativeText.MeasureLogString(Text, SkinManager.GetLogFontByType(MaterialSkinManager.FontType.Body1));
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

            var width = checkboxSize;
            var height = checkboxSize;

            var x = width / 2;
            var y = (ClientRectangle.Height / 2) - (height / 2);

            if (CheckAlign == ContentAlignment.MiddleRight)
            {
                x = ClientRectangle.Width - width - 2;
            }


            //int CHECKBOX_CENTER = _boxOffset + CHECKBOX_SIZE_HALF - 1;
            int CHECKBOX_CENTER = x + (width / 2) - 1;
            int CHECKBOX_MIDDLE = y + (height / 2) - 1;
            Point animationSource = new Point(CHECKBOX_CENTER, CHECKBOX_MIDDLE);
            double animationProgress = _checkAM.GetProgress();
            Rectangle checkMarkLineFill = new Rectangle(x, y, (int)(width * animationProgress), height);

            int colorAlpha = Enabled ? (int)(animationProgress * 255.0) : SkinManager.CheckBoxOffDisabledColor.A;
            int backgroundAlpha = Enabled ? (int)(SkinManager.CheckboxOffColor.A * (1.0 - animationProgress)) : SkinManager.CheckBoxOffDisabledColor.A;
            int rippleHeight = ((HEIGHT_RIPPLE - Padding.Right + Padding.Left) % 2 == 0) ? (HEIGHT_RIPPLE - Padding.Right + Padding.Left) - 3 : (HEIGHT_RIPPLE - Padding.Right + Padding.Left) - 2;

            SolidBrush brush = new SolidBrush(Color.FromArgb(colorAlpha, Enabled ? SkinManager.ColorScheme.AccentColor : SkinManager.CheckBoxOffDisabledColor));
            Pen pen = new Pen(brush.Color, 2);

            // draw hover animation
            if (Ripple)
            {
                double animationValue = _hoverAM.IsAnimating() ? _hoverAM.GetProgress() : hovered ? 1 : 0;
                int rippleWidth = (int)((HEIGHT_RIPPLE - Padding.Right - Padding.Left) * (0.7 + (0.3 * animationValue)));
                rippleHeight = (int)((HEIGHT_RIPPLE - Padding.Top - Padding.Bottom) * (0.7 + (0.3 * animationValue)));
                int rippleSize = (int)(rippleHeight * (0.7 + (0.3 * animationValue)));

                using (SolidBrush rippleBrush = new SolidBrush(Color.FromArgb((int)(40 * animationValue),
                    !Checked ? (SkinManager.Theme == MaterialSkinManager.Themes.Light ? Color.Black : Color.White) : brush.Color))) // no animation
                {
                    g.FillEllipse(rippleBrush, new Rectangle(animationSource.X - rippleWidth / 2, animationSource.Y - rippleHeight / 2, rippleSize, rippleSize));
                }
            }

            // draw ripple animation
            if (Ripple && _rippleAM.IsAnimating())
            {
                for (int i = 0; i < _rippleAM.GetAnimationCount(); i++)
                {
                    double animationValue = _rippleAM.GetProgress(i);
                    int rippleSize = (_rippleAM.GetDirection(i) == AnimationDirection.InOutIn) ? (int)(rippleHeight * (0.7 + (0.3 * animationValue))) : rippleHeight;

                    using (SolidBrush rippleBrush = new SolidBrush(Color.FromArgb((int)((animationValue * 40)), !Checked ? (SkinManager.Theme == MaterialSkinManager.Themes.Light ? Color.Black : Color.White) : brush.Color)))
                    {
                        g.FillEllipse(rippleBrush, new Rectangle(animationSource.X - rippleSize / 2, animationSource.Y - rippleSize / 2, rippleSize, rippleSize));
                    }
                }
            }

            using (GraphicsPath checkmarkPath = DrawHelper.CreateRoundRect(x, y, width, height, 1))
            {
                if (Enabled)
                {
                    using (Pen pen2 = new Pen(DrawHelper.BlendColor(Parent.BackColor, Enabled ? SkinManager.CheckboxOffColor : SkinManager.CheckBoxOffDisabledColor, backgroundAlpha), 2))
                    {
                        g.DrawPath(pen2, checkmarkPath);
                    }

                    g.DrawPath(pen, checkmarkPath);
                    g.FillPath(brush, checkmarkPath);
                }
                else
                {
                    if (Checked)
                        g.FillPath(brush, checkmarkPath);
                    else
                        g.DrawPath(pen, checkmarkPath);
                }

                g.DrawImage(DrawCheckMarkBitmap(), checkMarkLineFill);
            }

            // draw checkbox text
            using (NativeTextRenderer NativeText = new NativeTextRenderer(g))
            {
                var textSize = NativeText.MeasureString(Text, Font);
                var textWidth = _boxOffset + TEXT_OFFSET;
                var textX = TEXT_OFFSET + Padding.Right + Padding.Left;

                if (CheckAlign == ContentAlignment.MiddleRight || CheckAlign == ContentAlignment.TopRight || CheckAlign == ContentAlignment.BottomRight)
                {
                    textX = 3 + Padding.Left;
                    textWidth = TEXT_OFFSET;
                }

                Rectangle textLocation = new Rectangle(textX, 0, ClientRectangle.Width - textWidth, ClientRectangle.Height);
                NativeText.DrawMultilineTransparentText(Text, Font,
                    Enabled ? SkinManager.TextHighEmphasisColor : SkinManager.TextDisabledOrHintColor,
                    textLocation.Location,
                    textLocation.Size,
                    GetTextAlign());
            }

            // dispose used paint objects
            pen.Dispose();
            brush.Dispose();
        }

        //public override bool AutoSize
        //{
        //    get { return base.AutoSize; }
        //    set
        //    {
        //        base.AutoSize = value;
        //        if (value)
        //        {
        //            Size = new Size(10, 10);
        //        }
        //    }
        //}

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            AutoSize = false;
            Size = new Size(Width, 18);

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
                _oldCheckState = CheckState;
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
                if (ReadOnly) CheckState = _oldCheckState;
            };

            KeyDown += (sender, args) =>
            {
                if (Ripple && (args.KeyCode == Keys.Space) && _rippleAM.GetAnimationCount() == 0)
                {
                    _rippleAM.SecondaryIncrement = 0;
                    _rippleAM.StartNewAnimation(AnimationDirection.InOutIn, new object[] { Checked });
                }
                if (ReadOnly) CheckState = _oldCheckState;
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
                if (ReadOnly) CheckState = _oldCheckState;
            };

            KeyUp += (sender, args) =>
            {
                if (Ripple && (args.KeyCode == Keys.Space))
                {
                    MouseState = MouseState.HOVER;
                    _rippleAM.SecondaryIncrement = 0.08;
                }
                if (ReadOnly) CheckState = _oldCheckState;
            };

            MouseMove += (sender, args) =>
            {
                MouseLocation = args.Location;
                Cursor = IsMouseInCheckArea() ? Cursors.Hand : Cursors.Default;
            };
        }
        #endregion

        #region Private events and methods
        private Bitmap DrawCheckMarkBitmap()
        {
            Bitmap checkMark = new Bitmap(CHECKBOX_SIZE, CHECKBOX_SIZE);
            Graphics g = Graphics.FromImage(checkMark);

            // clear everything, transparent
            g.Clear(Color.Transparent);

            // draw the checkmark lines
            using (Pen pen = new Pen(Parent.BackColor, 2))
            {
                g.DrawLines(pen, CheckmarkLine);
            }

            return checkMark;
        }

        private bool IsMouseInCheckArea()
        {
            return ClientRectangle.Contains(MouseLocation);
        }

        private NativeTextRenderer.TextAlignFlags GetTextAlign()
        {
            switch (TextAlign)
            {
                case ContentAlignment.TopLeft:
                    return NativeTextRenderer.TextAlignFlags.Top | NativeTextRenderer.TextAlignFlags.Left;
                case ContentAlignment.TopCenter:
                    return NativeTextRenderer.TextAlignFlags.Top | NativeTextRenderer.TextAlignFlags.Center;
                case ContentAlignment.TopRight:
                    return NativeTextRenderer.TextAlignFlags.Top | NativeTextRenderer.TextAlignFlags.Right;
                case ContentAlignment.MiddleLeft:
                    return NativeTextRenderer.TextAlignFlags.Middle | NativeTextRenderer.TextAlignFlags.Left;
                case ContentAlignment.MiddleCenter:
                    return NativeTextRenderer.TextAlignFlags.Middle | NativeTextRenderer.TextAlignFlags.Center;
                case ContentAlignment.MiddleRight:
                    return NativeTextRenderer.TextAlignFlags.Middle | NativeTextRenderer.TextAlignFlags.Right;
                case ContentAlignment.BottomLeft:
                    return NativeTextRenderer.TextAlignFlags.Bottom | NativeTextRenderer.TextAlignFlags.Left;
                case ContentAlignment.BottomCenter:
                    return NativeTextRenderer.TextAlignFlags.Bottom | NativeTextRenderer.TextAlignFlags.Bottom;
                case ContentAlignment.BottomRight:
                    return NativeTextRenderer.TextAlignFlags.Bottom | NativeTextRenderer.TextAlignFlags.Right;
                default:
                    return NativeTextRenderer.TextAlignFlags.Top | NativeTextRenderer.TextAlignFlags.Left;
            }
        }
        #endregion
    }
}