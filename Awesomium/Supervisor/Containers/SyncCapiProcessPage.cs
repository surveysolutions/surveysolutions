using System;
using Common.Utils;
using System.Windows.Forms;
using Browsing.Supervisor.Controls;
using global::Synchronization.Core.Events;
using global::Synchronization.Core.Interface;

namespace Browsing.Supervisor.Containers
{
    using Browsing.Supervisor.Synchronization;

    public partial class SyncCapiProcessPage : Screen
    {
        #region Fields

        private bool repaint;
        private PleaseWaitPage pleaseWait;
        private SupervisorSyncManager syncManager;
        private StatusStrip statusStrip1;
        private ISettingsProvider clientSettings;

        #endregion

        #region Constructor

        public SyncCapiProcessPage(ISettingsProvider clientSettings, IRequesProcessor requestProcessor, IUrlUtils utils, ScreenHolder holder)
            : base(holder, true)
        {
            InitializeComponent();
            this.pleaseWait = new PleaseWaitPage();
            this.clientSettings = clientSettings;
            this.syncManager = new SupervisorSyncManager(this.pleaseWait, this.clientSettings, requestProcessor, utils);
            this.syncManager.EndOfSync += new EventHandler<SynchronizationCompletedEvent>(sync_EndOfSync);
            this.syncManager.BgnOfSync += new EventHandler<SynchronizationEvent>(sync_BgnOfSync);
            this.statusStrip1.Hide();
            var host = new ToolStripControlHost(this.pleaseWait);
            host.Size = this.statusStrip1.Size;
            this.statusStrip1.Items.AddRange(new ToolStripItem[] {host});
        }

        #endregion

        private void EnableDisableMenuItems(bool enable)
        {
            this.btnPull.Enabled = enable;
            this.btnPush.Enabled = enable;
            this.btnCancel.Visible = !enable;
        }

        private void sync_EndOfSync(object sender, SynchronizationCompletedEvent e)
        {
            if (this.InvokeRequired)
                this.Invoke(new MethodInvoker(() =>
                                                  {
                                                      MessageBox.Show(this, e.Log);
                                                      EnableDisableMenuItems(true);
                                                  }));
        }

        private void sync_BgnOfSync(object sender, SynchronizationEvent e)
        {
            if (this.InvokeRequired)
                this.Invoke(new MethodInvoker(() =>
                                                  {
                                                      EnableDisableMenuItems(false);
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


        private void HQSynchronization_Load(object sender, EventArgs e)
        {
        }

        protected override void OnUpdateConfigDependencies()
        {
            //base.OnUpdateConfigDependencies();
            //this.syncManager.UpdateSynchronizersList();
        }
    }
}
