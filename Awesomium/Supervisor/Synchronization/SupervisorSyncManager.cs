using Common.Utils;
using System.Threading;
using System.Collections.Generic;
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

        public SupervisorSyncManager(ISyncProgressObserver pleaseWait, ISettingsProvider provider, IRequesProcessor requestProcessor, IUrlUtils urlUtils)
            : base(pleaseWait, provider, requestProcessor, urlUtils)
        {
        }

        #endregion

        #region FromCAPI

        protected override void CheckPushPrerequisites()
        {
            var result = false;
            result =
                this.RequestProcessor.Process<bool>(UrlUtils.GetCheckPushPrerequisitesUrl(), false);
            if (!result)
                throw new CheckPrerequisitesException("Current device don't have any changes", SyncType.Push, null);
        }

        #endregion

        protected override void CheckPullPrerequisites()
        {
           throw new Exception("No implemented");
        }

        protected override void OnAddSynchronizers(IList<ISynchronizer> syncChain, ISettingsProvider settingsProvider)
        {
            syncChain.Add(new UsbSynchronizer(settingsProvider, this.UrlUtils));
        }

        #region Methods

        internal void ExportQuestionaries()
        {
            new Thread(DoExport).Start();
        }

        #endregion

        #region Helpers

        private void DoExport()
        {
            this.PushSupervisorCapi(SyncDirection.Up);
        }

        #endregion
    }
}
