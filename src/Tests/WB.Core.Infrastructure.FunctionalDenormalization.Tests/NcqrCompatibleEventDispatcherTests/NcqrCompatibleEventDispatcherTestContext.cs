using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.EventDispatcher;

namespace WB.Core.Infrastructure.FunctionalDenormalization.Tests.NcqrCompatibleEventDispatcherTests
{
    [Subject(typeof(NcqrCompatibleEventDispatcher))]
    internal class NcqrCompatibleEventDispatcherTestContext
    {
        protected static NcqrCompatibleEventDispatcher CreateNcqrCompatibleEventDispatcher()
        {
            return new NcqrCompatibleEventDispatcher();
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
