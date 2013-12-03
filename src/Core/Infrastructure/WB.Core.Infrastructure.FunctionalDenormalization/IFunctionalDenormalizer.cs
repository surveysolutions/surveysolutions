using System;
using Ncqrs.Eventing.ServiceModel.Bus;

namespace WB.Core.Infrastructure.FunctionalDenormalization
{
    public interface IFunctionalDenormalizer : IEventHandler
    {
        void Handle(IPublishableEvent evt);
        void RegisterHandlersInOldFashionNcqrsBus(InProcessEventBus oldEventBus);
        void ChangeForSingleEventSource(Guid eventSourceId);
        void FlushDataToPersistentStorage(Guid eventSourceId);
    }
}