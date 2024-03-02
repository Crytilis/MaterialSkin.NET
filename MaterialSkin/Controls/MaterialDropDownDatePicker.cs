using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace MaterialSkin.NET.Controls
{
    public partial class MaterialDropDownDatePicker : DropDownControl, INotifyPropertyChanged
    {
        #region Events
        public new event PropertyChangedEventHandler PropertyChanged;

        [Category("Action")]
        public event EventHandler ValueChanged;
        #endregion

        #region Private Variables
        private MaterialDatePicker objDateControl;

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

        [Bindable(true), RefreshProperties(RefreshProperties.All), Browsable(true), DefaultValue(typeof(DateTime), "NOW")]
        public DateTime Value
        {
            
            get => objDateControl.Value;
            set
            {
                var tmpdate = value < MinDate ? MinDate : (value > MaxDate ? MaxDate : value);
                objDateControl.Value = tmpdate;
                Text = objDateControl.Text;
                NotifyPropertyChanged();
            }
        }
        
        [Bindable(true), RefreshProperties(RefreshProperties.All), Browsable(true)]
        public DateTime MinDate
        {
            get => objDateControl.MinDate;
            set
            {
                if (value < DateTime.MinValue)
                {
                    throw new ArgumentOutOfRangeException("MinDate");
                }

                objDateControl.MinDate = value;
                NotifyPropertyChanged();
            }
        }

        [Bindable(true), RefreshProperties(RefreshProperties.All), Browsable(true)]
        public DateTime MaxDate
        {
            get => objDateControl.MaxDate;
            set
            {
                if (value > DateTime.MaxValue)
                {
                    throw new ArgumentOutOfRangeException("MaxDate");
                }

                objDateControl.MaxDate = value;
                NotifyPropertyChanged();
            }
        }

        public override Color BackColor { get => Parent == null ? SkinManager.BackdropColor : Parent.BackColor; set { } }

        #region MaterialSkin
        [Category("Material Skin"), DefaultValue(null), Localizable(true)]
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

        [Category("Material Skin"), DefaultValue(DateTimePickerFormat.Long), Localizable(true)]
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

        [Category("Material Skin"), Localizable(true)]
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


        [Category("Material Skin"), DefaultValue(true), Localizable(true)]
        public bool DropDownShowTime
        {
            get => objDateControl.ShowTime;
            set
            {
                objDateControl.ShowTime = value;
                Invalidate();
            }
        }

        [Category("Material Skin"), Localizable(true)]
        public string DropDownTimeHint
        {
            get => objDateControl.TimeHint;
            set
            {
                objDateControl.TimeHint = value;
                Invalidate();
            }
        }
        [Category("Material Skin")]
        public bool DropDownWideTimeBox
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

            Binding bindingValue = new Binding("Value", this, "Value");
            Binding bindingMinDate = new Binding("MinDate", this, "MinDate");
            Binding bindingMaxDate = new Binding("MaxDate", this, "MaxDate");
            Binding bindingText = new Binding("Text", this, "Text");
            objDateControl.DataBindings.Add(bindingValue);
            objDateControl.DataBindings.Add(bindingMinDate);
            objDateControl.DataBindings.Add(bindingMaxDate);
            objDateControl.DataBindings.Add(bindingText);
            Value = DateTime.Now;
            AutoSize = false;
            Size = new Size(Width, 27);
            Font = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), 8, FontStyle.Regular, GraphicsUnit.Point);
            DropDownTimeFont = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), 12f, FontStyle.Regular, GraphicsUnit.Point);
            Format = DateTimePickerFormat.Long;
            IconFont = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), 12f, FontStyle.Regular, GraphicsUnit.Point);
            IconPadding = new Padding(0,-2,35,0);
            IconToShow = "\uebcc";
            ShowIcon = true;
            UseSmallHint = true;
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
