using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Enumerator.Native.WebInterview.LifeCycle
{
    public class InterviewLifecycle
    {
        public const int RefreshEntitiesLimit = 100;

        public class LifecycleAction
        {
            public HashSet<Identity> RefreshEntities { get; } = new HashSet<Identity>();
            public bool RefreshFilteredOptions { get; set; } = false;
            public HashSet<Identity> RefreshRemovedEntities { get; } = new HashSet<Identity>();
            public bool ReloadInterview { get; set; } = false;
            public bool FinishInterview { get; set; } = false;
        }

        public InterviewLifecycle RefreshRemovedEntities(Guid aggId, IEnumerable<Identity> identities)
        {
            var lifecycleAction = Store.GetOrAdd(aggId, a => new LifecycleAction());
            foreach (var identity in identities)
            {
                lifecycleAction.RefreshRemovedEntities.Add(identity);
            }

            return this;
        }

        public InterviewLifecycle RefreshEntities(Guid aggId, IEnumerable<Identity> keys)
        {
            var entities = Store.GetOrAdd(aggId, a => new LifecycleAction());

            foreach (var key in keys)
                entities.RefreshEntities.Add(key);

            return this;
        }

        public InterviewLifecycle RefreshEntities(Guid aggId, Identity id)
        {
            var entities = Store.GetOrAdd(aggId, a => new LifecycleAction());
            entities.RefreshEntities.Add(id);
            return this;
        }

        public InterviewLifecycle RefreshEntitiesWithFilteredOptions(Guid aggId)
        {
            var entities = Store.GetOrAdd(aggId, a => new LifecycleAction());
            entities.RefreshFilteredOptions = true;
            return this;
        }

        public ConcurrentDictionary<Guid, LifecycleAction> Store { get; } = new ConcurrentDictionary<Guid, LifecycleAction>();
        
        public InterviewLifecycle ReloadInterview(Guid aggId)
        {
            var entities = Store.GetOrAdd(aggId, a => new LifecycleAction());
            entities.ReloadInterview = true;
            return this;
        }

        public InterviewLifecycle FinishInterview(Guid aggId)
        {
            var entities = Store.GetOrAdd(aggId, a => new LifecycleAction());
            entities.FinishInterview = true;
            return this;
        }

        public bool HasAlreadyTooMuchRefreshEntities(Guid id)
        {
            var store = Store.GetOrAdd(id, a => new LifecycleAction());
            return store.RefreshEntities.Count > RefreshEntitiesLimit;
        }
    }
}
