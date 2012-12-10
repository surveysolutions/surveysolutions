using System;
using System.Collections.Generic;
using Synchronization.Core.Interface;
using Synchronization.Core.Events;
using Synchronization.Core.Errors;

namespace Synchronization.Core.SynchronizationFlow
{
    public abstract class AbstractSynchronizer : ISynchronizer
    {
        #region C-tor

        protected AbstractSynchronizer(ISettingsProvider clientSettingsprovider)
        {
            this.SettingsProvider = clientSettingsprovider;
        }

        #endregion

        #region Properties

        protected ISettingsProvider SettingsProvider { get; private set; }

        /// <summary>
        /// Synchronization process identifier
        /// </summary>
        protected Guid SyncProcessId { get; set; }

        #endregion

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

        public bool IsActive { get; private set; }

        public Guid Push(SyncDirection direction)
        {
            try
            {
                SyncProcessId = Guid.Empty;
                
                OnPush(direction);

                return SyncProcessId;
            }
            catch
            {
                SyncProcessId = Guid.Empty;
                throw;
            }
        }

        public Guid Pull(SyncDirection direction)
        {
            try
            {
                SyncProcessId = Guid.Empty;
                
                OnPull(direction);

                return SyncProcessId;
            }
            catch
            {
                SyncProcessId = Guid.Empty;
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
            finally
            {
                SyncProcessId = Guid.Empty;
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
