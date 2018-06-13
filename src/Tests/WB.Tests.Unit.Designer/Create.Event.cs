using System;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;

namespace WB.Tests.Unit.Designer
{
    internal static partial class Create
    {
        public static IPublishedEvent<T> ToPublishedEvent<T>(this T @event,
                Guid? eventSourceId = null,
                string origin = null,
                DateTime? eventTimeStamp = null,
                Guid? eventId = null)
                where T : class, IEvent
        {
            var mock = new Mock<IPublishedEvent<T>>();
            var eventIdentifier = eventId ?? Guid.NewGuid();
            mock.Setup(x => x.Payload).Returns(@event);
            mock.Setup(x => x.EventSourceId).Returns(eventSourceId ?? Guid.NewGuid());
            mock.Setup(x => x.Origin).Returns(origin);
            mock.Setup(x => x.EventIdentifier).Returns(eventIdentifier);
            mock.Setup(x => x.EventTimeStamp).Returns((eventTimeStamp ?? DateTime.Now));
            var publishableEventMock = mock.As<IUncommittedEvent>();
            publishableEventMock.Setup(x => x.Payload).Returns(@event);
            return mock.Object;
        }
    }
}