using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public event PropertyChangedEventHandler PropertyChanged;

        [Browsable(false)]
        public int Depth { get; set; }
        [Browsable(false)]
        public MaterialSkinManager SkinManager { get => MaterialSkinManager.Instance; }
        [Browsable(false)]
        public MouseState MouseState { get; set; }

        public new Color BackColor { get => SkinManager.BackgroundColor; }

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

        private MaterialMaskedTextBox hoursBox;
        private MaterialMaskedTextBox minutesBox;
        private MaterialMaskedTextBox secondsBox;

        private int _gttMasterField;

        [Bindable(true), RefreshProperties(RefreshProperties.All)]
        [Browsable(true), DefaultValue(typeof(DateTime), "01.01.2023")]
        public DateTime Date { 
            get { return currentDate; }
            set { 
                currentDate = value;

                if(onDateChanged != null)
                {
                    onDateChanged(currentDate);
                }
                NotifyPropertyChanged();

                Invalidate();
            }
        }
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

        public delegate void DateChanged(DateTime newDateTime);
        public event DateChanged onDateChanged;


        private Brush HoverBrush;

        #region Private Variables
        private Font dayFont;
        private Font topDayFont;
        private Font currentMonthFont;
        private Font selectedMonthFont;
        private Font selectedDayFont;
        private Font yearFont;
        private Font hoursFont;
        private Font minutesFont;
        private Font secondFont;

        private bool showTime;
        private bool showHours;
        private bool showMinutes;
        private bool showSeconds;
        #endregion
        #region Properties
        [Category("Appearance"), Localizable(true)]
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
        [Category("Appearance"), Localizable(true)]
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

        [Category("Appearance"), Localizable(true)]
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
        
        [Category("Appearance"), Localizable(true)]
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

        [Category("Appearance"), Localizable(true)]
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
        
        [Category("Appearance"), Localizable(true)]
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

        [Category("Appearance"), Localizable(true)]
        public Font HoursFont {
            get { return hoursFont; }
            set
            {
                if (value != null)
                {
                    var font = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), value.SizeInPoints, value.Style, GraphicsUnit.Point);
                    hoursFont = font;
                }
                Invalidate();
            }
        }

        [Category("Appearance"), Localizable(true)]
        public Font MinutesFont {
            get { return minutesFont; }
            set
            {
                if (value != null)
                {
                    var font = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), value.SizeInPoints, value.Style, GraphicsUnit.Point);
                    minutesFont = font;
                }
                Invalidate();
            }
        }

        [Category("Appearance"), Localizable(true)]
        public Font SecondsFont {
            get { return secondFont; }
            set
            {
                if (value != null)
                {
                    var font = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), value.SizeInPoints, value.Style, GraphicsUnit.Point);
                    secondFont = font;
                }
                Invalidate();
            }
        }


        [Category("Appearance"), Localizable(true)]
        public bool ShowTime
        {
            get => showTime;
            set
            {
                showTime = value;
                Invalidate();
            }
        }

        [Category("Appearance"), Localizable(true)]
        public bool ShowHours
        {
            get => showHours;
            set
            {
                showHours = value;
                Invalidate();
            }
        }

        [Category("Appearance"), Localizable(true)]
        public bool ShowMinutes
        {
            get => showMinutes;
            set
            {
                showMinutes = value;
                Invalidate();
            }
        }

        [Category("Appearance"), Localizable(true)]
        public bool ShowSeconds
        {
            get => showSeconds;
            set
            {
                showSeconds = value;
                Invalidate();
            }
        }
        #endregion


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

            DayFont = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), 10.25f, FontStyle.Regular, GraphicsUnit.Point);
            TopDayFont = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), 9.25f, FontStyle.Regular, GraphicsUnit.Point);
            CurrentMonthFont = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), 12.25f, FontStyle.Bold, GraphicsUnit.Point);
            SelectedMonthFont = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), 16.25f, FontStyle.Bold, GraphicsUnit.Point);
            SelectedDayFont = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), 40.25f, FontStyle.Bold, GraphicsUnit.Point);
            YearFont = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), 14.25f, FontStyle.Bold, GraphicsUnit.Point);
            
            DoubleBuffered = true;
            dateRectDefaultSize = (Width - kwPadding - 10) / 7;
            Date = DateTime.Now;

            hoverX = -1;
            hoverY = -1;
            CalculateRectangles();
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

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
            if (hoverX >= 0)
            {
                selectedX = hoverX;
                selectedY = hoverY;
                Date = dateRectangles[selectedX][selectedY].Date;
                return;
            }
            if (recentHovered)
            {
                Date = FirstDayOfMonth(currentDate.AddMonths(-1));
                CalculateRectangles();
                Invalidate();
                return;
            }
            if (nextHovered)
            {
                Date = FirstDayOfMonth(currentDate.AddMonths(1));
                CalculateRectangles();
                Invalidate();
                return;
            }
            base.OnMouseUp(e);
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

            g.Clear(Parent.BackColor);
            Rectangle rect = new Rectangle(Location, ClientRectangle.Size);
            DrawHelper.DrawSquareShadow(g, rect, 1);

            RectangleF dateRectF = new RectangleF(ClientRectangle.Location, ClientRectangle.Size);
            dateRectF.X -= 0.5f;
            dateRectF.Y -= 0.5f;
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

            #region Time
            if(ShowTime)
            {

            }
            #endregion

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
