using Common.Utils;
using System.Collections.Generic;
using Synchronization.Core.Events;
using global::Synchronization.Core;
using global::Synchronization.Core.Errors;
using global::Synchronization.Core.Interface;
using global::Synchronization.Core.SynchronizationFlow;

namespace Browsing.Supervisor.Synchronization
{
    using System;

    /// <summary>
    /// The class is responsible for import/export operations by Supervisor
    /// </summary>
    public class SupervisorSyncManager : SyncManager
    {
        #region C-tor

        public SupervisorSyncManager(ISyncProgressObserver pleaseWait, ISettingsProvider provider, IRequesProcessor requestProcessor, IUrlUtils urlUtils, IUsbProvider usbProvider)
            : base(pleaseWait, provider, requestProcessor, urlUtils, usbProvider)
        {
        }

        #endregion

        #region FromCAPI

        protected override void CheckPushPrerequisites(SyncDirection direction)
        {
            var result = this.RequestProcessor.Process<bool>(UrlUtils.GetCheckPushPrerequisitesUrl(), false);
            if (!result)
                throw new CheckPrerequisitesException("Current device doesn't contain any changes to export", SyncType.Push, null);
        }

        #endregion

        protected override void CheckPullPrerequisites(SyncDirection direction)
        {
            // Prerequisites empty at this moment
        }

        protected override void OnAddSynchronizers(IList<ISynchronizer> syncChain, ISettingsProvider settingsProvider)
        {
            syncChain.Add(new UsbSynchronizer(settingsProvider, this.UrlUtils, this.UsbProvider));
        }

        protected override SynchronizationStatisticEvent OnGetStatisticsAfterSyncronization(SyncType action)
        {
            return null;
        }
    }
}
