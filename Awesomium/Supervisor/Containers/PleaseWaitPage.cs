using System.Threading;
using System.Windows.Forms;
using Synchronization.Core.Interface;
using Synchronization.Core.SynchronizationFlow;

namespace Browsing.Supervisor.Containers
{
    
    public partial class PleaseWaitPage : UserControl, ISyncProgressObserver
    {
        #region Constructor

        public PleaseWaitPage()
        {
            InitializeComponent();
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
                Invoke(new MethodInvoker(() =>
                {
                    if (!this.Parent.IsDisposed) 
                        this.Parent.Hide();
                }));
            else
                if (!this.Parent.IsDisposed)
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
        public void SetBeginning(ISyncProgressStatus status)
        {
            SetProgress(0);
            SetStatusLabel(string.Format("{0} data is being processed. Plase wait...", status.ActionType == SyncType.Pull ? "Pulling" : "Pushing"));

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
            if (status.Error != null)
            {
                SetStatusLabel(status.Error.Message, true);

                MakeInvisibleProgressBar(0);
                Deactivate(false);

                return;
            }

            SetStatusLabel(string.Format("{0} data has been completed successfully", status.ActionType == SyncType.Pull ? "Pulling" : "Pushing"));

            Deactivate(false);
        }

        #endregion
    }
}
