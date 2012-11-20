using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Common.Utils;
using Newtonsoft.Json;
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
            syncChain.Add(new NetworkSynchronizer(settingsProvider, RequestProcessor, this.UrlUtils));
            syncChain.Add(new UsbSynchronizer(settingsProvider, this.UrlUtils, this.UsbProvider));
        }

        protected override List<string> OnGetStatisticsAfterSyncronization(SyncType action)
        {
            if (action==SyncType.Push)
                return GetPushStatistics();
            return GetPullStatistics();
        }

        #region Helpers

        private List<string> GetPushStatistics()
        {
            var ret = new List<string>();
            try
            {
                var result = this.RequestProcessor.Process<string>(UrlUtils.GetPushStatisticUrl(), String.Empty);
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };
                var items = JsonConvert.DeserializeObject<List<SyncStatisticInfo>>(result, settings);

                ret.AddRange(items.Select(syncStatisticInfo => syncStatisticInfo.ApprovedQuestionaries + " " + syncStatisticInfo.InterviewersName + " questionnaires were sent for approval"));
                
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
                var result = this.RequestProcessor.Process<string>(UrlUtils.GetPullStatisticUrl(), String.Empty);
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };
                var items = JsonConvert.DeserializeObject<List<SyncStatisticInfo>>(result, settings);
                var line = items.Where(syncStatisticInfo => syncStatisticInfo.New).Aggregate("New interviewers were received: ", (current, syncStatisticInfo) => current + syncStatisticInfo.InterviewersName+", ");
                
                if (items.Where(syncStatisticInfo => syncStatisticInfo.New).Count()>0)ret.Add(line.Substring(0,line.Length-2));

                foreach (var syncStatisticInfo in items)
                {
                    if (syncStatisticInfo.Assignments>0)
                    
                        ret.Add(syncStatisticInfo.InterviewersName + " has got" + syncStatisticInfo.Assignments +" new assignments");

                    if (syncStatisticInfo.RejectQuestionaries>0)
                        ret.Add(syncStatisticInfo.RejectQuestionaries+" "+syncStatisticInfo.InterviewersName + "'s questionnaires were rejected");

                    if (syncStatisticInfo.ApprovedQuestionaries>0)
                        ret.Add(syncStatisticInfo.ApprovedQuestionaries+" "+syncStatisticInfo.InterviewersName + "'s questionnaires were approved");
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
