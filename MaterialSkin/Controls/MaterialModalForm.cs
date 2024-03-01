using MaterialSkin.Animations;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace MaterialSkin.Controls
{
    /// <summary>
    /// MaterialModalForm is a Form that is meant to be used a Modal Form. Call with ShowMaterialDialog() instead of ShowDialog()
    /// </summary>
    public class MaterialModalForm : MaterialForm
    {

        private readonly AnimationManager _animationManager = new AnimationManager();
        private readonly bool _closeAnimation = false;
        private Form _formOverlay;

        /// <summary>
        /// Constructor Setting up the animation manager
        /// </summary>
        public MaterialModalForm()
        {
            if (DesignMode) return;
            _animationManager.AnimationType        = AnimationType.EaseOut;
            _animationManager.Increment            = 0.03;
            _animationManager.OnAnimationProgress += _AnimationManager_OnAnimationProgress;
        }

        public DialogResult ShowMaterialDialog(Form parentForm) 
        {
            _formOverlay = new Form
            {
                BackColor = Color.Black,
                Opacity = 0.5,
                MinimizeBox = false,
                MaximizeBox = false,
                Text = "",
                ShowIcon = false,
                ControlBox = false,
                FormBorderStyle = FormBorderStyle.None,
                Size = new Size(parentForm.Width, parentForm.Height),
                ShowInTaskbar = false,
                Owner = parentForm,
                Visible = true,
                Location = new Point(parentForm.Location.X, parentForm.Location.Y),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom,
            };
            return ShowDialog(parentForm);
        }


        /// <summary>
        /// Sets up the Starting Location and starts the Animation
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (DesignMode) return;
            Location = new Point(Convert.ToInt32(Owner.Location.X + (Owner.Width / 2) - (Width / 2)), Convert.ToInt32(Owner.Location.Y + (Owner.Height / 2) - (Height / 2)));
            _animationManager.StartNewAnimation(AnimationDirection.In);
            
        }

        /// <summary>
        /// Animates the Form slides
        /// </summary>
        void _AnimationManager_OnAnimationProgress(object sender)
        {
            if (_closeAnimation)
            {
                Opacity = _animationManager.GetProgress();
            }
        }

        /// <summary>
        /// Overrides the Closing Event to Animate the Slide Out
        /// </summary>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (DesignMode) return;
            if (_formOverlay != null)
            {
                _formOverlay.Visible = false;
                _formOverlay.Close();
                _formOverlay.Dispose();
                _formOverlay = null;
            }

            DialogResult res = DialogResult;

            base.OnClosing(e);
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (DesignMode) return false;
            if (ModifierKeys != Keys.None || keyData != Keys.Escape) return base.ProcessDialogKey(keyData);
            Close();
            return true;
        }
    }
}
