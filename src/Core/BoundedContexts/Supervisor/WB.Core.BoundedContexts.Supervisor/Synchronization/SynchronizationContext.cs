using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization
{
    public class SynchronizationContext
    {
        private const string StoreStatusKey = "SynchronizationStatus";
        private readonly IPlainStorageAccessor<SynchronizationStatus> statusStorage;
        private List<string> synchronizationMessages = new List<string>();
        private List<string> synchronizationErrors = new List<string>();

        public SynchronizationContext(IPlainStorageAccessor<SynchronizationStatus> statusStorage)
        {
            if (statusStorage == null) throw new ArgumentNullException("statusStorage");
            this.statusStorage = statusStorage;
        }

        public bool IsRunning { get; private set; }

        public void Start()
        {
            IsRunning = true;
            synchronizationMessages.Clear();
            synchronizationErrors.Clear();
            this.PushMessage("Synchronization started");

            var status = new SynchronizationStatus
            {
                IsRunning = true,
                StartedAt = DateTime.Now,
                StartedAtUtc = DateTime.UtcNow,
            };
            this.statusStorage.Store(status, StoreStatusKey);
        }

        public void Stop()
        {
            IsRunning = false;
            this.PushMessage("Synchronization done");

            var status = this.statusStorage.GetById(StoreStatusKey) ?? new SynchronizationStatus();
            status.IsRunning = false;
            status.FinisedAt = DateTime.Now;
            status.FinishedAtUtc = DateTime.UtcNow;
            status.ErrorsCount = this.synchronizationErrors.Count;
            this.statusStorage.Store(status, StoreStatusKey);
        }

        public void PushMessage(string message)
        {
            synchronizationMessages.Add(message);
        }

        public void PushError(string message)
        {
            synchronizationErrors.Add(message);
        }

        public string GetStatus()
        {
            return synchronizationMessages.LastOrDefault();
        }

        public IReadOnlyCollection<string> GetMessages()
        {
            return new ReadOnlyCollection<string>(this.synchronizationMessages);
        }

        public IReadOnlyCollection<string> GetErrors()
        {
            return new ReadOnlyCollection<string>(this.synchronizationErrors);
        }

        public SynchronizationStatus GetPersistedStatus()
        {
            return this.statusStorage.GetById(StoreStatusKey);
        }

        public void MarkHqAsReachable()
        {
            var status = this.statusStorage.GetById(StoreStatusKey) ?? new SynchronizationStatus();
            status.IsHqReachable = true;
            this.statusStorage.Store(status, StoreStatusKey);
        }

        public void MarkHqAsUnReachable()
        {
            var status = this.statusStorage.GetById(StoreStatusKey) ?? new SynchronizationStatus();
            status.IsHqReachable = false;
            this.statusStorage.Store(status, StoreStatusKey);
        }
    }
}