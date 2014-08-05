using System;
using System.Linq;
using Machine.Specifications;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.Interview.StatisticsDenormalizerTests
{
    internal class when_duplicate_creation_event_exists_in_event_stream : StatisticsDenormalizerTestContext
    {
        Establish context = () =>
        {
            statisticsStorage = new TestInMemoryWriter<StatisticsLineGroupedByUserAndTemplate>();
            interviewBriefStorage = new TestInMemoryWriter<InterviewBrief>();

            denormalizer = CreateStatisticsDenormalizer(statisticsStorage: statisticsStorage,
                interviewBriefStorage:interviewBriefStorage);

            duplicatedCreateEvent = ToPublishedEvent(new InterviewOnClientCreated(Guid.NewGuid(), Guid.NewGuid(), 1), Guid.NewGuid());

            denormalizer.Handle(duplicatedCreateEvent);
        };

        Because of = () => denormalizer.Handle(duplicatedCreateEvent);

        It should_not_duplicate_count_of_interviews = () => statisticsStorage.Dictionary.First().Value.CreatedCount.ShouldEqual(1);

        private static StatisticsDenormalizer denormalizer;
        private static TestInMemoryWriter<StatisticsLineGroupedByUserAndTemplate> statisticsStorage;
        private static IPublishedEvent<InterviewOnClientCreated> duplicatedCreateEvent;
        private static TestInMemoryWriter<InterviewBrief> interviewBriefStorage;
    }
}