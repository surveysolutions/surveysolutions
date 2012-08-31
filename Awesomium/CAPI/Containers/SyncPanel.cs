using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using Synchronization.Core.Interface;
using Synchronization.Core.SynchronizationFlow;


namespace Browsing.CAPI.Containers
{
    public enum SyncState
    {
        Idle,
        Pull,
        Push
    }

    public partial class SyncPanel : UserControl, ISyncProgressObserver
    {
        ManualResetEvent inactiveStatus = new ManualResetEvent(true);

        internal EventHandler PushPressed;
        internal EventHandler PullPressed;
        internal EventHandler CancelPressed;

        public SyncPanel()
        {
            InitializeComponent();
        }

        internal void UpdateLook()
        {
            MakeInvisibleStatus(0);
            SetLabel(this.labelAvlUsb, null);
            SetIdleState();
            UpdateUsbList();
        }

        public SyncState State
        {
            set 
            {
                switch (value)
                {
                    case SyncState.Idle:
                        SetIdleState();
                        break;
                    case SyncState.Push:
                        SetPushState();
                        break;
                    case SyncState.Pull:
                        SetPullState();
                        break;
                }
            }
        }

        private void SetIdleState()
        {
            this.progressBar.Visible = false;
            this.cancelButton.Visible = false;
        }

        private void SetPullState()
        {
            this.cancelButton.Visible = true;
        }

        private void SetPushState()
        {
            this.cancelButton.Visible = true;
        }

        private void pullButton_Click(object sender, EventArgs e)
        {
            if (PullPressed != null)
                PullPressed(sender, e);

            SetPullState();
        }

        private void pushButton_Click(object sender, EventArgs e)
        {
            if (PushPressed != null)
                PushPressed(sender, e);

            SetPushState();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            if (CancelPressed != null)
                CancelPressed(sender, e);
        }

        /// <summary>
        /// Create list of available drivers
        /// </summary>
        /// <returns></returns>
        private List<string> ReviewDriversList()
        {
            List<string> drivers = new List<string>();
            DriveInfo[] listDrives = DriveInfo.GetDrives();

            foreach (var drive in listDrives)
            {
                if (drive.DriveType == DriveType.Removable)
                    drivers.Add(drive.ToString());
            }

            return drivers;
        }

        internal void UpdateUsbList()
        {
            var drives = ReviewDriversList();

            string s = string.Empty;
            foreach(var d in drives)
                s += d + "  ";

            SetLabel(this.labelAvlUsb, string.IsNullOrEmpty(s) ? null : string.Format("Available USB drivers: {0}", s));
        }

        internal void ShowResult(string log)
        {
            SetLabel(this.resultLabel, log);
        }

        #region Helpers

        /// <summary>
        /// Hide the form. Wait before hiding
        /// </summary>
        /// <param name="waitTime">Time in milliseconds to wait before hiding</param>
        private void MakeInvisibleStatus(object waitTime)
        {
            Thread.Sleep((int)waitTime);

            SetLabel(this.resultLabel, null);
            SetLabel(this.statusLabel, null);

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
        private void SetLabel(Label label, string status, bool error = false)
        {
            if (label.InvokeRequired)
            {
                label.Invoke(new MethodInvoker(() =>
                {
                    label.Visible = !string.IsNullOrEmpty(status);
                    label.Text = status;
                    label.ForeColor = error ? System.Drawing.Color.Red : System.Drawing.Color.Black;
                }));
            }
            else
            {
                label.Visible = !string.IsNullOrEmpty(status);
                label.Text = status;
                label.ForeColor = error ? System.Drawing.Color.Red : System.Drawing.Color.Black;
            }
        }

        /// <summary>
        /// Helper method to dissapear in diffrent thread if wait operation expected before hiding the form
        /// </summary>
        /// <param name="immediately"></param>
        private void Deactivate(bool immediately)
        {
            if (immediately)
                MakeInvisibleStatus(0);
            else
                new Thread(MakeInvisibleStatus).Start(10000);
        }

        /// <summary>
        /// Show progress state
        /// </summary>
        /// <param name="progressPercentage"></param>
        private void SetProgress(int progressPercentage)
        {
            if (this.progressBar.InvokeRequired)
                this.progressBar.Invoke(new MethodInvoker(() => 
                { 
                    this.progressBar.Value = progressPercentage; 
                }));
            else
                this.progressBar.Value = progressPercentage;

        }

        #endregion

        #region ISyncProgressObserver Memebers

        public void SetBeginning(ISyncProgressStatus status)
        {
            SetProgress(0);
            SetLabel(this.statusLabel, string.Format("{0} data is being processed. Plase wait...", status.ActionType == SyncType.Pull ? "Pulling" : "Pushing"));
            SetLabel(this.resultLabel, null);

            this.inactiveStatus.WaitOne(5000);

            if (InvokeRequired)
                Invoke(new MethodInvoker(() => this.progressBar.Show() ));
            else
                this.progressBar.Show();

            this.inactiveStatus.Reset();
        }

        public void SetProgress(ISyncProgressStatus status)
        {
            SetProgress(status.ProgressPercents);
        }

        public void SetCompleted(ISyncProgressStatus status)
        {
            var error = status.Error != null;

            var statusText = error ?
                status.Error.Message :
                string.Format("{0} data has been completed successfully", status.ActionType == SyncType.Pull ? "Pulling" : "Pushing");

            MakeInvisibleProgressBar(0);
            SetLabel(this.statusLabel, statusText, error);
            Deactivate(false);
        }

        #endregion
    }
}
