using System;
using System.Threading;
using System.Collections.Generic;
using Synchronization.Core.Interface;
using Synchronization.Core.Events;
using Synchronization.Core.Errors;
using Common.Utils;

namespace Synchronization.Core.SynchronizationFlow
{
    public abstract class AbstractSynchronizer : ISynchronizer
    {
        #region variables

        private ManualResetEvent stopRequested = new ManualResetEvent(false);
        private readonly IRequestProcessor requestProcessor;
        
        /// <summary>
        /// Synchronization process identifier
        /// </summary>
        private Guid syncProcessId;

        #endregion

        #region C-tor

        protected AbstractSynchronizer(ISettingsProvider clientSettingsprovider, IRequestProcessor requestProcessor, IUrlUtils urlUtils)
        {
            this.SettingsProvider = clientSettingsprovider;
            this.UrlUtils = urlUtils;

            this.requestProcessor = requestProcessor;
        }

        #endregion

        #region Properties

        protected IUrlUtils UrlUtils { get; private set; }
        protected ISettingsProvider SettingsProvider { get; private set; }

        #endregion

        #region Helpers

        private bool IsCancelled { get { return this.stopRequested.WaitOne(100); } }

        private IList<ServiceException> GetInactiveErrors()
        {
            return OnGetInactiveErrors();
        }

        protected T ProcessWebRequest<T>(string url, T defaultValue)
        {
            return this.requestProcessor.Process<T>(url, defaultValue);
        }

        #endregion

        #region Abstract and Virtual

        protected abstract Guid OnPush(SyncDirection direction);
        protected abstract Guid OnPull(SyncDirection direction);
        protected abstract IList<ServiceException> OnCheckSyncIssues(SyncType syncAction, SyncDirection direction);

                
        /// <summary>
        /// Interrupt pending loading
        /// </summary>
        protected virtual void OnStop()
        {
            if (this.syncProcessId == Guid.Empty /*no sync process*/ || IsCancelled /*already cancelled*/)
                return;

            var endProcess = ProcessWebRequest<bool>(UrlUtils.GetEndProcessUrl(this.syncProcessId), false);
            if (endProcess)
                this.stopRequested.Set();
        }

        // The event-invoking method that derived classes can override.
        protected virtual void OnSyncProgressChanged(SynchronizationEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<SynchronizationEventArgs> handler = SyncProgressChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected abstract bool OnUpdateStatus();

        protected virtual IList<ServiceException> OnGetInactiveErrors()
        {
            return new List<ServiceException>();
        }

        #endregion

        #region utility methods

        protected void WaitForEndProcess(Action<SynchronizationEventArgs> eventRiser, SyncType syncType, SyncDirection direction)
        {
            int percentage = 0;
            var url = UrlUtils.GetPushCheckStateUrl(this.syncProcessId);

            while (percentage != 100)
            {
                Thread.Sleep(1000);

                if (IsCancelled)
                    throw new CancelledServiceException("Synchronization is cancelled");

                percentage = ProcessWebRequest<int>(url, -1);
                if (percentage < 0)
                    throw new SynchronizationException("Synchronization is failed");

                eventRiser(new SynchronizationEventArgs(new SyncStatus(syncType, direction, percentage / 2 + 50, null)));
            }
        }

        #endregion

        #region Implementation of ISynchronizer

        public event EventHandler<SynchronizationEventArgs> SyncProgressChanged;

        public bool IsActive { get; private set; }

        public Guid Push(SyncDirection direction)
        {
            try
            {
                this.syncProcessId = Guid.Empty;

                this.stopRequested.Reset();

                this.syncProcessId = OnPush(direction);

                WaitForEndProcess(OnSyncProgressChanged, SyncType.Push, direction);

                return this.syncProcessId;
            }
            catch(Exception e)
            {
                this.syncProcessId = Guid.Empty;
                throw new SynchronizationException(
                    string.Format("Push to local center {0} is failed ", UrlUtils.GetEnpointUrl()), e);
            }
        }

        public Guid Pull(SyncDirection direction)
        {
            try
            {
                this.syncProcessId = Guid.Empty;

                this.stopRequested.Reset();

                this.syncProcessId = OnPull(direction);

                WaitForEndProcess(OnSyncProgressChanged, SyncType.Pull, direction);
            
                return this.syncProcessId;
            }
            catch (Exception e)
            {
                this.syncProcessId = Guid.Empty;

                throw new SynchronizationException(
                   string.Format("Pull from local center {0} is failed ", UrlUtils.GetEnpointUrl()), e);
            }
        }

        public void Stop()
        {
            try
            {
                OnStop();
            }
            catch
            {
                throw;
            }
            finally
            {
                this.syncProcessId = Guid.Empty;
            }
        }

        public void UpdateStatus()
        {
            IsActive = OnUpdateStatus();
        }

        public abstract string GetSuccessMessage(SyncType syncAction, SyncDirection direction);

        public IList<ServiceException> CheckSyncIssues(SyncType syncAction, SyncDirection direction)
        {
            if (!IsActive)
                return GetInactiveErrors();

            return OnCheckSyncIssues(syncAction, direction);
        }

        #endregion
    }
}
