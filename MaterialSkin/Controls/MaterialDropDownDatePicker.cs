using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace MaterialSkin.Controls
{
    public partial class MaterialDropDownDatePicker : DropDownControl, INotifyPropertyChanged
    {
        #region Events
        public new event PropertyChangedEventHandler PropertyChanged;
        [Category("Action")]
        public event EventHandler DateChanged;
        #endregion

        #region Private Variables
        private MaterialDatePicker objDateControl;
        private DateTime date;
        #endregion

        #region Properties

        [Bindable(true), RefreshProperties(RefreshProperties.All)]
        [Browsable(true), DefaultValue(typeof(DateTime), "NOW")]
        public DateTime Date
        {
            get => date;
            set
            {
                date = value; objDateControl.Date = date;
                Text = date.ToShortDateString();
                DateChanged?.Invoke(this, EventArgs.Empty);
                NotifyPropertyChanged();
            }
        }
        public override Color BackColor { get => Parent == null ? SkinManager.BackdropColor : Parent.BackColor; set { } }
        #endregion

        #region Constructor
        public MaterialDropDownDatePicker()
        {
            InitializeComponent();
            objDateControl = new MaterialDatePicker();
            Date = DateTime.Now;
            objDateControl.onDateChanged += objDateControl_onDateChanged;
            InitializeDropDown(objDateControl);

            AutoSize = false;
            Size = new Size(Width, 27);
            Font = new Font(SkinManager.GetFontFamily(SkinManager.CurrentFontFamily), 8.25f, FontStyle.Regular, GraphicsUnit.Point);
        }
        #endregion

        #region Eventhandler methods
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void objDateControl_onDateChanged(DateTime newDateTime)
        {
            date = newDateTime;
            Text = newDateTime.ToShortDateString();
        }
        #endregion
    }
}
