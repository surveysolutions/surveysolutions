using System;
using System.Threading;
using System.Windows.Forms;

namespace Client
{
    internal interface IStatusIndicator
    {
        void Reset();
        void AssignProgress(int progressPercentage);
        void SetCompletedStatus(bool canceled, Exception error);
    }

    public partial class PleaseWaitControl : UserControl, IStatusIndicator
    {
        #region C-tor

        public PleaseWaitControl()
        {
            //copyJobSize = GetJobSize(Configuration.ProgramDirectory);
            InitializeComponent();

            // since we are going to optimize resources and keep this form in memory the calls below are neccessary 
            // to activate the form in the main thread, while c-tor is called
            //Show();
            //Hide();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Hide the form. Wait before hiding
        /// </summary>
        /// <param name="waitTime">Time in milliseconds to wait before hiding</param>
        private void MakeInvisible(object waitTime)
        {
            Thread.Sleep((int)waitTime);

            if (InvokeRequired)
                Invoke(new MethodInvoker(() => { this.Parent.Hide(); }));
            else
                this.Parent.Hide();
        }

        /// <summary>
        /// Assign text content to status label
        /// </summary>
        /// <param name="status"></param>
        private void SetStatus(string status)
        {
            if (this.statusLabel.InvokeRequired)
            {
                this.statusLabel.Invoke(new MethodInvoker(() =>
                {
                    this.statusLabel.Text = status;
                    BringToFront();
                }));
            }
            else
            {
                this.statusLabel.Text = status;
                BringToFront();
            }
        }

        /// <summary>
        /// Helper method to dissapear in diffrent thread if wait operation expected before hiding the form
        /// </summary>
        /// <param name="immediately"></param>
        private void Dissapear(bool immediately)
        {
            if (immediately)
            {
                MakeInvisible(0);
                return;
            }

            new Thread(MakeInvisible).Start(2000);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Reset the form content and put it foreground
        /// </summary>
        public void Reset()
        {
            AssignProgress(0);
            SetStatus("Export started. Plase wait...");

            if (InvokeRequired)
                Invoke(new MethodInvoker(() => { this.Parent.Show(); }));
            else
                this.Parent.Show();
        }

        /// <summary>
        /// Show progress state
        /// </summary>
        /// <param name="progressPercentage"></param>
        public void AssignProgress(int progressPercentage)
        {
            if (this.progressBar.InvokeRequired)
                this.progressBar.Invoke(new MethodInvoker(() => { this.progressBar.Value = progressPercentage; }));
            else
                this.progressBar.Value = progressPercentage;
        }

        /// <summary>
        /// Show comleted status if ok, otherwise just hide for now
        /// </summary>
        /// <param name="canceled"></param>
        /// <param name="error"></param>
        /// <remarks>We should add error report if canceling caused by an error</remarks>
        public void SetCompletedStatus(bool canceled, Exception error)
        {
            if (canceled || error != null)
            {
                Dissapear(true);
                return;
            }

            SetStatus("Data export completed successfully.");
            Dissapear(false);
        }

        #endregion
    }
}
