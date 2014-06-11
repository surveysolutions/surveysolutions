using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.SharedKernels.SurveyManagement.Synchronization.Schedulers
{
    public abstract class SynchorizationContext
    {
        private readonly IPlainStorageAccessor<SynchronizationStatus> statusStorage;
        private readonly List<string> synchronizationMessages = new List<string>();
        private readonly List<string> synchronizationErrors = new List<string>();

        public virtual bool IsRunning { get; private set; }
        protected abstract string StorageDocumentId { get; }

        protected SynchorizationContext(IPlainStorageAccessor<SynchronizationStatus> statusStorage)
        {
            this.statusStorage = statusStorage;
        }

        public void Start()
        {
            this.IsRunning = true;
            this.synchronizationMessages.Clear();
            this.synchronizationErrors.Clear();

            var status = new SynchronizationStatus
            {
                IsRunning = true,
                StartedAt = DateTime.Now,
                StartedAtUtc = DateTime.UtcNow,
            };
            this.statusStorage.Store(status, this.StorageDocumentId);
        }

        public void Stop()
        {
            this.IsRunning = false;

            var status = this.statusStorage.GetById(this.StorageDocumentId) ?? new SynchronizationStatus();
            status.IsRunning = false;

            status.FinishedAt = DateTime.Now;
            status.FinishedAtUtc = DateTime.UtcNow;

            status.ErrorsCount = this.synchronizationErrors.Count;
            this.statusStorage.Store(status, this.StorageDocumentId);
        }

        public virtual void PushMessage(string message)
        {
            this.synchronizationMessages.Add(DateTime.Now + ": " + message);
        }

        public virtual void PushError(string message)
        {
            this.synchronizationErrors.Add(message);
        }

        public virtual string GetStatus()
        {
            return this.synchronizationMessages.LastOrDefault();
        }

        public virtual IReadOnlyCollection<string> GetMessages()
        {
            return new ReadOnlyCollection<string>(this.synchronizationMessages);
        }

        public virtual IReadOnlyCollection<string> GetErrors()
        {
            return new ReadOnlyCollection<string>(this.synchronizationErrors);
        }

        public SynchronizationStatus GetPersistedStatus()
        {
            return this.statusStorage.GetById(this.StorageDocumentId);
        }
    }
}