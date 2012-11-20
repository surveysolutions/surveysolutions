using System;
using System.Threading;
using Browsing.CAPI.Synchronization;
using Browsing.Common.Controls;
using Common.Utils;
using Synchronization.Core.Interface;
using Synchronization.Core.SynchronizationFlow;

namespace Browsing.CAPI.Containers
{
    public partial class CAPISynchronization : Common.Containers.Synchronization
    {
        public CAPISynchronization(ISettingsProvider clientSettings, IRequesProcessor requestProcessor, IUrlUtils utils, ScreenHolder holder)
            : base(clientSettings, requestProcessor, utils, holder)
        {
            InitializeComponent();
        }

        protected override ISyncManager DoInstantiateSyncManager(ISyncProgressObserver progressObserver, ISettingsProvider clientSettings, IRequesProcessor requestProcessor, IUrlUtils utils, IUsbProvider usbProvider)
        {
            return new CapiSyncManager(progressObserver, clientSettings, requestProcessor, utils, usbProvider);
        }

        private void ExportData()
        {
            SyncManager.Push(SyncDirection.Up);
        }

        private void ImportData()
        {
            SyncManager.Pull(SyncDirection.Down);
        }

        protected override void OnPushClicked()
        {
            new Thread(ExportData).Start();
        }

        protected override void OnPullClicked()
        {
            new Thread(ImportData).Start();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.MakeStatisticListVisible(true);
            this.AddLineToStatistic("Statistic is empty..");
        }
    }
}
