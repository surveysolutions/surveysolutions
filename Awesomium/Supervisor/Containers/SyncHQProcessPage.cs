using Browsing.Common.Controls;
using Browsing.Supervisor.Synchronization;
using Common.Utils;
using Synchronization.Core.Interface;

namespace Browsing.Supervisor.Containers
{
    public partial class SyncHQProcessPage : Common.Containers.Synchronization
    {
        public SyncHQProcessPage(
            ISettingsProvider clientSettings, 
            IRequestProcessor requestProcessor, 
            IUrlUtils utils, 
            ScreenHolder holder)
            : base(clientSettings, requestProcessor, utils, holder)
        {
            InitializeComponent();

            ContentPanel.Enabled = false;
        }

        protected override ISyncManager DoInstantiateSyncManager(ISyncProgressObserver progressObserver, ISettingsProvider clientSettings, IRequestProcessor requestProcessor, IUrlUtils utils, IUsbProvider usbProvider)
        {
            return new SupervisorSyncManager(progressObserver, clientSettings, requestProcessor, utils, usbProvider);
        }

        protected override void OnPushClicked()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnPullClicked()
        {
            throw new System.NotImplementedException();
        }
    }
}
