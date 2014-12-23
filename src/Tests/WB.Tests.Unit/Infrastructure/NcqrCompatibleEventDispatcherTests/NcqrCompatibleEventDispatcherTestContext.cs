using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.Implementation.EventDispatcher;

namespace WB.Tests.Unit.Infrastructure.NcqrCompatibleEventDispatcherTests
{
    [Subject(typeof(NcqrCompatibleEventDispatcher))]
    internal class NcqrCompatibleEventDispatcherTestContext
    {
        protected static NcqrCompatibleEventDispatcher CreateNcqrCompatibleEventDispatcher(Type[] handlersToIgnore = null)
        {
            return new NcqrCompatibleEventDispatcher(Mock.Of<IEventStore>(), handlersToIgnore ?? new Type[0]);
        }

        protected static IPublishableEvent CreatePublishableEvent(Guid eventSourceId)
        {
            return Mock.Of<IPublishableEvent>(_ => _.Payload == new object() && _.EventSourceId == eventSourceId);
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
