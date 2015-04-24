using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization
{
    public abstract class HeadquartersSynchronizationContext
    {
        private readonly IPlainKeyValueStorage<SynchronizationStatus> statusStorage;
        private readonly List<string> synchronizationMessages = new List<string>();
        private readonly List<string> synchronizationErrors = new List<string>();

        protected abstract string StorageDocumentId { get; }

        protected HeadquartersSynchronizationContext(IPlainKeyValueStorage<SynchronizationStatus> statusStorage)
        {
            this.statusStorage = statusStorage;
        }

        public virtual bool IsRunning { get; private set; }

        public void Start()
        {
            this.IsRunning = true;
            this.synchronizationMessages.Clear();
            this.synchronizationErrors.Clear();
            this.PushMessage("Synchronization started");

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
            this.PushMessage("Synchronization done");

            var status = this.statusStorage.GetById(this.StorageDocumentId) ?? new SynchronizationStatus();
            status.IsRunning = false;
            if (status.IsHqReachable.HasValue && status.IsHqReachable.Value)
            {
                status.FinishedAt = DateTime.Now;
                status.FinishedAtUtc = DateTime.UtcNow;
            }
            status.ErrorsCount = this.synchronizationErrors.Count;
            this.statusStorage.Store(status, this.StorageDocumentId);
        }

        public virtual void PushMessage(string message)
        {
            this.synchronizationMessages.Add(DateTime.Now + ": " + message);
        }

        public virtual void PushMessageFormat(string message, params object[] args)
        {
            this.PushMessage(string.Format(message, args));
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

        public void MarkHqAsReachable()
        {
            var status = this.statusStorage.GetById(this.StorageDocumentId) ?? new SynchronizationStatus();
            status.IsHqReachable = true;
            this.statusStorage.Store(status, this.StorageDocumentId);
        }

        public void MarkHqAsUnReachable()
        {
            var status = this.statusStorage.GetById(this.StorageDocumentId) ?? new SynchronizationStatus();
            status.IsHqReachable = false;
            this.statusStorage.Store(status, this.StorageDocumentId);
        }
    }
}