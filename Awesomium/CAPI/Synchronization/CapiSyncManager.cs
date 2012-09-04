using System;
using System.Collections.Generic;
using System.Threading;

using Common.Utils;
using Synchronization.Core;
using Synchronization.Core.Errors;
using Synchronization.Core.Interface;
using Synchronization.Core.SynchronizationFlow;

namespace Browsing.CAPI.Synchronization
{
    /// <summary>
    /// The class is responsible for import/export operations by CAPI
    /// </summary>
    public class CapiSyncManager : SyncManager
    {
        #region C-tor

        public CapiSyncManager(ISyncProgressObserver pleaseWait, ISettingsProvider provider, IRequesProcessor requestProcessor, IUrlUtils urlUtils, IUsbProvider usbProvider)
            : base(pleaseWait, provider, requestProcessor, urlUtils, usbProvider)
        {
        }

        #endregion

        protected override void OnAddSynchronizers(IList<ISynchronizer> syncChain, ISettingsProvider settingsProvider)
        {
            if (!string.IsNullOrEmpty(UrlUtils.GetEnpointUrl()))
                syncChain.Add(new NetworkSynchronizer(settingsProvider, RequestProcessor, this.UrlUtils));
    
            syncChain.Add(new UsbSynchronizer(settingsProvider, this.UrlUtils, this.UsbProvider));
        }

        #region Helpers

        private void DoExport()
        {
            Push(SyncDirection.Up);
        }

        private void DoImport()
        {
            Pull(SyncDirection.Down);
        }

        #endregion

        #region Overloaded
        
        protected override void CheckPushPrerequisites()
        {
//#if !DEBUG
            var result =  this.RequestProcessor.Process<bool>(UrlUtils.GetCheckPushPrerequisitesUrl(), false);
            if (!result)
                throw new CheckPrerequisitesException("Current device doesn't contain any changes to export", SyncType.Push, null);
//#endif
        }

        protected override void CheckPullPrerequisites()
        {
            // Prerequisites empty at this moment
        }

        protected override string OnDoSynchronizationAction(SyncType action, SyncDirection direction)
        {
            string syncResult = null;
            try
            {
                syncResult = base.OnDoSynchronizationAction(action, direction);
            }
            catch (Exception e)
            {
                //syncResult = e.Message;
                throw e;
            }
            /*finally
            {
                if (!string.IsNullOrEmpty(syncResult))
                    MessageBox.Show(syncResult);
            }*/

            return syncResult;
        }

        #endregion
    }
}
