using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Common.Utils;
using Synchronization.Core.Interface;
using Synchronization.Core.Events;
using Synchronization.Core.Errors;

namespace Synchronization.Core.SynchronizationFlow
{
    public class NetworkSynchronizer : AbstractSynchronizer
    {
        #region variables

        private readonly IRequesProcessor _requestProcessor;
        private readonly IUrlUtils _urlUtils;
        private AutoResetEvent stopRequested = new AutoResetEvent(false);

        #endregion

        public NetworkSynchronizer(ISettingsProvider settingsprovider, IRequesProcessor requestProcessor, IUrlUtils urlUtils)
            : base(settingsprovider)
        {
            this._urlUtils = urlUtils;
            this._requestProcessor = requestProcessor;
        }

        #region Overrides of AbstractSynchronizer

        protected override void OnPush(SyncDirection direction)
        {
            try
            {
                var processGuid = this._requestProcessor.Process<Guid>(this._urlUtils.GetPushUrl(this.SettingsProvider.Settings.ClientId), default(Guid));

                WaitForEndProcess(processGuid, OnSyncProgressChanged, SyncType.Push, direction);
            }
            catch (Exception e)
            {
                throw new SynchronizationException(
                    string.Format("Push to local center {0} is failed ", this._urlUtils.GetEnpointUrl()), e);
            }
        }

        protected override void OnPull(SyncDirection direction)
        {
            try
            {
                var processGuid = this._requestProcessor.Process<Guid>(this._urlUtils.GetPullUrl(this.SettingsProvider.Settings.ClientId), default(Guid));
                
                WaitForEndProcess(processGuid, OnSyncProgressChanged, SyncType.Pull, direction);
            }
            catch (Exception e)
            {
                throw new SynchronizationException(
                   string.Format("Pull from local center {0} is failed ", this._urlUtils.GetEnpointUrl()), e);
            }
        }

        protected override void OnStop()
        {
            this.stopRequested.Set();
        }

        protected override IList<SynchronizationException> OnCheckSyncIssues(SyncType syncType, SyncDirection direction)
        {
            SynchronizationException e = null;
            try
            {
                var netEndPoint = this._urlUtils.GetEnpointUrl();

                // test if there is connection to synchronization endpoint
                if (this._requestProcessor.Process<string>(netEndPoint, "False") == "False")
                    e = new NetUnreachableException(netEndPoint);
            }
            catch(Exception ex)
            {
                e = new NetUnreachableException(ex.Message);
            }

            return e == null ? null : new List<SynchronizationException>() { e };
        }

        protected override bool OnUpdateStatus()
        {
            return !string.IsNullOrEmpty(this._urlUtils.GetEnpointUrl());
        }

        protected override IList<SynchronizationException> OnGetInactiveErrors()
        {
            var errors = base.OnGetInactiveErrors();
            errors.Add(new InactiveNetSynchronizerException());

            return errors;
        }

        public override string GetSuccessMessage(SyncType syncAction, SyncDirection direction)
        {
            return string.Format("Network {0} is successful with local center {1}", syncAction, this._urlUtils.GetEnpointUrl());
        }

        #endregion

        #region utility methods

        protected void WaitForEndProcess(Guid processid, Action<SynchronizationEvent> eventRiser, SyncType syncType, SyncDirection direction)
        {
            int percentage = 0;

            while (percentage != 100)
            {
                if (this.stopRequested.WaitOne(100))
                {
                    throw new SynchronizationException("network synchronization is canceled");
                    
                }
                Thread.Sleep(1000);
                percentage = this._requestProcessor.Process<int>(this._urlUtils.GetPushCheckStateUrl(processid), -1);
                if (percentage < 0)
                    throw new SynchronizationException("network synchronization is failed");

                eventRiser(new SynchronizationEvent(new SyncStatus(syncType, direction, percentage, null)));

                
            }
        }

        #endregion
    }
}
