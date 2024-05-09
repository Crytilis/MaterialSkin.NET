using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MaterialSkin.Controls
{
    public class MaterialCard : Panel, IMaterialControl
    {
        private bool drawShadows;
        private bool useCustomBackgroundColor;
        private int radius;

        [Category("Material Skin"), DefaultValue(false), Description("Use Control colors set by user"), DisplayName("UseCustomBackgroundColor")]
        public bool _UseCustomBackgroundColor
        {
            get => useCustomBackgroundColor;
            set { useCustomBackgroundColor = value; Invalidate(); }
        }

        [Category("Material Skin"), DefaultValue(true), Description("Draw Shadows around control")]
        public bool DrawShadows
        {
            get => drawShadows;
            set { drawShadows = value; Invalidate(); }
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

        [Browsable(false)]
        public int Depth { get; set; }

        [Browsable(false)]
        public MaterialSkinManager SkinManager => MaterialSkinManager.Instance;

        [Browsable(false)]
        public MouseState MouseState { get; set; }

        public MaterialCard()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            DrawShadows = true;
            Paint += new PaintEventHandler(paintControl);
            BackColor = SkinManager.BackgroundColor;
            ForeColor = SkinManager.TextHighEmphasisColor;
            Margin = new Padding(MaterialSkinManager.FORM_PADDING);
            Padding = new Padding(MaterialSkinManager.FORM_PADDING);

            Radius = radius <= 0 ? 4 : radius;
        }

        private void drawShadowOnParent(object sender, PaintEventArgs e)
        {
            if (Parent == null)
            {
                RemoveShadowPaintEvent((Control)sender, drawShadowOnParent);
                return;
            }

            if (!DrawShadows || Parent == null) return;

            // paint shadow on parent
            Graphics gp = e.Graphics;
            Rectangle rect = new Rectangle(Location, ClientRectangle.Size);
            gp.SmoothingMode = SmoothingMode.AntiAlias;
            DrawHelper.DrawSquareShadow(gp, rect, Radius);
        }

        protected override void InitLayout()
        {
            base.InitLayout();
            Invalidate();
            LocationChanged += (sender, e) => { if (DrawShadows) Parent?.Invalidate(); };
            ForeColor = SkinManager.TextHighEmphasisColor;
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

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            if(!_UseCustomBackgroundColor)
            {
                BackColor = SkinManager.BackgroundColor;
            }
        }

        private void paintControl(Object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            g.Clear(Parent.BackColor);

            // card rectangle path
            RectangleF cardRectF = new RectangleF(ClientRectangle.Location, ClientRectangle.Size);
            cardRectF.X -= 0.5f;
            cardRectF.Y -= 0.5f;
            GraphicsPath cardPath = DrawHelper.CreateRoundRect(cardRectF, Radius);

            // button shadow (blend with form shadow)
            DrawHelper.DrawSquareShadow(g, ClientRectangle, Radius);
            
            // Draw card
            using (SolidBrush normalBrush = new SolidBrush(BackColor))
            {
                g.FillPath(normalBrush, cardPath);
            }
        }
    }
}