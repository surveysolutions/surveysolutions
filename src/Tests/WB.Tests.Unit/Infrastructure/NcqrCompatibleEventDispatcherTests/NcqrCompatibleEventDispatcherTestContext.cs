using System;
using System.Collections.Generic;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Infrastructure.NcqrCompatibleEventDispatcherTests
{
    [NUnit.Framework.TestOf(typeof(NcqrCompatibleEventDispatcher))]
    internal class NcqrCompatibleEventDispatcherTestContext
    {
        protected static NcqrCompatibleEventDispatcher CreateNcqrCompatibleEventDispatcher(EventBusSettings eventBusSettings = null)
        {
            return Create.Service.NcqrCompatibleEventDispatcher(eventBusSettings: eventBusSettings);
        }

        protected static IPublishableEvent CreatePublishableEvent(Guid? eventSourceId = null)
        {
            return Create.Fake.PublishableEvent(eventSourceId);
        }

        protected static IEnumerable<IPublishableEvent> CreatePublishableEvents(int countOfEvents, Guid? eventSourceId = null)
        {
            for (int i = 0; i < countOfEvents; i++)
            {
                yield return CreatePublishableEvent(eventSourceId ?? Guid.NewGuid());
            }
        }
    }
}
