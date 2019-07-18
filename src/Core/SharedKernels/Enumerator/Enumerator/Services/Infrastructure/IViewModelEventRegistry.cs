using System;
using System.Collections.Generic;
using Ncqrs.Eventing;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    public interface IViewModelEventRegistry
    {
        void Subscribe(IViewModelEventHandler handler, string aggregateRootId = null);

        void Unsubscribe(IViewModelEventHandler handler);

        bool IsSubscribed(IViewModelEventHandler handler);

        IReadOnlyCollection<IViewModelEventHandler> GetViewModelsByEvent(CommittedEvent @event);

        void RemoveAggregateRoot(string aggregateRootId);

        bool IsAsyncViewModelHandleMethod(Type viewModelType, Type eventType);
    }
}
