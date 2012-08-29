using Common.Utils;
using global::Synchronization.Core;
using global::Synchronization.Core.Interface;

namespace Browsing.Supervisor.Synchronization
{
    using System.Collections.Generic;

    /// <summary>
    /// The class is responsible for import/export operations by CAPI
    /// </summary>
    public class SupervisorSyncManager : SyncManager
    {
        #region C-tor

        public SupervisorSyncManager(ISyncProgressObserver pleaseWait, ISettingsProvider provider, IRequesProcessor requestProcessor, IUrlUtils urlUtils)
            : base(pleaseWait, provider, requestProcessor, urlUtils)
        {
        }

        #endregion

        protected override void CheckPushPrerequisites()
        {
            throw new System.NotImplementedException();
        }

        protected override void CheckPullPrerequisites()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnAddSynchronizers(IList<ISynchronizer> syncChain, ISettingsProvider settingsProvider)
        {
            throw new System.NotImplementedException();
        }
    }
}
