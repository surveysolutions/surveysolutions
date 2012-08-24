using System;
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

        private readonly string _host;
        private readonly IRequesProcessor _requestProcessor;
        private readonly string _pushAdress;
        private readonly string _pullAdress;
        private readonly string _pushCheckStateAdress;
        private readonly string _endPointAdressAdress;

        #endregion

        public NetworkSynchronizer(ISettingsProvider settingsprovider, IRequesProcessor requestProcessor, string host, string pushAdress, string pullAdress, string pushCheckStateAdress, string endPointAdressAdress)
            : base(settingsprovider)
        {
            this._host = host;
            this._endPointAdressAdress = endPointAdressAdress;
            this._pushAdress = pushAdress;
            this._pullAdress = pullAdress;
            this._pushCheckStateAdress = pushCheckStateAdress;
            this._requestProcessor = requestProcessor;
        }

        public string Host
        {
            get { return _host; }
        }

        protected string PushAdress
        {
            get { return string.Format("{0}{1}?url={2}&syncKey={3}", _host, _pushAdress, _endPointAdressAdress, this.SettingsProvider.Settings.ClientId); }
        }

        protected string PullAdress
        {
            get { return string.Format("{0}{1}?url={2}&syncKey={3}", _host, _pullAdress, _endPointAdressAdress, this.SettingsProvider.Settings.ClientId); }
        }

        protected Uri PushCheckStateAdress
        {
            get { return new Uri(_host + _pushCheckStateAdress); }
        }

        #region Overrides of AbstractSynchronizer

        protected override void OnPush(SyncDirection direction)
        {
            try
            {
                var processGuid = this._requestProcessor.Process<Guid>(PushAdress);
                WaitForEndProcess(processGuid, OnSyncProgressChanged, SyncType.Push, direction);
               
            }
            catch (Exception e)
            {
                throw new SynchronizationException(
                    string.Format("Push to local center {0} is failed ", this._endPointAdressAdress), e);
            }
        }

        protected override void OnPull(SyncDirection direction)
        {
            try
            {
                var processGuid = this._requestProcessor.Process<Guid>(PullAdress);
                WaitForEndProcess(processGuid, OnSyncProgressChanged, SyncType.Pull, direction);
            }
            catch (Exception e)
            {
                throw new SynchronizationException(
                   string.Format("Pull to local center {0} is failed ", this._endPointAdressAdress), e);
            }
        }

        protected override void OnStop()
        {
            //throw new NotImplementedException();
        }

        #endregion

        #region utility methods

        protected void WaitForEndProcess(Guid processid, Action<SynchronizationEvent> eventRiser, SyncType syncType, SyncDirection direction)
        {
            int percentage = 0;

            while (percentage != 100)
            {
                percentage = this._requestProcessor.Process<int>(string.Format("{0}?id={1}", PushCheckStateAdress, processid));
                if (percentage < 0)
                    throw new SynchronizationException("network synchronization is failed");

                eventRiser(new SynchronizationEvent(new SyncStatus(syncType, direction, percentage, null)));
            
                Thread.Sleep(1000);
            }

        }

        #endregion

        public override string BuildSuccessMessage(SyncType syncAction, SyncDirection direction)
        {
            return string.Format("Network {0} is successful with local center {1}", syncAction, Host);
        }
    }
}
