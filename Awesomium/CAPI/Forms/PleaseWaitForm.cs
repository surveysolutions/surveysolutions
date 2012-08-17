using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Synchronization.Core.Interface;

namespace Browsing.CAPI.Forms
{
    public partial class PleaseWaitControl : UserControl, ISyncProgressObserver
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

            if (IsDisposed || Parent == null || Parent.IsDisposed)
                return;

            if (InvokeRequired)
                Invoke(new MethodInvoker(() => { this.Parent.Hide(); }));
            else
                this.Parent.Hide();

            this.inactiveStatus.Set();
        }

        private void MakeInvisibleProgressBar(object waitTime)
        {
            Thread.Sleep((int)waitTime);

            if (IsDisposed || this.progressBar == null || this.progressBar.IsDisposed)
                return;

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

        /// <summary>
        /// Show progress state
        /// </summary>
        /// <param name="progressPercentage"></param>
        private void SetProgress(int progressPercentage)
        {
            if (this.progressBar.InvokeRequired)
                this.progressBar.Invoke(new MethodInvoker(() => { this.progressBar.Value = progressPercentage; }));
            else
                this.progressBar.Value = progressPercentage;

        }

        #endregion

        #region Methods

        /// <summary>
        /// Show progress state
        /// </summary>
        /// <param name="status"></param>
        public void SetProgress(ISyncProgressStatus status)
        {
            SetProgress(status.ProgressPercents);
        }

        /// <summary>
        /// Reset the form content and put it foreground
        /// </summary>
        public void SetBeginning()
        {
            SetProgress(0);
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
        /// <param name="errors"></param>
        /// <remarks>We should add error report if canceling caused by an error</remarks>
        public void SetCompleted(ISyncProgressStatus status)
        {
            if (status.IsCanceled || status.IsError)
            {
                if (status.IsCanceled && !status.IsError)
                    SetStatusLabel("Data export canceled", true);
                else
                {
                    SetStatusLabel("Data export error", true);

                }
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
