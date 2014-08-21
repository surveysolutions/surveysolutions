using System;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.Interview.InterviewsChartDenormalizerTests
{
    internal class InterviewsChartDenormalizerTestContext
    {
        protected static InterviewsChartDenormalizer CreateStatisticsDenormalizer(
                                IReadSideRepositoryWriter<StatisticsLineGroupedByDateAndTemplate> statisticsStorage = null,
                                IReadSideRepositoryWriter<InterviewDetailsForChart> interviewBriefStorage = null)
        {
            return new InterviewsChartDenormalizer(
                statisticsStorage ?? Mock.Of<IReadSideRepositoryWriter<StatisticsLineGroupedByDateAndTemplate>>(),
                interviewBriefStorage ?? Mock.Of<IReadSideRepositoryWriter<InterviewDetailsForChart>>());
        }


        protected static IPublishedEvent<T> ToPublishedEvent<T>(T @event, Guid eventSourceId)
            where T : class
        {
            return Mock.Of<IPublishedEvent<T>>(publishedEvent
                => publishedEvent.Payload == @event
                && publishedEvent.EventSourceId == eventSourceId);
        }

        protected static IPublishedEvent<InterviewStatusChanged> CreateInterviewStatusChangedEvent(InterviewStatus status, Guid eventSourceId)
        {
            var evnt = ToPublishedEvent(new InterviewStatusChanged(status, String.Empty), eventSourceId);
            return evnt;
        }
    }
}
