using System;
using Synchronization.Core.Interface;
using Synchronization.Core.Events;

namespace Synchronization.Core.SynchronizationFlow
{
    public abstract class AbstractSynchronizer : ISynchronizer
    {
        protected readonly ISettingsProvider SettingsProvider;

        public AbstractSynchronizer(ISettingsProvider clientSettingsprovider)
        {
            this.SettingsProvider = clientSettingsprovider;
        }

        #region Abstract and Virtual

        protected abstract void OnPush(SyncDirection direction);
        protected abstract void OnPushSupervisorCapi(SyncDirection direction);
        protected abstract void OnPull(SyncDirection direction);
        protected abstract void OnStop();

        // The event-invoking method that derived classes can override.
        protected virtual void OnSyncProgressChanged(SynchronizationEvent e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<SynchronizationEvent> handler = SyncProgressChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion

        #region Implementation of ISynchronizer

        public event EventHandler<SynchronizationEvent> SyncProgressChanged;

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

        public void PushSupervisorCAPI(SyncDirection direction)
        {
            try
            {
                OnPushSupervisorCapi(direction);
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

        public abstract string BuildSuccessMessage(SyncType syncAction, SyncDirection direction);

        #endregion
    }
}
