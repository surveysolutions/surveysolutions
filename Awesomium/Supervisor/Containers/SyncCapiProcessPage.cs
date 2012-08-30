using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Browsing.Supervisor.Containers
{
    using Browsing.Supervisor.Controls;

    using Common.Utils;

    using global::Synchronization.Core.Events;
    using global::Synchronization.Core.Interface;

    public partial class SyncCapiProcessPage : Screen
    {
        #region Fields

        private bool repaint;
        //private PleaseWaitControl pleaseWait;
        //private CapiSyncManager syncManager;
        private StatusStrip statusStrip1;
        private ISettingsProvider clientSettings;

        #endregion

        public SyncCapiProcessPage(ISettingsProvider clientSettings, IRequesProcessor requestProcessor, IUrlUtils utils, ScreenHolder holder)
            : base(holder, true)
        {
            InitializeComponent();
            //this.pleaseWait = new PleaseWaitControl();
            //this.clientSettings = clientSettings;
            //this.syncManager = new CapiSyncManager(this.pleaseWait, this.clientSettings, requestProcessor, utils);
            //this.syncManager.EndOfSync += new EventHandler<SynchronizationCompletedEvent>(sync_EndOfSync);
            //this.syncManager.BgnOfSync += new EventHandler<SynchronizationEvent>(sync_BgnOfSync);
            this.statusStrip1.Hide();
            //var host = new ToolStripControlHost(this.pleaseWait);
            //host.Size = this.statusStrip1.Size;
            //this.statusStrip1.Items.AddRange(new ToolStripItem[] {host});
        }

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
            //try
            //{
            //    this.syncManager.ExportQuestionaries();
            //}
            //catch
            //{
            //}

        }

        private void btnPull_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    this.syncManager.ImportQuestionaries();
            //}
            //catch
            //{
            //}

        }

        #endregion

        private void btnCancel_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    this.syncManager.Stop();
            //}
            //catch
            //{
            //}
        }

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
