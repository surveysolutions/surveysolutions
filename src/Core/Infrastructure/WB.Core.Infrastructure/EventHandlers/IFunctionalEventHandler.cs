using System.Collections.Generic;
using Ncqrs.Eventing.ServiceModel.Bus;

namespace WB.Core.Infrastructure.EventHandlers
{
    public interface IFunctionalEventHandler
    {
        void Handle(IEnumerable<IPublishableEvent> publishableEvents);
    }
}
