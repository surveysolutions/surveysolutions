using System;
using System.Collections.Generic;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventHandlers;

namespace WB.Core.Infrastructure.Implementation.EventDispatcher
{
    public interface IDenormalizerRegistry
    {
        void RegisterFunctional<T>() where T : IFunctionalEventHandler;
        void Register<T>() where T : IEventHandler;
        IReadOnlyCollection<Type> FunctionalDenormalizers { get; }
        IReadOnlyCollection<Type> SequentialDenormalizers { get; }
        EventHandlerMethod HandlerMethod(Type denormalizer, Type eventType);
        bool CanHandleEvent(Type denormalizerType, IPublishableEvent evnt);
    }
}