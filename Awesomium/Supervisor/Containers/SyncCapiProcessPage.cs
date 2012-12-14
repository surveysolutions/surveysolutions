using Browsing.Common.Controls;
using Browsing.Supervisor.Synchronization;
using Common.Utils;
using Synchronization.Core.Interface;
using Synchronization.Core.SynchronizationFlow;
using System.Threading;

namespace Browsing.Supervisor.Containers
{
    public partial class SyncCapiProcessPage : Common.Containers.Synchronization
    {
        #region Constructor

        public SyncCapiProcessPage(
            ISettingsProvider clientSettings,
            IRequestProcessor requestProcessor,
            IUrlUtils utils,
            ScreenHolder holder)
            : base(clientSettings, requestProcessor, utils, holder)
        {
            InitializeComponent();

            EnablePush(false);
        }

        #endregion

        #region Overloaded

        protected override ISyncManager DoInstantiateSyncManager(ISyncProgressObserver progressObserver, ISettingsProvider clientSettings, IRequestProcessor requestProcessor, IUrlUtils utils, IUsbProvider usbProvider)
        {
            return new SupervisorSyncManager(progressObserver, clientSettings, requestProcessor, utils, usbProvider);
        }

        #endregion

        private void ExportData()
        {
            //SyncManager.Push(SyncDirection.Down);
        }

        private void ImportData()
        {
            SyncManager.Pull(SyncDirection.Up);
        }

        protected override void OnEnablePush(bool enable)
        {
            base.OnEnablePush(false);
        }

        protected override void OnPushClicked()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnPullClicked()
        {
            new Thread(ImportData).Start();
        }
    }
}
