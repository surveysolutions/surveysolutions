using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Browsing.Common.Controls;
using Synchronization.Core.Interface;
using Synchronization.Core.SynchronizationFlow;

namespace Browsing.Common.Containers
{
    public enum SyncState
    {
        Idle,
        Pull,
        Push
    }

    public partial class SyncPanel : UserControl, ISyncProgressObserver, IUsbProvider
    {
 

        internal EventHandler PushPressed;
        internal EventHandler PullPressed;
        internal EventHandler CancelPressed;
        



        public SyncPanel()
        {
            InitializeComponent();
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

        public void UpdateUsbStatusPanelUsbList()
        {
            this.usbStatusPanel.UpdateUsbList();
        }

        public void UpdateUsbStatusPanelLook()
        {
            this.usbStatusPanel.UpdateLook();
        }

        public void SetIdleState()
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
        

        

        internal void ShowResult(string log)
        {
            usbStatusPanel.ChangeResultLabel(log);

        }

        internal void ShowError(string log)
        {
            usbStatusPanel.ChangeStatusLabel(log, true);
        }

        #region Helpers

        /// <summary>
        /// Hide the form. Wait before hiding
        /// </summary>
        /// <param name="waitTime">Time in milliseconds to wait before hiding</param>


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
             usbStatusPanel.ChangeStatusLabel(string.Format("{0} data is being processed. Please wait...", status.ActionType == SyncType.Pull ? "Pulling" : "Pushing"));
             usbStatusPanel.ChangeResultLabel(null);

            this.usbStatusPanel.InactiveStatus.WaitOne(5000);

            if (InvokeRequired)
                Invoke(new MethodInvoker(() => this.progressBar.Show()));
            else
                this.progressBar.Show();

            this.usbStatusPanel.InactiveStatus.Reset();
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
            usbStatusPanel.ChangeStatusLabel(statusText, error);
            usbStatusPanel.Deactivate(false);
        }

        #endregion

        #region IDriverProvider Memebers

        public DriveInfo ActiveUsb
        {
            get { return this.usbStatusPanel.ChosenUsb; }
        }

        public bool IsAnyAvailable
        {
            get { return usbStatusPanel.ReviewDriversList().Count > 0; }
        }

        #endregion

        internal void EnablePush(bool enable)
        {
            if (this.pushButton.InvokeRequired)
                this.pushButton.Invoke(new MethodInvoker(() => this.pushButton.Enabled = enable));
            else
                this.pushButton.Enabled = enable;
        }

        internal void EnablePull(bool enable)
        {
            if (this.pullButton.InvokeRequired)
                this.pullButton.Invoke(new MethodInvoker(() => this.pullButton.Enabled = enable));
            else
                this.pullButton.Enabled = enable;
        }
    }
}
