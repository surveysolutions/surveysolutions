using System;
using System.Windows.Forms;
using Common.Utils;
using Synchronization.Core.Events;
using Synchronization.Core.Interface;
using Synchronization.Core.SynchronizationFlow;
using Synchronization.Core;
using System.Threading;

namespace Browsing.Common.Containers
{
    public abstract partial class Synchronization : Screen
    {
        public Synchronization(ISettingsProvider clientSettings, IRequesProcessor requestProcessor, IUrlUtils utils, ScreenHolder holder)
            : base(holder, true)
        {
            InitializeComponent();

            this.syncPanel.Parent = ContentPanel;

            this.syncPanel.PullPressed += btnPull_Click;
            this.syncPanel.PushPressed += btnPush_Click;
            this.syncPanel.CancelPressed += btnCancel_Click;

            this.SyncManager = DoInstantiateSyncManager(this.syncPanel, clientSettings, requestProcessor, utils, this.syncPanel);

            System.Diagnostics.Debug.Assert(this.SyncManager != null);

            this.SyncManager.EndOfSync += new EventHandler<SynchronizationCompletedEvent>(sync_EndOfSync);
            this.SyncManager.BgnOfSync += new EventHandler<SynchronizationEvent>(sync_BgnOfSync);
        }

        protected abstract internal ISyncManager DoInstantiateSyncManager(ISyncProgressObserver progressObserver, ISettingsProvider clientSettings, IRequesProcessor requestProcessor, IUrlUtils utils, IUsbProvider usbProvider);
        protected abstract internal void OnPushClicked();
        protected abstract internal void OnPullClicked();

        private void sync_EndOfSync(object sender, SynchronizationCompletedEvent e)
        {
            if (this.InvokeRequired)
                this.Invoke(new MethodInvoker(() =>
                                                  {
                                                      this.syncPanel.ShowResult(e.Log);
                                                      this.syncPanel.State = SyncState.Idle;
                                                  }));
        }

        private void sync_BgnOfSync(object sender, SynchronizationEvent e)
        {
            if (this.InvokeRequired)
                this.Invoke(new MethodInvoker(() =>
                                                  {
                                                      this.syncPanel.State = 
                                                          e.Status.ActionType == SyncType.Push ? SyncState.Push : SyncState.Pull;
                                                  }));
        }

        #region Event Handlers

        private void btnPush_Click(object sender, EventArgs e)
        {
            try
            {
                OnPushClicked();
            }
            catch
            {
            }

        }

        private void btnPull_Click(object sender, EventArgs e)
        {
            try
            {
                OnPullClicked();
            }
            catch
            {
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                this.SyncManager.Stop();
            }
            catch
            {
            }
        }

        #endregion

        #region Fields

        protected internal ISyncManager SyncManager { get; private set; }

        #endregion

        #region Overloading

        protected override void OnUpdateConfigDependencies()
        {
            base.OnUpdateConfigDependencies();

            this.SyncManager.UpdateSynchronizersList();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.syncPanel.UpdateLook();
        }

        #endregion

        public void UpdateUsbList()
        {
            this.syncPanel.UpdateUsbList();
        }
    }
}
