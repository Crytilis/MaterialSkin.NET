using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaterialSkin.Controls
{
    public partial class MaterialDropDownDatePicker : DropDownControl
    {
        public override Color BackColor { get => Parent == null ? SkinManager.BackdropColor : Parent.BackColor; set { } }

        private MaterialDatePicker objDateControl;
        private DateTime date;
        public DateTime Date
        {
            get => date;
            set
            {
                date = value; objDateControl.Date = date;
                Text = date.ToShortDateString();
            }
        }
        public MaterialDropDownDatePicker()
        {
            InitializeComponent();
            objDateControl = new MaterialDatePicker();
            Date = DateTime.Now;
            objDateControl.onDateChanged += objDateControl_onDateChanged;
            InitializeDropDown(objDateControl);
        }

        void objDateControl_onDateChanged(DateTime newDateTime)
        {
            date = newDateTime;
            Text = newDateTime.ToShortDateString();
        }
    }
}
