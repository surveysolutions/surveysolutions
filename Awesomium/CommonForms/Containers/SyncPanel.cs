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
        ManualResetEvent inactiveStatus = new ManualResetEvent(true);

        internal EventHandler PushPressed;
        internal EventHandler PullPressed;
        internal EventHandler CancelPressed;

        private DriveInfo choozenUSB = null;

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
        private List<DriveInfo> ReviewDriversList()
        {
            List<DriveInfo> drivers = new List<DriveInfo>();
            DriveInfo[] listDrives = DriveInfo.GetDrives();

            foreach (var drive in listDrives)
            {
                if (drive.DriveType == DriveType.Removable)
                    drivers.Add(drive);
            }

            return drivers;
        }

        internal void UpdateUsbList()
        {
            this.usbStrip.Items.Clear();
            this.choozenUSB = null;

            var drives = ReviewDriversList();

            ComponentResourceManager resources = new ComponentResourceManager(typeof(SyncPanel));
            foreach (var drive in drives)
            {
                int imageIndex = 0;
                bool theDriverIsChoozen = this.choozenUSB != null && string.Compare(drive.Name, this.choozenUSB.Name, true) == 0;
                if (theDriverIsChoozen)
                {
                    this.choozenUSB = drive;
                    imageIndex = 1;
                }

                FlatButton item = new FlatButton()
                {
                    Text = drive.Name.Trim(new char[]{'/','\\'}),
                    Tag = drive,

                    Image = this.usbImageList.Images[imageIndex],
                    Font = new Font(this.tableLayoutPanel2.Font, FontStyle.Italic | FontStyle.Bold | FontStyle.Underline),
                    ImageAlign = ContentAlignment.TopCenter,
                    TextAlign = ContentAlignment.TopCenter,
                };

                item.Click += UsbChoosen;

                this.usbStrip.Items.Add(new ToolStripControlHost(item));
            }

            if (this.usbStrip.Items.Count == 0)
            {
                this.usbStrip.Visible = false;
                this.usbStrip.Height = 120;
                SetLabel(this.labelAvlUsb, null);
            }
            else
            {
                this.usbStrip.Visible = true;
                SetLabel(this.labelAvlUsb, "Available USB drivers:");
            }
        }

        private void UsbChoosen(object sender, EventArgs args)
        {
            this.choozenUSB = null;

            foreach (var usbItem in this.usbStrip.Items)
            {
                var control = (usbItem as ToolStripControlHost).Control;
                var button = control as FlatButton;
                if (button == null)
                    continue;

                if (button == sender)
                {
                    this.choozenUSB = button.Tag as DriveInfo;
                    button.Image = this.usbImageList.Images[1];
                }
                else
                    button.Image = this.usbImageList.Images[0];
            }
        }

        internal void ShowResult(string log)
        {
            SetLabel(this.resultLabel, log);
        }

        internal void ShowError(string log)
        {
            SetLabel(this.statusLabel, log, true);
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
                Invoke(new MethodInvoker(() => this.progressBar.Show()));
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

        #region IDriverProvider Memebers

        public DriveInfo ActiveUsb
        {
            get { return this.choozenUSB; }
        }

        public bool IsAnyAvailable
        {
            get { return ReviewDriversList().Count > 0; }
        }

        #endregion

        internal void EnablePush(bool enable)
        {
            this.pushButton.Enabled = enable;
        }

        internal void EnablePull(bool enable)
        {
            this.pullButton.Enabled = enable;
        }
    }
}
