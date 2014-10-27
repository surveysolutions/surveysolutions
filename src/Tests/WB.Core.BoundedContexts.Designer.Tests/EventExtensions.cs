using System;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;

namespace WB.Core.BoundedContexts.Designer.Tests
{
    internal static class EventExtensions
    {
        public static IPublishedEvent<T> ToPublishedEvent<T>(this T @event)
            where T : class
        {
            return ToPublishedEvent<T>(@event, Guid.NewGuid());
        }

        public static IPublishedEvent<T> ToPublishedEvent<T>(this T @event, Guid eventSourceId)
            where T : class
        {
            return Mock.Of<IPublishedEvent<T>>(publishedEvent
                => publishedEvent.Payload == @event &&
                    publishedEvent.EventSourceId == eventSourceId);
        }
    }
}