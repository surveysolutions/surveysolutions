using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Management;
using System.Windows.Forms;
using Browsing.CAPI.ClientSettings;
using Browsing.CAPI.Forms;
using Browsing.CAPI.Synchronization;
using Common.Utils;
using Synchronization.Core.Events;
using Synchronization.Core.SynchronizationFlow;
using Synchronization.Core.Interface;

namespace Browsing.CAPI.Containers
{
    public partial class CAPISynchronization : Screen
    {
        public CAPISynchronization(ISettingsProvider clientSettings, IRequesProcessor requestProcessor, IUrlUtils utils, ScreenHolder holder)
            : base(holder, true)
        {
            InitializeComponent();

            this.syncPanel.Parent = ContentPanel;

            this.syncPanel.PullPressed += btnPull_Click;
            this.syncPanel.PushPressed += btnPush_Click;
            this.syncPanel.CancelPressed += btnCancel_Click;
            
            this.pleaseWait = new PleaseWaitControl();

            this.syncManager = new CapiSyncManager(this.syncPanel, clientSettings, requestProcessor, utils);
            this.syncManager.EndOfSync += new EventHandler<SynchronizationCompletedEvent>(sync_EndOfSync);
            this.syncManager.BgnOfSync += new EventHandler<SynchronizationEvent>(sync_BgnOfSync);
        }

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
                this.syncManager.ExportQuestionaries();
            }
            catch
            {
            }

        }

        private void btnPull_Click(object sender, EventArgs e)
        {
            try
            {
                this.syncManager.ImportQuestionaries();
            }
            catch
            {
            }

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                this.syncManager.Stop();
            }
            catch
            {
            }
        }

        #endregion

        #region Fields

        private PleaseWaitControl pleaseWait;
        private CapiSyncManager syncManager;

        #endregion

        #region Overloading

        protected override void OnUpdateConfigDependencies()
        {
            base.OnUpdateConfigDependencies();

            this.syncManager.UpdateSynchronizersList();
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
