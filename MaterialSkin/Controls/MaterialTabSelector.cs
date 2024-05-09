using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace MaterialSkin.Controls
{
    using static MaterialBaseTabSelector;

    public class MaterialTabSelector : Control, IMaterialControl
    {
        [Browsable(false)]
        public int Depth { get; set; }

        [Browsable(false)]
        public MaterialSkinManager SkinManager => MaterialSkinManager.Instance;

        [Browsable(false)]
        public MouseState MouseState { get; set; }

        private TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
        private Size originalSize;
        private Point originalLocation;
        private bool isMouseWheelAction = false;

        private MaterialTabControl _baseTabControl;
        private MaterialBaseTabSelector baseTabSelector;

        [Category("Appearance"), Localizable(true)]
        public override Font Font
        {
            get => baseTabSelector.Font;
            set
            {
                var font = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), value.SizeInPoints, value.Style, GraphicsUnit.Point);
                baseTabSelector.Font = font;
                Invalidate();
            }
        }

        [Category("Material Skin"), Browsable(true)]
        public MaterialTabControl BaseTabControl
        {
            get => baseTabSelector.BaseTabControl;
            set { baseTabSelector.BaseTabControl = value; }
        }

        [Category("Material Skin"), DefaultValue(60), Browsable(true)]
        public int TabWidthMin {
            get => baseTabSelector.TabWidthMin;
            set
            {
                baseTabSelector.TabWidthMin = value;
                Invalidate();
            }
        }
        [Category("Material Skin"), DefaultValue(264), Browsable(true)]
        public int TabWidthMax
        {
            get => baseTabSelector.TabWidthMax;
            set
            {
                baseTabSelector.TabWidthMax = value;
                Invalidate();
            }
        }

        [Category("Material Skin"), DefaultValue(6), Browsable(true)]
        public int TabHeaderPadding
        {
            get => baseTabSelector.TabHeaderPadding;
            set
            {
                baseTabSelector.TabHeaderPadding = value;
                Invalidate();
            }
        }

        [Category("Material Skin"), DefaultValue(10), Browsable(true)]
        public int FirstTabPadding
        {
            get => baseTabSelector.FirstTabPadding;
            set
            {
                baseTabSelector.FirstTabPadding = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        public CustomCharacterCasing CharacterCasing
        {
            get => baseTabSelector.CharacterCasing;
            set
            {
                baseTabSelector.CharacterCasing = value;
                Invalidate();
            }
        }

        [Category("Material Skin"), Browsable(true), DisplayName("Tab Indicator Height"), DefaultValue(2)]
        public int TabIndicatorHeight 
        {
            get { return baseTabSelector.TabIndicatorHeight; }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("Tab Indicator Height", value, "Value should be > 0");
                else
                {
                    baseTabSelector.TabIndicatorHeight = value;
                    Refresh();
                }
            }
        }

        [Category("Material Skin"), Browsable(true), DisplayName("Tab Label"), DefaultValue(TabLabelStyle.Text)]
        public TabLabelStyle TabLabel
        {
            get { return baseTabSelector.TabLabel; }
            set
            {
                baseTabSelector.TabLabel = value;
                Invalidate();
            }
        }

        # region Forwarding events to baseTextBox

        public new event EventHandler AutoSizeChanged
        {
            add
            {
                baseTabSelector.AutoSizeChanged += value;
            }
            remove
            {
                baseTabSelector.AutoSizeChanged -= value;
            }
        }

        public new event EventHandler BackgroundImageChanged
        {
            add
            {
                baseTabSelector.BackgroundImageChanged += value;
            }
            remove
            {
                baseTabSelector.BackgroundImageChanged -= value;
            }
        }

        public new event EventHandler BackgroundImageLayoutChanged
        {
            add
            {
                baseTabSelector.BackgroundImageLayoutChanged += value;
            }
            remove
            {
                baseTabSelector.BackgroundImageLayoutChanged -= value;
            }
        }

        public new event EventHandler BindingContextChanged
        {
            add
            {
                baseTabSelector.BindingContextChanged += value;
            }
            remove
            {
                baseTabSelector.BindingContextChanged -= value;
            }
        }

        public new event EventHandler CausesValidationChanged
        {
            add
            {
                baseTabSelector.CausesValidationChanged += value;
            }
            remove
            {
                baseTabSelector.CausesValidationChanged -= value;
            }
        }

        public new event UICuesEventHandler ChangeUICues
        {
            add
            {
                baseTabSelector.ChangeUICues += value;
            }
            remove
            {
                baseTabSelector.ChangeUICues -= value;
            }
        }

        public new event EventHandler Click
        {
            add
            {
                baseTabSelector.Click += value;
            }
            remove
            {
                baseTabSelector.Click -= value;
            }
        }

        public new event EventHandler ClientSizeChanged
        {
            add
            {
                baseTabSelector.ClientSizeChanged += value;
            }
            remove
            {
                baseTabSelector.ClientSizeChanged -= value;
            }
        }

#if NETFRAMEWORK
        public new event EventHandler ContextMenuChanged
        {
            add
            {
                baseTabSelector.ContextMenuChanged += value;
            }
            remove
            {
                baseTabSelector.ContextMenuChanged -= value;
            }
        }
#endif

        public new event EventHandler ContextMenuStripChanged
        {
            add
            {
                baseTabSelector.ContextMenuStripChanged += value;
            }
            remove
            {
                baseTabSelector.ContextMenuStripChanged -= value;
            }
        }

        public new event ControlEventHandler ControlAdded
        {
            add
            {
                baseTabSelector.ControlAdded += value;
            }
            remove
            {
                baseTabSelector.ControlAdded -= value;
            }
        }

        public new event ControlEventHandler ControlRemoved
        {
            add
            {
                baseTabSelector.ControlRemoved += value;
            }
            remove
            {
                baseTabSelector.ControlRemoved -= value;
            }
        }

        public new event EventHandler CursorChanged
        {
            add
            {
                baseTabSelector.CursorChanged += value;
            }
            remove
            {
                baseTabSelector.CursorChanged -= value;
            }
        }

        public new event EventHandler Disposed
        {
            add
            {
                baseTabSelector.Disposed += value;
            }
            remove
            {
                baseTabSelector.Disposed -= value;
            }
        }

        public new event EventHandler DockChanged
        {
            add
            {
                baseTabSelector.DockChanged += value;
            }
            remove
            {
                baseTabSelector.DockChanged -= value;
            }
        }

        public new event EventHandler DoubleClick
        {
            add
            {
                baseTabSelector.DoubleClick += value;
            }
            remove
            {
                baseTabSelector.DoubleClick -= value;
            }
        }

        public new event DragEventHandler DragDrop
        {
            add
            {
                baseTabSelector.DragDrop += value;
            }
            remove
            {
                baseTabSelector.DragDrop -= value;
            }
        }

        public new event DragEventHandler DragEnter
        {
            add
            {
                baseTabSelector.DragEnter += value;
            }
            remove
            {
                baseTabSelector.DragEnter -= value;
            }
        }

        public new event EventHandler DragLeave
        {
            add
            {
                baseTabSelector.DragLeave += value;
            }
            remove
            {
                baseTabSelector.DragLeave -= value;
            }
        }

        public new event DragEventHandler DragOver
        {
            add
            {
                baseTabSelector.DragOver += value;
            }
            remove
            {
                baseTabSelector.DragOver -= value;
            }
        }

        public new event EventHandler EnabledChanged
        {
            add
            {
                baseTabSelector.EnabledChanged += value;
            }
            remove
            {
                baseTabSelector.EnabledChanged -= value;
            }
        }

        public new event EventHandler Enter
        {
            add
            {
                baseTabSelector.Enter += value;
            }
            remove
            {
                baseTabSelector.Enter -= value;
            }
        }

        public new event EventHandler FontChanged
        {
            add
            {
                baseTabSelector.FontChanged += value;
            }
            remove
            {
                baseTabSelector.FontChanged -= value;
            }
        }

        public new event EventHandler ForeColorChanged
        {
            add
            {
                baseTabSelector.ForeColorChanged += value;
            }
            remove
            {
                baseTabSelector.ForeColorChanged -= value;
            }
        }

        public new event GiveFeedbackEventHandler GiveFeedback
        {
            add
            {
                baseTabSelector.GiveFeedback += value;
            }
            remove
            {
                baseTabSelector.GiveFeedback -= value;
            }
        }

        public new event EventHandler GotFocus
        {
            add
            {
                baseTabSelector.GotFocus += value;
            }
            remove
            {
                baseTabSelector.GotFocus -= value;
            }
        }

        public new event EventHandler HandleCreated
        {
            add
            {
                baseTabSelector.HandleCreated += value;
            }
            remove
            {
                baseTabSelector.HandleCreated -= value;
            }
        }

        public new event EventHandler HandleDestroyed
        {
            add
            {
                baseTabSelector.HandleDestroyed += value;
            }
            remove
            {
                baseTabSelector.HandleDestroyed -= value;
            }
        }

        public new event HelpEventHandler HelpRequested
        {
            add
            {
                baseTabSelector.HelpRequested += value;
            }
            remove
            {
                baseTabSelector.HelpRequested -= value;
            }
        }

        public new event EventHandler ImeModeChanged
        {
            add
            {
                baseTabSelector.ImeModeChanged += value;
            }
            remove
            {
                baseTabSelector.ImeModeChanged -= value;
            }
        }

        public new event InvalidateEventHandler Invalidated
        {
            add
            {
                baseTabSelector.Invalidated += value;
            }
            remove
            {
                baseTabSelector.Invalidated -= value;
            }
        }

        public new event KeyEventHandler KeyDown
        {
            add
            {
                baseTabSelector.KeyDown += value;
            }
            remove
            {
                baseTabSelector.KeyDown -= value;
            }
        }

        public new event KeyPressEventHandler KeyPress
        {
            add
            {
                baseTabSelector.KeyPress += value;
            }
            remove
            {
                baseTabSelector.KeyPress -= value;
            }
        }

        public new event KeyEventHandler KeyUp
        {
            add
            {
                baseTabSelector.KeyUp += value;
            }
            remove
            {
                baseTabSelector.KeyUp -= value;
            }
        }

        public new event LayoutEventHandler Layout
        {
            add
            {
                baseTabSelector.Layout += value;
            }
            remove
            {
                baseTabSelector.Layout -= value;
            }
        }

        public new event EventHandler Leave
        {
            add
            {
                baseTabSelector.Leave += value;
            }
            remove
            {
                baseTabSelector.Leave -= value;
            }
        }

        public new event EventHandler LocationChanged
        {
            add
            {
                baseTabSelector.LocationChanged += value;
            }
            remove
            {
                baseTabSelector.LocationChanged -= value;
            }
        }

        public new event EventHandler LostFocus
        {
            add
            {
                baseTabSelector.LostFocus += value;
            }
            remove
            {
                baseTabSelector.LostFocus -= value;
            }
        }

        public new event EventHandler MarginChanged
        {
            add
            {
                baseTabSelector.MarginChanged += value;
            }
            remove
            {
                baseTabSelector.MarginChanged -= value;
            }
        }

        public new event EventHandler MouseCaptureChanged
        {
            add
            {
                baseTabSelector.MouseCaptureChanged += value;
            }
            remove
            {
                baseTabSelector.MouseCaptureChanged -= value;
            }
        }

        public new event MouseEventHandler MouseClick
        {
            add
            {
                baseTabSelector.MouseClick += value;
            }
            remove
            {
                baseTabSelector.MouseClick -= value;
            }
        }

        public new event MouseEventHandler MouseDoubleClick
        {
            add
            {
                baseTabSelector.MouseDoubleClick += value;
            }
            remove
            {
                baseTabSelector.MouseDoubleClick -= value;
            }
        }

        public new event MouseEventHandler MouseDown
        {
            add
            {
                baseTabSelector.MouseDown += value;
            }
            remove
            {
                baseTabSelector.MouseDown -= value;
            }
        }

        public new event EventHandler MouseEnter
        {
            add
            {
                baseTabSelector.MouseEnter += value;
            }
            remove
            {
                baseTabSelector.MouseEnter -= value;
            }
        }

        public new event EventHandler MouseHover
        {
            add
            {
                baseTabSelector.MouseHover += value;
            }
            remove
            {
                baseTabSelector.MouseHover -= value;
            }
        }

        public new event EventHandler MouseLeave
        {
            add
            {
                baseTabSelector.MouseLeave += value;
            }
            remove
            {
                baseTabSelector.MouseLeave -= value;
            }
        }

        public new event MouseEventHandler MouseMove
        {
            add
            {
                baseTabSelector.MouseMove += value;
            }
            remove
            {
                baseTabSelector.MouseMove -= value;
            }
        }

        public new event MouseEventHandler MouseUp
        {
            add
            {
                baseTabSelector.MouseUp += value;
            }
            remove
            {
                baseTabSelector.MouseUp -= value;
            }
        }

        public new event MouseEventHandler MouseWheel
        {
            add
            {
                baseTabSelector.MouseWheel += value;
            }
            remove
            {
                baseTabSelector.MouseWheel -= value;
            }
        }

        public new event EventHandler Move
        {
            add
            {
                baseTabSelector.Move += value;
            }
            remove
            {
                baseTabSelector.Move -= value;
            }
        }

        public new event EventHandler PaddingChanged
        {
            add
            {
                baseTabSelector.PaddingChanged += value;
            }
            remove
            {
                baseTabSelector.PaddingChanged -= value;
            }
        }

        public new event PaintEventHandler Paint
        {
            add
            {
                baseTabSelector.Paint += value;
            }
            remove
            {
                baseTabSelector.Paint -= value;
            }
        }

        public new event EventHandler ParentChanged
        {
            add
            {
                baseTabSelector.ParentChanged += value;
            }
            remove
            {
                baseTabSelector.ParentChanged -= value;
            }
        }

        public new event PreviewKeyDownEventHandler PreviewKeyDown
        {
            add
            {
                baseTabSelector.PreviewKeyDown += value;
            }
            remove
            {
                baseTabSelector.PreviewKeyDown -= value;
            }
        }

        public new event EventHandler RegionChanged
        {
            add
            {
                baseTabSelector.RegionChanged += value;
            }
            remove
            {
                baseTabSelector.RegionChanged -= value;
            }
        }

        public new event EventHandler Resize
        {
            add
            {
                baseTabSelector.Resize += value;
            }
            remove
            {
                baseTabSelector.Resize -= value;
            }
        }

        public new event EventHandler SizeChanged
        {
            add
            {
                baseTabSelector.SizeChanged += value;
            }
            remove
            {
                baseTabSelector.SizeChanged -= value;
            }
        }

        public new event EventHandler TabIndexChanged
        {
            add
            {
                baseTabSelector.TabIndexChanged += value;
            }
            remove
            {
                baseTabSelector.TabIndexChanged -= value;
            }
        }

        public new event EventHandler TabStopChanged
        {
            add
            {
                baseTabSelector.TabStopChanged += value;
            }
            remove
            {
                baseTabSelector.TabStopChanged -= value;
            }
        }

        public new event EventHandler Validated
        {
            add
            {
                baseTabSelector.Validated += value;
            }
            remove
            {
                baseTabSelector.Validated -= value;
            }
        }

        public new event CancelEventHandler Validating
        {
            add
            {
                baseTabSelector.Validating += value;
            }
            remove
            {
                baseTabSelector.Validating -= value;
            }
        }

        public new event EventHandler VisibleChanged
        {
            add
            {
                baseTabSelector.VisibleChanged += value;
            }
            remove
            {
                baseTabSelector.VisibleChanged -= value;
            }
        }
        # endregion


        public MaterialTabSelector()
        {
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer, true);
            baseTabSelector = new MaterialBaseTabSelector();
            Font = SkinManager.GetFontByType(MaterialSkinManager.FontType.Body1);
            TabIndicatorHeight = 2;
            TabLabel = TabLabelStyle.Text;

            Size = new Size(480, 48);
            //baseTabSelector.Location = new Point(0, 0);
            //baseTabSelector.Dock = DockStyle.Fill;
            baseTabSelector.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            Controls.Add(baseTabSelector);
            originalSize = Size;
            originalLocation = Location;

            Size = new Size(480, 30);
            FirstTabPadding = 10;
            TabWidthMin = 60;
            TabHeaderPadding = 6;
        }

        public void UpdateTabRect()
        {
            baseTabSelector.UpdateTabRects();
        }
    }
}
