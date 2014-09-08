using System;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Tests.Integration.InterviewStatistics
{
    internal static class Create
    {
        private static IPublishedEvent<T> ToPublishedEvent<T>(T @event, Guid eventSourceId, DateTime date)
            where T : class
        {
            return Mock.Of<IPublishedEvent<T>>(publishedEvent
                => publishedEvent.Payload == @event 
                    && publishedEvent.EventSourceId == eventSourceId
                    && publishedEvent.EventTimeStamp == date);
        }

        public static IPublishedEvent<InterviewCreated> InterviewCreatedEvent(Guid eventSourceId, DateTime eventDate, Guid userId, Guid questionnaireId, long questionnaireVersion)
        {
            return ToPublishedEvent(new InterviewCreated(userId, questionnaireId, questionnaireVersion), eventSourceId, eventDate);
        }
        public static IPublishedEvent<InterviewFromPreloadedDataCreated> InterviewFromPreloadedDataCreatedEvent(Guid eventSourceId, DateTime eventDate, Guid userId, Guid questionnaireId, long questionnaireVersion)
        {
            return ToPublishedEvent(new InterviewFromPreloadedDataCreated(userId, questionnaireId, questionnaireVersion), eventSourceId, eventDate);
        }

        public static IPublishedEvent<InterviewOnClientCreated> InterviewOnClientCreatedEvent(Guid eventSourceId, DateTime eventDate, Guid userId, Guid questionnaireId, long questionnaireVersion)
        {
            return ToPublishedEvent(new InterviewOnClientCreated(userId, questionnaireId, questionnaireVersion), eventSourceId, eventDate);
        }

        public static IPublishedEvent<InterviewStatusChanged> InterviewStatusChanged(Guid eventSourceId, DateTime eventDate, InterviewStatus status)
        {
            return ToPublishedEvent(new InterviewStatusChanged(status, string.Empty), eventSourceId, eventDate);
        }
    }
}