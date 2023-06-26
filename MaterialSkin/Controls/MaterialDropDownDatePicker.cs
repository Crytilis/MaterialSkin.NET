using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ScrollBar;
using System.Threading;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace MaterialSkin.Controls
{
    public partial class MaterialDropDownDatePicker : DropDownControl, INotifyPropertyChanged
    {
        #region Events
        public new event PropertyChangedEventHandler PropertyChanged;
        [Category("Action")]
        public event MaterialDatePicker.DateChangedHandler DateChanged
        {
            add { objDateControl.DateChanged += value; }
            remove { objDateControl.DateChanged -= value; }
        }
        #endregion

        #region Private Variables
        private MaterialDatePicker objDateControl;
        private DateTime date;

        private bool showCheckbox = false;
        #endregion

        #region Properties

        [Category("Appearance"), DefaultValue(null), Localizable(true)]
        public bool ShowCheckBox
        {
            get => showCheckbox;
            set
            {
                showCheckbox = value;
                Invalidate();
            }
        }

        [Category("Appearance"), Localizable(true)]
        public override string Text
        {
            get => objDateControl?.Text ?? base.Text;
            set
            {
                base.Text = value;
                if (objDateControl != null)
                {
                    objDateControl.Text = value;
                }
                this.Invalidate();
            }
        }

        [Bindable(true), RefreshProperties(RefreshProperties.All)]
        [Browsable(true), DefaultValue(typeof(DateTime), "NOW")]
        public DateTime Date
        {
            get => date;
            set
            {
                date = value; objDateControl.Date = date;
                Text = objDateControl.Text;
                NotifyPropertyChanged();
            }
        }
        public override Color BackColor { get => Parent == null ? SkinManager.BackdropColor : Parent.BackColor; set { } }

        #region MaterialSkin
        [Category("MaterialSkin"), DefaultValue(null), Localizable(true)]
        public string CustomFormat
        {
            get
            {
                return objDateControl.CustomFormat;
            }
            set
            {
                if ((value != null && !value.Equals(objDateControl.CustomFormat)) || (value == null && objDateControl.CustomFormat != null))
                {
                    objDateControl.CustomFormat = value;
                    if (base.IsHandleCreated && objDateControl.Format == DateTimePickerFormat.Custom)
                    {
                    }
                }
                Invalidate();
            }
        }

        [Category("MaterialSkin"), DefaultValue(DateTimePickerFormat.Long), Localizable(true)]
        public DateTimePickerFormat Format
        {
            get
            {
                return objDateControl.Format;
            }
            set
            {
                if (objDateControl.Format != value)
                {
                    objDateControl.Format = value;
                }
                Invalidate();
            }
        }

        [Category("MaterialSkin"), Localizable(true)]
        public Font DropDownTimeFont
        {
            get { return objDateControl.TimeFont; }
            set
            {
                if (value != null)
                {
                    var font = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), value.SizeInPoints, value.Style, GraphicsUnit.Point);
                    objDateControl.TimeFont = font;
                }
                Invalidate();
            }
        }


        [Category("MaterialSkin"), DefaultValue(true), Localizable(true)]
        public bool DropDownShowTime
        {
            get => objDateControl.ShowTime;
            set
            {
                objDateControl.ShowTime = value;
                Invalidate();
            }
        }

        [Category("MaterialSkin"), Localizable(true)]
        public string DropDownTimeHint
        {
            get => objDateControl.TimeHint;
            set
            {
                objDateControl.TimeHint = value;
                Invalidate();
            }
        }
        [Category("MaterialSkin"), Localizable(true)]
        public bool DropDownWideTimevox
        {
            get => objDateControl.WideTimeBox;
            set
            {
                objDateControl.WideTimeBox = value;
                Invalidate();
            }
        }
        #endregion
        #endregion

        #region Constructor
        public MaterialDropDownDatePicker()
        {
            InitializeComponent();
            objDateControl = new MaterialDatePicker();
            InitializeDropDown(objDateControl);

            Binding bindingDate = new Binding("Date", this, "Date");
            Binding bindingText = new Binding("Text", this, "Text");
            objDateControl.DataBindings.Add(bindingDate);
            objDateControl.DataBindings.Add(bindingText);
            Date = DateTime.Now;
            AutoSize = false;
            Size = new Size(Width, 27);
            Font = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), 8, FontStyle.Regular, GraphicsUnit.Point);
            DropDownTimeFont = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), 12f, FontStyle.Regular, GraphicsUnit.Point);
            Format = DateTimePickerFormat.Long;
        }
        #endregion

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }

        #region Eventhandler methods
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
