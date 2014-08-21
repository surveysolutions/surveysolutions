using System;
using System.Linq;
using Machine.Specifications;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.Interview.InterviewsChartDenormalizerTests
{
    internal class when_duplicate_creation_event_exists_in_event_stream_in_chart_denormalizer : InterviewsChartDenormalizerTestContext
    {
        private Establish context = () =>
        {
            statisticsStorage = new TestInMemoryWriter<StatisticsLineGroupedByDateAndTemplate>();
            interviewBriefStorage = new TestInMemoryWriter<InterviewDetailsForChart>();

            denormalizer = CreateStatisticsDenormalizer(statisticsStorage, interviewBriefStorage);

            duplicatedCreateEvent = ToPublishedEvent(new InterviewOnClientCreated(Guid.NewGuid(), Guid.NewGuid(), 1), Guid.NewGuid());

            denormalizer.Handle(duplicatedCreateEvent);
        };

        Because of = () => denormalizer.Handle(duplicatedCreateEvent);

        It should_not_duplicate_count_of_interviews = () => statisticsStorage.Dictionary.Count().ShouldEqual(1);

        private static InterviewsChartDenormalizer denormalizer;
        private static TestInMemoryWriter<StatisticsLineGroupedByDateAndTemplate> statisticsStorage;
        private static IPublishedEvent<InterviewOnClientCreated> duplicatedCreateEvent;
        private static TestInMemoryWriter<InterviewDetailsForChart> interviewBriefStorage;
    }
}