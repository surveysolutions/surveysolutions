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

        private readonly IRequestProcessor _requestProcessor;
        private readonly IUrlUtils _urlUtils;
        private AutoResetEvent stopRequested = new AutoResetEvent(false);

        #endregion

        public NetworkSynchronizer(ISettingsProvider settingsprovider, IRequestProcessor requestProcessor, IUrlUtils urlUtils)
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
                this.stopRequested.Reset();

                SyncProcessId = this._requestProcessor.Process<Guid>(this._urlUtils.GetPushUrl(this.SettingsProvider.Settings.ClientId), default(Guid));

                WaitForEndProcess(OnSyncProgressChanged, SyncType.Push, direction);
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
                this.stopRequested.Reset();

                SyncProcessId = this._requestProcessor.Process<Guid>(this._urlUtils.GetPullUrl(this.SettingsProvider.Settings.ClientId), default(Guid));

                WaitForEndProcess(OnSyncProgressChanged, SyncType.Pull, direction);
            }
            catch (Exception e)
            {
                throw new SynchronizationException(
                   string.Format("Pull from local center {0} is failed ", this._urlUtils.GetEnpointUrl()), e);
            }
        }

        protected override void OnStop()
        {
            if (SyncProcessId == Guid.Empty || !this.stopRequested.WaitOne(100))
                return;

            var endProcess = this._requestProcessor.Process<bool>(this._urlUtils.GetEndProcessUrl(SyncProcessId), false);
            if (endProcess)
                this.stopRequested.Set();
        }

        protected override IList<ServiceException> OnCheckSyncIssues(SyncType syncType, SyncDirection direction)
        {
            ServiceException e = null;
            try
            {
                var netEndPoint = this._urlUtils.GetEnpointUrl();

                // test if there is connection to synchronization endpoint
                if (this._requestProcessor.Process<string>(netEndPoint, "False") == "False")
                    e = new NetUnreachableException(netEndPoint);
            }
            catch (Exception ex)
            {
                e = new NetUnreachableException(ex.Message);
            }

            return e == null ? null : new List<ServiceException>() { e };
        }

        protected override bool OnUpdateStatus()
        {
            return !string.IsNullOrEmpty(this._urlUtils.GetEnpointUrl());
        }

        protected override IList<ServiceException> OnGetInactiveErrors()
        {
            var errors = base.OnGetInactiveErrors();
            errors.Add(new InactiveNetServiceException());

            return errors;
        }

        public override string GetSuccessMessage(SyncType syncAction, SyncDirection direction)
        {
            return string.Format("Network {0} is successful with local center {1}", syncAction, this._urlUtils.GetEnpointUrl());
        }

        #endregion

        #region utility methods

        private void WaitForEndProcess(Action<SynchronizationEventArgs> eventRiser, SyncType syncType, SyncDirection direction)
        {
            int percentage = 0;

            while (percentage != 100)
            {
                Thread.Sleep(1000);

                if (this.stopRequested.WaitOne(100))
                    throw new CancelledServiceException("Network synchronization is cancelled");

                percentage = this._requestProcessor.Process<int>(this._urlUtils.GetPushCheckStateUrl(SyncProcessId), -1);
                if (percentage < 0)
                    throw new SynchronizationException("Network synchronization is failed");

                eventRiser(new SynchronizationEventArgs(new SyncStatus(syncType, direction, percentage, null)));
            }
        }

        #endregion
    }
}
