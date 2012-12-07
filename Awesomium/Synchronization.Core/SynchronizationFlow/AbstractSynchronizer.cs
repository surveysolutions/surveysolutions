using System;
using System.Collections.Generic;
using Synchronization.Core.Interface;
using Synchronization.Core.Events;
using Synchronization.Core.Errors;

namespace Synchronization.Core.SynchronizationFlow
{
    public abstract class AbstractSynchronizer : ISynchronizer
    {
        protected readonly ISettingsProvider SettingsProvider;

        public AbstractSynchronizer(ISettingsProvider clientSettingsprovider)
        {
            this.SettingsProvider = clientSettingsprovider;
        }

        #region Helpers

        private IList<ServiceException> GetInactiveErrors()
        {
            return OnGetInactiveErrors();
        }

        #endregion

        #region Abstract and Virtual

        protected abstract void OnPush(SyncDirection direction);
        protected abstract void OnPull(SyncDirection direction);
        protected abstract void OnStop();
        protected abstract IList<ServiceException> OnCheckSyncIssues(SyncType syncAction, SyncDirection direction);
        
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

        #region Implementation of ISynchronizer

        public event EventHandler<SynchronizationEventArgs> SyncProgressChanged;

        public void Push(SyncDirection direction)
        {
            try
            {
                OnPush(direction);
            }
            catch
            {
                throw;
            }
        }

        public void Pull(SyncDirection direction)
        {
            try
            {
                OnPull(direction);
            }
            catch
            {
                throw;
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
        }

        public void UpdateStatus()
        {
            IsActive = OnUpdateStatus();
        }

        public bool IsActive { get; private set; }

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
