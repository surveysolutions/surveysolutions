using System;
using System.Collections.Generic;
using System.Reflection;
using Ncqrs.Eventing;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    public interface IViewModelEventRegistry
    {
        void Subscribe(IViewModelEventHandler handler, string aggregateRootId = null);

        void Subscribe(IViewModelEventHandler handler, string aggregateRootId, Identity entityIdentity);

        void Unsubscribe(IViewModelEventHandler handler);

        bool IsSubscribed(IViewModelEventHandler handler);

        IReadOnlyCollection<IViewModelEventHandler> GetViewModelsByEvent(CommittedEvent @event);

        void RemoveAggregateRoot(string aggregateRootId);

        MethodInfo GetViewModelHandleMethod(Type viewModelType, Type eventType);

        void Reset();
        void WriteToLogInfoBySubscribers();
    }
}
