using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MaterialSkin.NET.Controls
{
    [ToolboxItem(false)]
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDispatch)]
    [DefaultProperty("Value")]
    [DefaultEvent("ValueChanged")]
    [DefaultBindingProperty("Value")]
    public class NumericUpDown : UpDownBase, ISupportInitialize
    {

        private static readonly Decimal DefaultValue = Decimal.Zero;
        private static readonly Decimal DefaultMinimum = Decimal.Zero;
        private static readonly Decimal DefaultMaximum = (Decimal)100.0;
        private const int DefaultDecimalPlaces = 0;
        private static readonly Decimal DefaultIncrement = Decimal.One;
        private const bool DefaultThousandsSeparator = false;
        private const bool DefaultHexadecimal = false;
        private const int InvalidValue = -1;

        ////////////////////////////////////////////////////////////// 
        // Member variables
        // 
        //////////////////////////////////////////////////////////////

        /// 
        ///     The number of decimal places to display. 
        /// 
        private int decimalPlaces = DefaultDecimalPlaces;

        /// 
        ///     The amount to increment by. 
        /// 
        private Decimal increment = DefaultIncrement;

        // Display the thousands separator? 
        private bool thousandsSeparator = DefaultThousandsSeparator;

        // Minimum and maximum values 
        private Decimal minimum = DefaultMinimum;
        private Decimal maximum = DefaultMaximum;

        // Hexadecimal
        private bool hexadecimal = DefaultHexadecimal;

        // Internal storage of the current value
        private Decimal currentValue = DefaultValue;
        private bool currentValueChanged;

        // Event handler for the onValueChanged event 
        private EventHandler onValueChanged = null;

        // Disable value range checking while initializing the control
        private bool initializing = false;

        // Provides for finer acceleration behavior. 
        private NumericUpDownAccelerationCollection accelerations;

        // the current NumericUpDownAcceleration object. 
        private int accelerationsCurrentIndex;

        // Used to calculate the time elapsed since the up/down button was pressed,
        // to know when to get the next entry in the accelaration table. 
        private long buttonPressedStartTime;

        ///  
        /// 
        ///    [To be supplied.] 
        /// 
        [
            SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters") // "0" is the default value for numeric up down.
                                                                                                        // So we don't have to localize it. 
        ]
        public NumericUpDown() : base()
        {
            // this class overrides GetPreferredSizeCore, let Control automatically cache the result 
            Type t = this.GetType();
            BindingFlags bf = BindingFlags.Instance | BindingFlags.NonPublic;
            MethodInfo mi = t.GetMethod("SetState2", bf);
            mi.Invoke(this, new object[] { 0x00000800, true });
            Text = "0";
            StopAcceleration();
        }

        ////////////////////////////////////////////////////////////// 
        // Properties
        // 
        ////////////////////////////////////////////////////////////// 


        /// 
        ///     Specifies the acceleration information.
        /// 
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public NumericUpDownAccelerationCollection Accelerations
        {
            get
            {
                if (this.accelerations == null)
                {
                    this.accelerations = new NumericUpDownAccelerationCollection();
                }
                return this.accelerations;
            }
        }

        /// 
        ///  
        ///    Gets or sets the number of decimal places to display in the up-down control.
        /// 
        [Category("Data"), DefaultValue(NumericUpDown.DefaultDecimalPlaces)]
        public int DecimalPlaces
        {
            get
            {
                return decimalPlaces;
            }

            set
            {
                if (value < 0 || value > 99)
                {
                    throw new ArgumentOutOfRangeException("DecimalPlaces", string.Format("The given decimal places {0} is not between {1} and {2}", value.ToString(CultureInfo.CurrentCulture), (0).ToString(CultureInfo.CurrentCulture), "99"));
                }
                decimalPlaces = value;
                UpdateEditText();
            }
        }

        /// 
        ///  
        ///    Gets or 
        ///       sets a value indicating whether the up-down control should
        ///       display the value it contains in hexadecimal format. 
        /// 
        [Category("Appearance"), DefaultValue(NumericUpDown.DefaultHexadecimal)]
        public bool Hexadecimal
        {
            get
            {
                return hexadecimal;
            }

            set
            {
                hexadecimal = value;
                UpdateEditText();
            }
        }

        /// 
        /// 
        ///    Gets or sets the value
        ///       to increment or 
        ///       decrement the up-down control when the up or down buttons are clicked.
        ///  
        [Category("Data")]
        public Decimal Increment
        {
            get
            {
                if (this.accelerationsCurrentIndex != InvalidValue)
                {
                    return this.Accelerations[this.accelerationsCurrentIndex].Increment;
                }

                return this.increment;
            }
            set
            {
                if (value < (Decimal)0.0)
                {
                    throw new ArgumentOutOfRangeException("Increment", $"given value ({value.ToString(CultureInfo.CurrentCulture)}) is out of range");
                }
                else
                {
                    this.increment = value;
                }
            }
        }


        /// 
        ///  
        ///    Gets or sets the maximum value for the up-down control. 
        /// 
        [Category("Data"), RefreshProperties(RefreshProperties.All)]
        public Decimal Maximum
        {
            get
            {
                return maximum;
            }
            set
            {
                maximum = value;
                if (minimum > maximum)
                {
                    minimum = maximum;
                }

                Value = Constrain(currentValue);

                Debug.Assert(maximum == value, "Maximum != what we just set it to!");
            }
        }

        /// 
        ///  
        ///    Gets or sets the minimum allowed value for the up-down control. 
        /// 
        [Category("Data"), RefreshProperties(RefreshProperties.All)]
        public Decimal Minimum
        {
            get
            {
                return minimum;
            }
            set
            {
                minimum = value;
                if (minimum > maximum)
                {
                    maximum = value;
                }

                Value = Constrain(currentValue);

                Debug.Assert(minimum.Equals(value), "Minimum != what we just set it to!");
            }
        }

        /// 
        ///  
        ///     
        ///    [To be supplied.]
        ///     
        /// 
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Padding Padding
        {
            get { return base.Padding; }
            set { base.Padding = value; }
        }

        [Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never)]
        new public event EventHandler PaddingChanged
        {
            add { base.PaddingChanged += value; }
            remove { base.PaddingChanged -= value; }
        }

        /// 
        ///     Determines whether the UpDownButtons have been pressed for enough time to activate acceleration. 
        /// 
        private bool Spinning
        {
            get
            {
                return this.accelerations != null && this.buttonPressedStartTime != InvalidValue;
            }
        }

        /// 
        ///  
        /// 
        ///     
        ///       The text displayed in the control. 
        ///    
        ///  
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Bindable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        // We're just overriding this to make it non-browsable. 
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
            }
        }

        ///  
        /// 
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        new public event EventHandler TextChanged
        {
            add
            {
                base.TextChanged += value;
            }
            remove
            {
                base.TextChanged -= value;
            }
        }

        /// 
        /// 
        ///    Gets or sets a value indicating whether a thousands
        ///       separator is displayed in the up-down control when appropriate. 
        /// 
        [Category("Data"), DefaultValue(NumericUpDown.DefaultThousandsSeparator), Localizable(true)]
        public bool ThousandsSeparator
        {
            get
            {
                return thousandsSeparator;
            }
            set
            {
                thousandsSeparator = value;
                UpdateEditText();
            }
        }

        /* 
         * The current value of the control 
         */
        ///  
        /// 
        ///    Gets or sets the value
        ///       assigned to the up-down control.
        ///  
        [Category("Appearance"), Localizable(true), Bindable(true)]
        public Decimal Value
        {
            get
            {
                if (UserEdit)
                {
                    ValidateEditText();
                }
                return currentValue;
            }
            set
            {
                if (value != currentValue)
                {

                    if (!initializing && ((value < minimum) || (value > maximum)))
                    {
                        throw new ArgumentOutOfRangeException("Value", $"given value ({value.ToString(CultureInfo.CurrentCulture)}) is out of range of the set 'Maximum' and 'Minimum'");
                    }
                    else
                    {
                        currentValue = value;

                        OnValueChanged(EventArgs.Empty);
                        currentValueChanged = true;
                        UpdateEditText();
                    }
                }
            }
        }


        //////////////////////////////////////////////////////////////
        // Methods
        //
        ////////////////////////////////////////////////////////////// 

        ///  
        ///  
        ///    
        ///       Occurs when the  property has been changed in some way. 
        ///    
        /// 
        [Category("Action")]
        public event EventHandler ValueChanged
        {
            add
            {
                onValueChanged += value;
            }
            remove
            {
                onValueChanged -= value;
            }
        }

        ///  
        /// 
        ///  
        ///    Handles tasks required when the control is being initialized. 
        /// 
        public void BeginInit()
        {
            initializing = true;
        }

        // 
        // Returns the provided value constrained to be within the min and max.
        // 
        private Decimal Constrain(Decimal value)
        {

            Debug.Assert(minimum <= maximum,
                         "minimum > maximum");

            if (value < minimum)
            {
                value = minimum;
            }

            if (value > maximum)
            {
                value = maximum;
            }

            return value;
        }

        /// 
        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new NumericUpDownAccessibleObject(this);
        }

        /// 
        /// 
        ///    
        ///       Decrements the value of the up-down control. 
        ///    
        ///  
        public override void DownButton()
        {
            SetNextAcceleration();

            if (UserEdit)
            {
                ParseEditText();
            }

            Decimal newValue = currentValue;

            // Operations on Decimals can throw OverflowException. 
            //
            try
            {
                newValue -= this.Increment;

                if (newValue < minimum)
                {
                    newValue = minimum;
                    if (this.Spinning)
                    {
                        StopAcceleration();
                    }
                }
            }
            catch (OverflowException)
            {
                newValue = minimum;
            }

            Value = newValue;
        }

        /// 
        ///  
        /// 
        ///    
        ///       Called when initialization of the control is complete.
        ///     
        /// 
        public void EndInit()
        {
            initializing = false;
            Value = Constrain(currentValue);
            UpdateEditText();
        }

        /// 
        ///     Overridden to set/reset acceleration variables. 
        /// 
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (base.InterceptArrowKeys && (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down) && !this.Spinning)
            {
                StartAcceleration();
            }

            base.OnKeyDown(e);
        }

        /// 
        ///     Overridden to set/reset acceleration variables. 
        ///  
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (base.InterceptArrowKeys && (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down))
            {
                StopAcceleration();
            }

            base.OnKeyUp(e);
        }

        ///  
        /// 
        ///  
        ///    
        ///       Restricts the entry of characters to digits (including hex), the negative sign,
        ///       the decimal point, and editing keystrokes (backspace).
        ///     
        /// 
        protected override void OnTextBoxKeyPress(object source, KeyPressEventArgs e)
        {

            base.OnTextBoxKeyPress(source, e);

            NumberFormatInfo numberFormatInfo = System.Globalization.CultureInfo.CurrentCulture.NumberFormat;
            string decimalSeparator = numberFormatInfo.NumberDecimalSeparator;
            string groupSeparator = numberFormatInfo.NumberGroupSeparator;
            string negativeSign = numberFormatInfo.NegativeSign;

            string keyInput = e.KeyChar.ToString();

            if (Char.IsDigit(e.KeyChar))
            {
                // Digits are OK 
            }
            else if (keyInput.Equals(decimalSeparator) || keyInput.Equals(groupSeparator) ||
                     keyInput.Equals(negativeSign))
            {
                // Decimal separator is OK 
            }
            else if (e.KeyChar == '\b')
            {
                // Backspace key is OK 
            }
            else if (Hexadecimal && ((e.KeyChar >= 'a' && e.KeyChar <= 'f') || e.KeyChar >= 'A' && e.KeyChar <= 'F'))
            {
                // Hexadecimal digits are OK
            }
            else if ((ModifierKeys & (Keys.Control | Keys.Alt)) != 0)
            {
                // Let the edit control handle control and alt key combinations 
            }
            else
            {
                // Eat this invalid key and beep 
                e.Handled = true;
            }
        }

        ///  
        /// 
        /// Raises the  event. 
        ///  
        protected virtual void OnValueChanged(EventArgs e)
        {

            // Call the event handler
            if (onValueChanged != null)
            {
                onValueChanged(this, e);
            }
        }

        ///  
        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            if (UserEdit)
            {
                UpdateEditText();
            }
        }

        /// 
        ///   Overridden to start/end acceleration. 
        /// 
        internal void OnStartTimer()
        {
            StartAcceleration();
        }

        ///  
        ///   Overridden to start/end acceleration. 
        /// 
        internal void OnStopTimer()
        {
            StopAcceleration();
        }

        ///  
        /// 
        ///     
        ///       Converts the text displayed in the up-down control to a 
        ///       numeric value and evaluates it.
        ///     
        /// 
        protected void ParseEditText()
        {

            Debug.Assert(UserEdit == true, "ParseEditText() - UserEdit == false");

            try
            {
                // VSWhidbey 173332: Verify that the user is not starting the string with a "-" 
                // before attempting to set the Value property since a "-" is a valid character with
                // which to start a string representing a negative number. 
                if (!string.IsNullOrEmpty(Text) &&
                    !(Text.Length == 1 && Text == "-"))
                {
                    if (Hexadecimal)
                    {
                        Value = Constrain(Convert.ToDecimal(Convert.ToInt32(Text, 16)));
                    }
                    else
                    {
                        Value = Constrain(Decimal.Parse(Text, CultureInfo.CurrentCulture));
                    }
                }
            }
            catch
            {
                // Leave value as it is
            }
            finally
            {
                UserEdit = false;
            }
        }

        /// 
        ///     Updates the index of the UpDownNumericAcceleration entry to use (if needed).
        /// 
        private void SetNextAcceleration()
        {
            // Spinning will check if accelerations is null.
            if (this.Spinning && this.accelerationsCurrentIndex < (this.accelerations.Count - 1))
            { // if index not the last entry ... 
                // Ticks are in 100-nanoseconds (1E-7 seconds). 
                long nowTicks = DateTime.Now.Ticks;
                long buttonPressedElapsedTime = nowTicks - this.buttonPressedStartTime;
                long accelerationInterval = 10000000L * this.accelerations[this.accelerationsCurrentIndex + 1].Seconds;  // next entry.

                // If Up/Down button pressed for more than the current acceleration entry interval, get next entry in the accel table.
                if (buttonPressedElapsedTime > accelerationInterval)
                {
                    this.buttonPressedStartTime = nowTicks;
                    this.accelerationsCurrentIndex++;
                }
            }
        }

        private void ResetIncrement()
        {
            Increment = DefaultIncrement;
        }

        private void ResetMaximum()
        {
            Maximum = DefaultMaximum;
        }

        private void ResetMinimum()
        {
            Minimum = DefaultMinimum;
        }

        private void ResetValue()
        {
            Value = DefaultValue;
        }

        /// 
        /// Indicates whether the  property should be
        ///    persisted.
        ///  
        private bool ShouldSerializeIncrement()
        {
            return !Increment.Equals(NumericUpDown.DefaultIncrement);
        }

        ///  
        /// Indicates whether the  property should be persisted.
        /// 
        private bool ShouldSerializeMaximum()
        {
            return !Maximum.Equals(NumericUpDown.DefaultMaximum);
        }

        ///  
        /// Indicates whether the  property should be persisted.
        ///  
        private bool ShouldSerializeMinimum()
        {
            return !Minimum.Equals(NumericUpDown.DefaultMinimum);
        }

        /// 
        /// Indicates whether the  property should be persisted. 
        ///  
        private bool ShouldSerializeValue()
        {
            return !Value.Equals(NumericUpDown.DefaultValue);
        }


        ///  
        ///     Records when UpDownButtons are pressed to enable acceleration.
        ///  
        private void StartAcceleration()
        {
            this.buttonPressedStartTime = DateTime.Now.Ticks;
        }

        /// 
        ///     Reset when UpDownButtons are pressed.
        ///  
        private void StopAcceleration()
        {
            this.accelerationsCurrentIndex = InvalidValue;
            this.buttonPressedStartTime = InvalidValue;
        }

        /// 
        /// 
        ///     Provides some interesting info about this control in String form.
        ///  
        /// 
        public override string ToString()
        {

            string s = base.ToString();
            s += ", Minimum = " + Minimum.ToString(CultureInfo.CurrentCulture) + ", Maximum = " + Maximum.ToString(CultureInfo.CurrentCulture);
            return s;
        }

        ///  
        /// 
        ///     
        ///       Increments the value of the up-down control. 
        ///    
        ///  
        public override void UpButton()
        {
            SetNextAcceleration();

            if (UserEdit)
            {
                ParseEditText();
            }

            Decimal newValue = currentValue;

            // Operations on Decimals can throw OverflowException.
            //
            try
            {
                newValue += this.Increment;

                if (newValue > maximum)
                {
                    newValue = maximum;
                    if (this.Spinning)
                    {
                        StopAcceleration();
                    }
                }
            }
            catch (OverflowException)
            {
                newValue = maximum;
            }

            Value = newValue;
        }

        private string GetNumberText(decimal num)
        {
            string text;

            if (Hexadecimal)
            {
                text = ((Int64)num).ToString("X", CultureInfo.InvariantCulture);
                Debug.Assert(text == text.ToUpper(CultureInfo.InvariantCulture), "GetPreferredSize assumes hex digits to be uppercase.");
            }
            else
            {
                text = num.ToString((ThousandsSeparator ? "N" : "F") + DecimalPlaces.ToString(CultureInfo.CurrentCulture), CultureInfo.CurrentCulture);
            }
            return text;
        }

        ///  
        ///  
        ///    
        ///       Displays the current value of the up-down control in the appropriate format. 
        ///    
        /// 
        protected override void UpdateEditText()
        {
            if (!initializing)
            {
                if (base.UserEdit)
                {
                    ParseEditText();
                }

                if (currentValueChanged || (!string.IsNullOrEmpty(Text) && (Text.Length != 1 || !(Text == "-"))))
                {
                    currentValueChanged = false;
                    base.ChangingText = true;
                    Text = GetNumberText(currentValue);
                }
            }
        }

        ///  
        ///  
        ///    
        ///       Validates and updates 
        ///       the text displayed in the up-down control.
        ///    
        /// 
        protected override void ValidateEditText()
        {

            // See if the edit text parses to a valid decimal 
            ParseEditText();
            UpdateEditText();
        }

        // This is not a breaking change -- Even though this control previously autosized to hieght,
        // it didn't actually have an AutoSize property.  The new AutoSize property enables the
        // smarter behavior. 
        internal Size GetPreferredSizeCore(Size proposedConstraints)
        {
            int height = PreferredHeight;

            int baseSize = Hexadecimal ? 16 : 10;
            int digit = GetLargestDigit(0, baseSize);
            // The floor of log is intentionally 1 less than the number of digits.  We initialize
            // testNumber to account for the missing digit.
            int numDigits = (int)Math.Floor(Math.Log(Math.Max(-(double)Minimum, (double)Maximum), baseSize));
            decimal testNumber;

            // preinitialize testNumber with the leading digit 
            if (digit != 0 || numDigits == 1)
            {
                testNumber = digit;
            }
            else
            {
                // zero can not be the leading digit if we need more than
                // one digit.  (0*baseSize = 0 in the loop below)
                testNumber = GetLargestDigit(1, baseSize);
            }

            // e.g., if the lagest digit is 7, and we can have 3 digits, the widest string would be "777" 
            for (int i = 0; i < numDigits; i++)
            {
                testNumber = testNumber * baseSize + digit;
            }

            int textWidth = TextRenderer.MeasureText(GetNumberText(testNumber), this.Font).Width;

            Type t = this.GetType();
            BindingFlags bf = BindingFlags.Instance | BindingFlags.NonPublic;
            FieldInfo fi = t.GetField("upDownButtons", bf);
            var upDownButtons = fi.GetValue(this);
            int udButtonWidth = (int)upDownButtons.GetType().GetProperty("Width").GetValue(upDownButtons, null);

            // Call AdjuctWindowRect to add space for the borders 
            int width = SizeFromClientSize(new Size(textWidth, height)).Width + udButtonWidth;
            return new Size(width, height) + Padding.Size;
        }

        private int GetLargestDigit(int start, int end)
        {
            int largestDigit = -1;
            int digitWidth = -1;

            for (int i = start; i < end; i++)
            {
                char ch;
                if (i < 10)
                {
                    ch = i.ToString(CultureInfo.InvariantCulture)[0];
                }
                else
                {
                    ch = (char)('A' + (i - 10));
                }

                Size digitSize = TextRenderer.MeasureText(ch.ToString(), this.Font);

                if (digitSize.Width >= digitWidth)
                {
                    digitWidth = digitSize.Width;
                    largestDigit = i;
                }
            }
            Debug.Assert(largestDigit != -1 && digitWidth != -1, "Failed to find largest digit.");
            return largestDigit;
        }

        [System.Runtime.InteropServices.ComVisible(true)]
        internal class NumericUpDownAccessibleObject : ControlAccessibleObject
        {

            public NumericUpDownAccessibleObject(NumericUpDown owner) : base(owner)
            {
            }

            public override AccessibleRole Role
            {
                get
                {
                    AccessibleRole role = Owner.AccessibleRole;
                    if (role != AccessibleRole.Default)
                    {
                        return role;
                    }
                    return AccessibleRole.ComboBox;
                }
            }

            public override int GetChildCount()
            {
                return 2;
            }
        }
    }

}
