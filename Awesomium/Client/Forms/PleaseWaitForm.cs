using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Client
{
    internal interface IStatusIndicator
    {
        void ActivateExportState();
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

        ManualResetEvent inactiveStatus = new ManualResetEvent(true);

        #region Helpers

        /// <summary>
        /// Hide the form. Wait before hiding
        /// </summary>
        /// <param name="waitTime">Time in milliseconds to wait before hiding</param>
        private void MakeInvisibleAll(object waitTime)
        {
            Thread.Sleep((int)waitTime);

            if (InvokeRequired)
                Invoke(new MethodInvoker(() => { this.Parent.Hide(); }));
            else
                this.Parent.Hide();

            this.inactiveStatus.Set();
        }

        private void MakeInvisibleProgressBar(object waitTime)
        {
            Thread.Sleep((int)waitTime);

            if (this.progressBar.InvokeRequired)
                Invoke(new MethodInvoker(() => { this.progressBar.Hide(); }));
            else
                this.progressBar.Hide();
        }

        /// <summary>
        /// Assign text content to status label
        /// </summary>
        /// <param name="status"></param>
        private void SetStatusLabel(string status, bool error = false)
        {
            if (this.statusLabel.InvokeRequired)
            {
                this.statusLabel.Invoke(new MethodInvoker(() =>
                {
                    this.statusLabel.Text = status;
                    this.statusLabel.ForeColor = error ? System.Drawing.Color.Red : System.Drawing.Color.Black;
                    BringToFront();
                }));
            }
            else
            {
                this.statusLabel.Text = status;
                this.statusLabel.ForeColor = error ? System.Drawing.Color.Red : System.Drawing.Color.Black;
                BringToFront();
            }
        }

        /// <summary>
        /// Helper method to dissapear in diffrent thread if wait operation expected before hiding the form
        /// </summary>
        /// <param name="immediately"></param>
        private void Deactivate(bool immediately)
        {
            if (immediately)
                MakeInvisibleAll(0);
            else
                new Thread(MakeInvisibleAll).Start(3000);
        }

        #endregion

        #region Methods

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
        /// Reset the form content and put it foreground
        /// </summary>
        public void ActivateExportState()
        {
            AssignProgress(0);
            SetStatusLabel("Export started. Plase wait...");

            this.inactiveStatus.WaitOne(5000);

            if (InvokeRequired)
                Invoke(new MethodInvoker(() => { this.Parent.Show(); this.progressBar.Show(); }));
            else
            {
                this.Parent.Show();
                this.progressBar.Show();
            }

            this.inactiveStatus.Reset();
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
                if (canceled || error == null)
                    SetStatusLabel("Data export canceled", true);
                else
                    SetStatusLabel("Data export error:" + error.Message, true);

                MakeInvisibleProgressBar(0);
                Deactivate(false);

                return;
            }

            SetStatusLabel("Data export completed successfully.");
            Deactivate(false);
        }

        #endregion


    }
}
