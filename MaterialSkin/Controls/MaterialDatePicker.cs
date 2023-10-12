using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MaterialSkin.Controls
{
    [ClassInterface(ClassInterfaceType.AutoDispatch)]
    [DefaultProperty("Date")]
    [DefaultEvent("DateChanged")]
    [DefaultBindingProperty("Date")]
    public partial class MaterialDatePicker : Control, INotifyPropertyChanged
    {
        #region Delegates & Events
        public delegate void DateChangedHandler(DateTime newDateTime);


        [Category("PropertyChanged")]
        public event EventHandler FormatChanged;

        [Browsable(false)]
        public event PropertyChangedEventHandler PropertyChanged;

        public event DateChangedHandler DateChanged;

        public event EventHandler ValueChanged;
        #endregion


        #region Private Variables
        private RectangleF topDayRect;
        private RectangleF topDateRect;
        private RectangleF monthRect;
        private RectangleF dayRect;
        private RectangleF yearRect;

        private RectangleF currentCal_Header;

        private RectangleF currentCal;
        private RectangleF previousCal;
        private RectangleF nextCal;
        private GraphicsPath shadowPath;
        private DateTime currentDate;
        private DateTime mindate = DateTime.MinValue;
        private DateTime maxdate = DateTime.MaxValue;

        private MaterialMaskedTextBox timeBox;

        private int _gttMasterField;

        private List<List<DateRect>> dateRectangles;
        private List<DateRect> kwRectangles;

        private int kwPadding = 30;
        private int dateRectDefaultSize;
        private int hoverX;
        private int hoverY;
        private int selectedX;
        private int selectedY;
        private bool recentHovered;
        private bool nextHovered;

        
        private Brush HoverBrush;

        private Font dayFont;
        private Font topDayFont;
        private Font currentMonthFont;
        private Font selectedMonthFont;
        private Font selectedDayFont;
        private Font yearFont;
        private Font timeFont;

        private DateTime creationTime = DateTime.Now;
        private string customFormat;
        private DateTimePickerFormat format;

        private bool showTime;
        private bool timeboxWide;
        private bool initialized = false;
        #endregion
        #region Properties
        [Browsable(false)]
        public int Depth { get; set; }
        [Browsable(false)]
        public MaterialSkinManager SkinManager { get => MaterialSkinManager.Instance; }
        [Browsable(false)]
        public MouseState MouseState { get; set; }

        public new Color BackColor { get => SkinManager.BackgroundColor; }

        [Bindable(true), RefreshProperties(RefreshProperties.All)]
        [Category("Material Skin"), Browsable(true), DefaultValue(typeof(DateTime), "NOW")]
        public DateTime Date { 
            get { return currentDate; }
            set { 
                currentDate = value < MinDate ? MinDate : (value > MaxDate ? MaxDate : value);

                if(timeBox != null)
                {
                    timeBox.Text = currentDate.ToString("HH:mm:ss");
                }
                DateChanged?.Invoke(currentDate);
                ValueChanged?.Invoke(this, EventArgs.Empty);
                NotifyPropertyChanged();

                Invalidate();
            }
        }
        [Bindable(true), RefreshProperties(RefreshProperties.All), Category("Material Skin"), Browsable(true)]
        public DateTime MinDate
        {
            get => mindate;
            set
            {
                if (value < DateTime.MinValue)
                {
                    throw new ArgumentOutOfRangeException("MinDate");
                }

                mindate = value;
                NotifyPropertyChanged();
            }
        }

        [Bindable(true), RefreshProperties(RefreshProperties.All), Category("Material Skin"), Browsable(true)]
        public DateTime MaxDate
        {
            get => maxdate;
            set
            {
                if (value > DateTime.MaxValue)
                {
                    throw new ArgumentOutOfRangeException("MaxDate");
                }

                maxdate = value;
                NotifyPropertyChanged();
            }
        }
        [Category("Material Skin"), DefaultValue(null), Localizable(true)]
        public string CustomFormat
        {
            get
            {
                return customFormat;
            }
            set
            {
                if ((value != null && !value.Equals(customFormat)) || (value == null && customFormat != null))
                {
                    customFormat = value;
                    if (base.IsHandleCreated && format == DateTimePickerFormat.Custom)
                    {
                        Invalidate();
                    }
                }
            }
        }

        [Category("Material Skin"), DefaultValue(DateTimePickerFormat.Long), Localizable(true)]
        public DateTimePickerFormat Format
        {
            get
            {
                return format;
            }
            set
            {
                if (format != value)
                {
                    format = value;
                    //RecreateHandle();
                    Invalidate();
                    FormatChanged?.Invoke(this.Parent ?? this, EventArgs.Empty);
                }
            }
        }

        [Category("Material Skin"), DefaultValue(true), Localizable(true)]
        public bool WideTimeBox
        {
            get => timeboxWide;
            set
            {
                timeboxWide = value;
                Invalidate();
            }
        }

        public override string Text
        {
            get
            {
                switch (Format)
                {
                    case DateTimePickerFormat.Short:
                        return Date.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);
                    case DateTimePickerFormat.Time:
                        return Date.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern);
                    case DateTimePickerFormat.Custom:
                        return Date.ToString(customFormat);
                    default:
                        return Date.ToString(CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern);
                }
            }
            set
            {
                if (value == null || value.Length == 0)
                {
                    ResetValue();
                }
                else
                {
                    Date = DateTime.Parse(value, CultureInfo.CurrentCulture);
                }
            }
        }

        [Category("Material Skin"), Localizable(true)]
        public Font DayFont
        {
            get { return dayFont; }
            set
            {
                if(value != null)
                {
                    var font = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), value.SizeInPoints, value.Style, GraphicsUnit.Point);
                    dayFont = font;
                }
                Invalidate();
            }
        }
        [Category("Material Skin"), Localizable(true)]
        public Font TopDayFont
        {
            get { return topDayFont; }
            set
            {
                if (value != null)
                {
                    var font = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), value.SizeInPoints, value.Style, GraphicsUnit.Point);
                    topDayFont = font;
                }
                Invalidate();
            }
        }

        [Category("Material Skin"), Localizable(true)]
        public Font SelectedMonthFont
        {
            get { return selectedMonthFont; }
            set
            {
                if (value != null)
                {
                    var font = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), value.SizeInPoints, value.Style, GraphicsUnit.Point);
                    selectedMonthFont = font;
                }
                Invalidate();
            }
        }
        
        [Category("Material Skin"), Localizable(true)]
        public Font CurrentMonthFont
        {
            get { return currentMonthFont; }
            set
            {
                if (value != null)
                {
                    var font = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), value.SizeInPoints, value.Style, GraphicsUnit.Point);
                    currentMonthFont = font;
                }
                Invalidate();
            }
        }

        [Category("Material Skin"), Localizable(true)]
        public Font SelectedDayFont
        {
            get { return selectedDayFont; }
            set
            {
                if (value != null)
                {
                    var font = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), value.SizeInPoints, value.Style, GraphicsUnit.Point);
                    selectedDayFont = font;
                }
                Invalidate();
            }
        }
        
        [Category("Material Skin"), Localizable(true)]
        public Font YearFont
        {
            get { return yearFont; }
            set
            {
                if (value != null)
                {
                    var font = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), value.SizeInPoints, value.Style, GraphicsUnit.Point);
                    yearFont = font;
                }
                Invalidate();
            }
        }

        [Category("Material Skin"), Localizable(true)]
        public Font TimeFont {
            get { return timeFont; }
            set
            {
                if (value != null)
                {
                    var font = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), value.SizeInPoints, value.Style, GraphicsUnit.Point);
                    timeFont = font;
                    if (timeBox != null)
                    {
                        timeBox.Font = font;
                    }
                }
                Invalidate();
            }
        }


        [Category("Material Skin"), Localizable(true)]
        public bool ShowTime
        {
            get => showTime;
            set
            {
                showTime = value;
                if(timeBox != null)
                {
                    timeBox.Visible = showTime;
                }
                Invalidate();
            }
        }

        public string TimeHint
        {
            get => timeBox?.Hint ?? "";
            set
            {
                if(timeBox != null)
                {
                    timeBox.Hint = value;
                    Invalidate();
                }
            }
        }
        #endregion

        #region Constructors & Initialize

        public MaterialDatePicker()
        {
            InitializeComponent();
            Initialize();
        }

        public MaterialDatePicker(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            CreateTimeControl();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

            Width = 280;
            Height = 425;
            topDayRect = new RectangleF(0f, 0f, Width, 20f);
            topDateRect = new RectangleF(0f, topDayRect.Bottom, Width, (float)(Height * 0.3));
            monthRect = new RectangleF(0f, topDayRect.Bottom, Width, (float)(topDateRect.Height * 0.3));
            dayRect = new RectangleF(0f, monthRect.Bottom, Width, (float)(topDateRect.Height * 0.4));
            yearRect = new RectangleF(0f, dayRect.Bottom, Width, (float)(topDateRect.Height * 0.3));
            currentCal = new RectangleF(0f, topDateRect.Bottom, Width, (float)(Height * 0.75));
            currentCal_Header = new RectangleF(0f, topDateRect.Bottom + 3, Width, (float)(currentCal.Height * 0.1));
            previousCal = new RectangleF(0f, currentCal_Header.Y, currentCal_Header.Height, currentCal_Header.Height);
            nextCal = new RectangleF(Width - currentCal_Header.Height, currentCal_Header.Y, currentCal_Header.Height, currentCal_Header.Height);
            shadowPath = new GraphicsPath();
            shadowPath.AddLine(-5, topDateRect.Bottom, Width, topDateRect.Bottom);

            DayFont = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), 10f, FontStyle.Regular, GraphicsUnit.Point);
            TopDayFont = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), 9f, FontStyle.Regular, GraphicsUnit.Point);
            CurrentMonthFont = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), 12f, FontStyle.Bold, GraphicsUnit.Point);
            SelectedMonthFont = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), 16f, FontStyle.Bold, GraphicsUnit.Point);
            SelectedDayFont = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), 40f, FontStyle.Bold, GraphicsUnit.Point);
            YearFont = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), 14f, FontStyle.Bold, GraphicsUnit.Point);
            TimeFont = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), 10f, FontStyle.Regular, GraphicsUnit.Point);

            WideTimeBox = true;

            Format = DateTimePickerFormat.Long;
            
            DoubleBuffered = true;
            dateRectDefaultSize = (Width - kwPadding - 10) / 7;
            Date = DateTime.Now;

            hoverX = -1;
            hoverY = -1;
            CalculateRectangles();

        }

        private void CreateTimeControl()
        {
            timeBox = new MaterialMaskedTextBox() {
                Font = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), 12, FontStyle.Regular, GraphicsUnit.Point),
                HintFont = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), 7, FontStyle.Regular, GraphicsUnit.Point),
                HintPadding = new Padding(10, -1, 0, 0),
                Hint = "Zeit",
                Width = 90,
                //Size = new Size(90, 36),
                Size = new Size(ClientSize.Width, 36),
                Location = new Point(0, ClientSize.Height - 36),
                TabStop = true,
                Visible = true,
                Mask = "00:00:00",
                PromptChar = '_',
                InsertKeyMode = InsertKeyMode.Overwrite,
                Text = Date.ToString("HH:mm:ss"),
                TextAlign = HorizontalAlignment.Center
            };

            this.Controls.Add(timeBox);
        }

        ~MaterialDatePicker()
        {
            timeBox.TextChanged -= TimeBox_TextChanged;
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if(!initialized && this.Visible)
            {
                initialized = true;
                timeBox.TextChanged += TimeBox_TextChanged;
            }
        }

        private void TimeBox_TextChanged(object sender, EventArgs e)
        {
            var tb = sender as BaseMaskedTextBox;
            var time = tb.Text.Split(':');
            if (time.Length <= 1)
            {
                return;
            }
            for (int i = 0; i < time.Length; i++)
            {
                var num = int.Parse(string.IsNullOrWhiteSpace(time[i]) ? "0" : time[i]);
                time[i] = (num > 59 ? 59 : num).ToString();
                if (i == 0)
                {
                    time[i] = (num > 23 ? 23 : num).ToString();
                }
                else if (num < 10)
                {
                    time[i] = $"{num:D2}";
                }
            }

            tb.Text = string.Join(":", time);

            var tmpDate = $"{currentDate:d} {tb.Text}";
            Date = DateTime.Parse(tmpDate);
        }
        #endregion

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Event Invokes
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            for (int i = 1; i < 7; i++)
            {
                dateRectangles.Add(new List<DateRect>());
                for (int j = 0; j < 7; j++)
                {
                    if (dateRectangles[i][j].Drawn)
                    {
                        if (dateRectangles[i][j].Rect.Contains(e.Location))
                        {
                            if (hoverX != i || hoverY != j)
                            {
                                hoverX = i;
                                recentHovered = false;
                                nextHovered = false;
                                hoverY = j;
                                Invalidate();
                            }
                            return;
                        }
                    }
                }
            }

            if (previousCal.Contains(e.Location))
            {
                recentHovered = true;
                hoverX = -1;
                Invalidate();
                return;
            }
            if (nextCal.Contains(e.Location))
            {
                nextHovered = true;
                hoverX = -1;
                Invalidate();
                return;
            }
            if (hoverX >= 0 || recentHovered || nextHovered)
            {
                hoverX = -1;
                recentHovered = false;
                nextHovered = false;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            DateTime date = DateTime.Now;

            if (hoverX >= 0)
            {
                selectedX = hoverX;
                selectedY = hoverY;
                date = dateRectangles[selectedX][selectedY].Date;
            }
            if (recentHovered)
            {
                date = FirstDayOfMonth(currentDate.AddMonths(-1));
            }
            if (nextHovered)
            {
                date = FirstDayOfMonth(currentDate.AddMonths(1));
            }

            var tmpDate = $"{date:d} {timeBox.Text}";
            Date = DateTime.Parse(tmpDate);
            CalculateRectangles();
            Invalidate();
            return;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            hoverX = -1;
            hoverY = -1;
            nextHovered = false;
            recentHovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnResize(EventArgs e)
        {
            Width = 280;
            Height = 425;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            HoverBrush = new SolidBrush(Color.FromArgb(100, SkinManager.ColorScheme.PrimaryColor));

            if (WideTimeBox)
            {
                timeBox.Size = new Size(ClientSize.Width, 36);
                timeBox.Location = new Point(0, ClientSize.Height - 36);
            }
            else
            {
                timeBox.Size = new Size(92, 36);
                timeBox.Location = new Point((ClientSize.Width / 2) - (timeBox.Width / 2), ClientSize.Height - 36);
            }

            g.Clear(Parent.BackColor);
            Rectangle rect = new Rectangle(Location, ClientRectangle.Size);
            DrawHelper.DrawSquareShadow(g, rect, 1);

            RectangleF dateRectF = new RectangleF(ClientRectangle.Location, ClientRectangle.Size);
            dateRectF.X += 1f;
            dateRectF.Width -= 2f;
            dateRectF.Y -= 1f;
            GraphicsPath datePath = DrawHelper.CreateRoundRect(dateRectF, 1);

            using (SolidBrush normalBrush = new SolidBrush(SkinManager.BackgroundColor))
            {
                g.FillPath(normalBrush, datePath);
            }

            g.FillRectangle(SkinManager.ColorScheme.DarkPrimaryBrush, topDayRect);
            g.FillRectangle(SkinManager.ColorScheme.PrimaryBrush, topDateRect);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            #region ToDay String
            using (NativeTextRenderer NativeText = new NativeTextRenderer(g))
            {
                NativeText.DrawTransparentText(currentDate.ToString("dddd"), TopDayFont,
                        SkinManager.ColorScheme.TextColor,
                        new Point((int)topDayRect.Location.X, (int)topDayRect.Location.Y),
                        new Size((int)topDayRect.Size.Width, (int)topDayRect.Size.Height),
                        NativeTextRenderer.TextAlignFlags.Center | NativeTextRenderer.TextAlignFlags.Middle);
            }
            #endregion
            #region Month String
            using (NativeTextRenderer NativeText = new NativeTextRenderer(g))
            {
                NativeText.DrawTransparentText(currentDate.ToString("MMMM"), SelectedMonthFont,
                        SkinManager.ColorScheme.TextColor,
                        new Point((int)monthRect.Location.X, (int)monthRect.Location.Y),
                        new Size((int)monthRect.Size.Width, (int)monthRect.Size.Height),
                        NativeTextRenderer.TextAlignFlags.Center | NativeTextRenderer.TextAlignFlags.Middle);
            }
            #endregion
            #region SelectedDay String
            using (NativeTextRenderer NativeText = new NativeTextRenderer(g))
            {
                NativeText.DrawTransparentText(currentDate.ToString("dd"), SelectedDayFont,
                        SkinManager.ColorScheme.TextColor,
                        new Point((int)dayRect.Location.X, (int)dayRect.Location.Y),
                        new Size((int)dayRect.Size.Width, (int)dayRect.Size.Height),
                        NativeTextRenderer.TextAlignFlags.Center | NativeTextRenderer.TextAlignFlags.Middle);
            }
            #endregion
            #region Year String
            using (NativeTextRenderer NativeText = new NativeTextRenderer(g))
            {
                NativeText.DrawTransparentText(currentDate.ToString("yyyy"), YearFont,
                        Color.FromArgb(80, SkinManager.ColorScheme.TextColor),
                        new Point((int)yearRect.Location.X, (int)yearRect.Location.Y),
                        new Size((int)yearRect.Size.Width, (int)yearRect.Size.Height),
                        NativeTextRenderer.TextAlignFlags.Center | NativeTextRenderer.TextAlignFlags.Middle);
            }
            #endregion

            #region Current Calendar header String
            using (NativeTextRenderer NativeText = new NativeTextRenderer(g))
            {
                NativeText.DrawTransparentText(currentDate.ToString("MMMM"), CurrentMonthFont,
                        SkinManager.TextMediumEmphasisColor,
                        new Point((int)currentCal_Header.Location.X, (int)currentCal_Header.Location.Y),
                        new Size((int)currentCal_Header.Size.Width, (int)currentCal_Header.Size.Height),
                        NativeTextRenderer.TextAlignFlags.Center | NativeTextRenderer.TextAlignFlags.Middle);
            }
            #endregion

            if (hoverX >= 0)
            {
                g.FillEllipse(HoverBrush, dateRectangles[hoverX][hoverY].Rect);
            }

            g.FillEllipse(SkinManager.ColorScheme.PrimaryBrush, dateRectangles[selectedX][selectedY].Rect);
            if (recentHovered) g.FillEllipse(HoverBrush, previousCal);

            if (nextHovered) g.FillEllipse(HoverBrush, nextCal);

            using (var buttonPen = new Pen(SkinManager.TextMediumEmphasisColor, 2))
            {
                g.DrawLine(buttonPen,
                        (int)(previousCal.X + previousCal.Width * 0.6),
                        (int)(previousCal.Y + previousCal.Height * 0.4),
                        (int)(previousCal.X + previousCal.Width * 0.4),
                        (int)(previousCal.Y + previousCal.Height * 0.5));

                g.DrawLine(buttonPen,
                        (int)(previousCal.X + previousCal.Width * 0.6),
                        (int)(previousCal.Y + previousCal.Height * 0.6),
                        (int)(previousCal.X + previousCal.Width * 0.4),
                        (int)(previousCal.Y + previousCal.Height * 0.5));

                g.DrawLine(buttonPen,
                       (int)(nextCal.X + nextCal.Width * 0.4),
                       (int)(nextCal.Y + nextCal.Height * 0.4),
                       (int)(nextCal.X + nextCal.Width * 0.6),
                       (int)(nextCal.Y + nextCal.Height * 0.5));

                g.DrawLine(buttonPen,
                        (int)(nextCal.X + nextCal.Width * 0.4),
                        (int)(nextCal.Y + nextCal.Height * 0.6),
                        (int)(nextCal.X + nextCal.Width * 0.6),
                        (int)(nextCal.Y + nextCal.Height * 0.5));
            }

            DateTime firstDay = FirstDayOfMonth(currentDate);
            for (int i = 0; i < 7; i++)
            {
                string strName;
                int dayOfWeek = (int)DateTime.Now.DayOfWeek - 1;
                if (dayOfWeek < 0) dayOfWeek = 6;

                strName = DateTime.Now.AddDays(-dayOfWeek + i).ToString("ddd");
                var dayNameRect = dateRectangles[0][i].Rect;

                using (NativeTextRenderer nativeText = new NativeTextRenderer(g))
                {
                    nativeText.DrawTransparentText(strName, DayFont,
                            Color.FromArgb(80, SkinManager.TextMediumEmphasisColor),
                            new Point((int)dayNameRect.Location.X, (int)dayNameRect.Location.Y),
                            new Size((int)dayNameRect.Size.Width, (int)dayNameRect.Size.Height),
                            NativeTextRenderer.TextAlignFlags.Center | NativeTextRenderer.TextAlignFlags.Middle);
                }
            }
            for (DateTime date = firstDay; date <= LastDayOfMonth(currentDate); date = date.AddDays(1))
            {
                int weekOfMonth = GetWeekNumber(date, firstDay);
                int dayOfWeek = (int)date.DayOfWeek - 1;
                if (dayOfWeek < 0) dayOfWeek = 6;

                var dayNameRect = dateRectangles[weekOfMonth][dayOfWeek].Rect;

                using (NativeTextRenderer nativeText = new NativeTextRenderer(g))
                {
                    nativeText.DrawTransparentText(date.Day.ToString(), DayFont,
                            SkinManager.TextMediumEmphasisColor,
                            new Point((int)dayNameRect.Location.X, (int)dayNameRect.Location.Y),
                            new Size((int)dayNameRect.Size.Width, (int)dayNameRect.Size.Height),
                            NativeTextRenderer.TextAlignFlags.Center | NativeTextRenderer.TextAlignFlags.Middle);
                }
            }

            for (int i = 0; i < kwRectangles.Count; i++)
            {
                if(kwRectangles[i].Date == DateTime.MinValue)
                {
                    continue;
                }

                Calendar cal = CultureInfo.CurrentCulture.Calendar;
                int week = cal.GetWeekOfYear(kwRectangles[i].Date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                var weekRect = kwRectangles[i].Rect;

                using (NativeTextRenderer nativeText = new NativeTextRenderer(g))
                {
                    nativeText.DrawTransparentText(week.ToString(), DayFont,
                            SkinManager.TextMediumEmphasisColor,
                            new Point((int)weekRect.Location.X, (int)weekRect.Location.Y),
                            new Size((int)weekRect.Size.Width, (int)weekRect.Size.Height),
                            NativeTextRenderer.TextAlignFlags.Center | NativeTextRenderer.TextAlignFlags.Middle);
                }

                using (var buttonPen = new Pen(SkinManager.ColorScheme.AccentColor, 2))
                {
                    g.DrawLine(buttonPen,
                            kwRectangles[i].Rect.Right - 4, kwRectangles[i].Rect.Top,
                            kwRectangles[i].Rect.Right - 4, kwRectangles[i].Rect.Bottom);
                }
            }
        }

        #endregion

        #region Private Methods
        private void ResetValue()
        {
            Date = DateTime.Now;
            OnTextChanged(EventArgs.Empty);
        }
        #endregion
        private void CalculateRectangles()
        {
            dateRectangles = new List<List<DateRect>>();
            kwRectangles = new List<DateRect>();
            for (int i = 0; i < 7; i++)
            {
                dateRectangles.Add(new List<DateRect>());
                for (int j = 0; j < 7; j++)
                {
                    dateRectangles[i].Add(new DateRect(new Rectangle(5 + (j * dateRectDefaultSize) + kwPadding, (int)(currentCal_Header.Bottom + (i * dateRectDefaultSize)), dateRectDefaultSize, dateRectDefaultSize)));
                }
            }

            for (int i = 0; i < 7; i++)
            {
                kwRectangles.Add(new DateRect(new Rectangle(5, (int)(currentCal_Header.Bottom + (i * dateRectDefaultSize)), dateRectDefaultSize, dateRectDefaultSize)));
            }

            DateTime FirstDay = FirstDayOfMonth(currentDate);
            for (DateTime date = FirstDay; date <= LastDayOfMonth(currentDate); date = date.AddDays(1))
            {
                int WeekOfMonth = GetWeekNumber(date, FirstDay);
                int dayOfWeek = (int)date.DayOfWeek - 1;
                if (dayOfWeek < 0) dayOfWeek = 6;
                if (date.DayOfYear == currentDate.DayOfYear && date.Year == currentDate.Year)
                {
                    selectedX = WeekOfMonth;
                    selectedY = dayOfWeek;
                }

                dateRectangles[WeekOfMonth][dayOfWeek].Drawn = true;
                dateRectangles[WeekOfMonth][dayOfWeek].Date = date;

                kwRectangles[WeekOfMonth].Drawn= true;
                kwRectangles[WeekOfMonth].Date= date;
            }

        }

        public DateTime FirstDayOfMonth(DateTime value)
        {
            return new DateTime(value.Year, value.Month, 1);
        }

        public DateTime LastDayOfMonth(DateTime value)
        {
            return new DateTime(value.Year, value.Month, DateTime.DaysInMonth(value.Year, value.Month));
        }

        public static int GetWeekNumber(DateTime CurrentDate, DateTime FirstDayOfMonth)
        {

            while (CurrentDate.Date.AddDays(1).DayOfWeek != CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek)
                CurrentDate = CurrentDate.AddDays(1);

            return (int)Math.Truncate((double)CurrentDate.Subtract(FirstDayOfMonth).TotalDays / 7f) + 1;
        }

        internal static NativeMethods.SYSTEMTIME DateTimeToSysTime(DateTime time)
        {
            NativeMethods.SYSTEMTIME sYSTEMTIME = new NativeMethods.SYSTEMTIME();
            sYSTEMTIME.wYear = (ushort)time.Year;
            sYSTEMTIME.wMonth = (ushort)time.Month;
            sYSTEMTIME.wDayOfWeek = (ushort)time.DayOfWeek;
            sYSTEMTIME.wDay = (ushort)time.Day;
            sYSTEMTIME.wHour = (ushort)time.Hour;
            sYSTEMTIME.wMinute = (ushort)time.Minute;
            sYSTEMTIME.wSecond = (ushort)time.Second;
            sYSTEMTIME.wMilliseconds = 0;
            return sYSTEMTIME;
        }

        internal static DateTime SysTimeToDateTime(NativeMethods.SYSTEMTIME s)
        {
            return new DateTime(s.wYear, s.wMonth, s.wDay, s.wHour, s.wMinute, s.wSecond);
        }

        private class DateRect
        {
            public Rectangle Rect;
            public bool Drawn = false;
            public DateTime Date;

            public DateRect(Rectangle pRect)
            {
                Rect = pRect;
            }
        }


    }
}
