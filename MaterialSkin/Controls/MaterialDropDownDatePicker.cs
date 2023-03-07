using System;
using System.Drawing;

namespace MaterialSkin.Controls
{
    public partial class MaterialDropDownDatePicker : DropDownControl
    {
        #region Events
        public event EventHandler DateChanged;
        #endregion

        #region Private Variables
        private MaterialDatePicker objDateControl;
        private DateTime date;
        #endregion

        #region Properties
        public DateTime Date
        {
            get => date;
            set
            {
                date = value; objDateControl.Date = date;
                Text = date.ToShortDateString();
                DateChanged?.Invoke(this, EventArgs.Empty);
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
        }
        #endregion

        #region Eventhandler methods
        private void objDateControl_onDateChanged(DateTime newDateTime)
        {
            date = newDateTime;
            Text = newDateTime.ToShortDateString();
        }
        #endregion
    }
}
