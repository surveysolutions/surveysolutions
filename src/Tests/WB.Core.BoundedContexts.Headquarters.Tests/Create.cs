using System;
using Ncqrs.Eventing.ServiceModel.Bus;
using NSubstitute;
using WB.Core.BoundedContexts.Headquarters.Authentication;

namespace WB.Core.BoundedContexts.Headquarters.Tests
{
    internal static class Create
    {
        internal static IPublishedEvent<T> PublishedEvent<T>(T @event = null, Guid? eventSourceId = null) where T : class
        {
            var publishedEvent = Substitute.For<IPublishedEvent<T>>();
            publishedEvent.Payload.Returns(@event);
            publishedEvent.EventSourceId.Returns(eventSourceId ?? Guid.Parse("1234567890abcdef0101010102020304"));
            publishedEvent.EventSequence.Returns(1);

            return publishedEvent;
        }

        internal static CustomPasswordValidator CustomPasswordValidator(int minPasswordLength = 10, string pattern = ".*")
        {
            return new CustomPasswordValidator(minPasswordLength, pattern);
        }
    }
}