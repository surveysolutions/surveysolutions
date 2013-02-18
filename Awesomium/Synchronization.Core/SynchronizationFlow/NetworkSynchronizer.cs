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
        public NetworkSynchronizer(ISettingsProvider settingsprovider, IRequestProcessor requestProcessor, IUrlUtils urlUtils)
            : base(settingsprovider, requestProcessor, urlUtils)
        {
        }

        #region Overrides of AbstractSynchronizer

        protected override Guid OnPush(SyncDirection direction)
        {
            return ProcessWebRequest<Guid>(this.UrlUtils.GetPushUrl(this.SettingsProvider.Settings.ClientId), default(Guid));
        }

        protected override Guid OnPull(SyncDirection direction)
        {
            return ProcessWebRequest<Guid>(this.UrlUtils.GetPullUrl(this.SettingsProvider.Settings.ClientId), default(Guid));
        }

        protected override IList<ServiceException> OnCheckSyncIssues(SyncType syncType, SyncDirection direction)
        {
            ServiceException e = null;
            try
            {
                var netEndPoint = this.UrlUtils.GetEnpointUrl();

                // test if there is connection to synchronization endpoint
                if (ProcessWebRequest<string>(netEndPoint, "False") == "False")
                    e = new NetUnreachableException(netEndPoint);
            }
            catch (Exception ex)
            {
                e = new NetIssueException(ex);
            }

            return e == null ? null : new List<ServiceException>() { e };
        }

        protected override bool OnUpdateStatus()
        {
            return !string.IsNullOrEmpty(this.UrlUtils.GetEnpointUrl());
        }

        protected override IList<ServiceException> OnGetInactiveErrors()
        {
            var errors = base.OnGetInactiveErrors();
            errors.Add(new EndpointNotSetException());

            return errors;
        }

        public override string GetSuccessMessage(SyncType syncAction, SyncDirection direction)
        {
            return string.Format("Network synchronization {0} is successful with {1}", syncAction, this.UrlUtils.GetEnpointUrl());
        }

        #endregion
    }
}
