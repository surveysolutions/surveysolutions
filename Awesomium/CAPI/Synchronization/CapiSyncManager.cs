using System;
using System.Collections.Generic;
using System.Linq;

using Common.Utils;
using Newtonsoft.Json;
using Synchronization.Core;
using Synchronization.Core.Errors;
using Synchronization.Core.Events;
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
            syncChain.Add(new NetworkSynchronizer(settingsProvider, RequestProcessor, this.UrlUtils));
            syncChain.Add(new UsbSynchronizer(settingsProvider, this.UrlUtils, this.UsbProvider));
        }

        protected override SynchronizationStatisticEvent OnGetStatisticsAfterSyncronization(SyncType action)
        {
            return action==SyncType.Push ? new SynchronizationStatisticEvent(GetPushStatistics()) : new SynchronizationStatisticEvent(GetPullStatistics());
        }

        #region Helpers

        private List<SyncStatisticInfo> GetStatItems(string url)
        {
            var result = this.RequestProcessor.Process<string>(url, String.Empty);
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };
            return  JsonConvert.DeserializeObject<List<SyncStatisticInfo>>(result, settings);
        }

        private List<string> GetPushStatistics()
        {
            var ret = new List<string>();
            try
            {
                var items = GetStatItems(UrlUtils.GetPushStatisticUrl());
                if (items.Count > 0)
                    foreach (var syncStatisticInfo in items)
                    {
                        if (syncStatisticInfo.Approved>0)
                            ret.Add(syncStatisticInfo.Approved + " " + syncStatisticInfo.UserName + "'s questionnaires were sent for approval");
                    }
                    
                else ret.Add("Not a questionnaire was sent for approval");
                
            }
            catch (Exception ex)
            {
                ret.Add("Statistic view error: "+ex.Message);
                
            }
            return ret;
            
        }

        private List<string> GetPullStatistics()
        {
            var ret = new List<string>();
            try
            {
                var items = GetStatItems(UrlUtils.GetPullStatisticUrl());
                var line = items.Where(syncStatisticInfo => syncStatisticInfo.IsNew).Aggregate("New interviewers were received: ", (current, syncStatisticInfo) => current + syncStatisticInfo.UserName+", ");

                if (items.Where(syncStatisticInfo => syncStatisticInfo.IsNew).Count() > 0) ret.Add(line.Substring(0, line.Length - 2));

                foreach (var syncStatisticInfo in items)
                {
                    if (syncStatisticInfo.NewAssignments>0)
                    
                        ret.Add(syncStatisticInfo.UserName + " has got " + syncStatisticInfo.NewAssignments +" new assignments");

                    if (syncStatisticInfo.Rejected>0)
                        ret.Add(syncStatisticInfo.Rejected + " " + syncStatisticInfo.UserName + "'s questionnaires were rejected");

                    if (syncStatisticInfo.Approved>0)
                        ret.Add(syncStatisticInfo.Approved + " " + syncStatisticInfo.UserName + "'s questionnaires were approved");
                }

            }
            catch (Exception ex)
            {
                ret.Add("Statistic view error: " + ex.Message);

            }
            return ret;
        }

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

        protected override void CheckPushPrerequisites(SyncDirection direction)
        {
//#if !DEBUG
            var result =  this.RequestProcessor.Process<bool>(UrlUtils.GetCheckPushPrerequisitesUrl(), false);
            if (!result)
                throw new CheckPrerequisitesException("Current device doesn't contain any changes to export", SyncType.Push, null);
//#endif
        }

        protected override void CheckPullPrerequisites(SyncDirection direction)
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
