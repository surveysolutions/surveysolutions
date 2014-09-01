using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.Interview.InterviewsChartDenormalizerTests
{
    internal class when_handling_series_of_events_in_chart_denormalizer : InterviewsChartDenormalizerTestContext
    {
        private Establish context = () =>
        {
            var interviewDetailsForChart = new InterviewDetailsForChart
            {
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = 1,
                Status = InterviewStatus.ApprovedBySupervisor
            };

            var interviewDetailsStorage =
                Mock.Of<IReadSideRepositoryWriter<InterviewDetailsForChart>>(
                    x => x.GetById(interviewId.FormatGuid()) == interviewDetailsForChart);

            var questionnaireDetailsForChart = new QuestionnaireDetailsForChart
            {
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = 1
            };

            var questionnaireDetailsStorage =
                Mock.Of<IReadSideRepositoryWriter<QuestionnaireDetailsForChart>>(x => x.GetById(Moq.It.IsAny<string>()) == questionnaireDetailsForChart);

            var statisticsMock = new StatisticsLineGroupedByDateAndTemplate
            {
                QuestionnaireId = questionnaireId,
                ApprovedBySupervisorCount = 1
            };

            statisticsStorage = new Mock<IReadSideRepositoryWriter<StatisticsLineGroupedByDateAndTemplate>>();
            statisticsStorage.Setup(x => x.GetById(Moq.It.IsAny<string>())).Returns(statisticsMock);
            statisticsStorage.Setup(x => x.Store(Moq.It.IsAny<StatisticsLineGroupedByDateAndTemplate>(), Moq.It.IsAny<string>()))
                .Callback((StatisticsLineGroupedByDateAndTemplate stats, string id) => statistics = stats);

            denormalizer = CreateStatisticsDenormalizer(statisticsStorage.Object, interviewDetailsStorage, questionnaireDetailsStorage);

            evnt1 = CreateInterviewCreatedEvent(questionnaireId, questionnaireId, 1, interviewId);
            evnt2 = CreateInterviewStatusChangedEvent(InterviewStatus.InterviewerAssigned, interviewId);
            evnt3 = CreateInterviewStatusChangedEvent(InterviewStatus.ApprovedByHeadquarters, interviewId);
        };

        Because of = () =>
            HandleAll(evnt1, evnt2, evnt3);

        It should_statistics_storage_stores_new_state = () =>
            statisticsStorage.Verify(x => x.Store(Moq.It.IsAny<StatisticsLineGroupedByDateAndTemplate>(), Moq.It.IsAny<string>()),
                Times.Exactly(2));

        It should_statistics_approved_by_headquarters_count_equals_1 = () =>
            statistics.InterviewerAssignedCount.ShouldEqual(0);

        It should_statistics_approved_by_supervisor_count_equals_1 = () =>
            statistics.ApprovedByHeadquartersCount.ShouldEqual(1);

        private static void HandleAll(IPublishedEvent<InterviewCreated> evnt1,
            IPublishedEvent<InterviewStatusChanged> evnt2,
            IPublishedEvent<InterviewStatusChanged> evnt3)
        {
            denormalizer.Handle(evnt1);
            denormalizer.Handle(evnt2);
            denormalizer.Handle(evnt3);
        }

        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static Guid interviewId = Guid.Parse("22222222222222222222222222222222");

        private static StatisticsLineGroupedByDateAndTemplate statistics;
        private static Mock<IReadSideRepositoryWriter<StatisticsLineGroupedByDateAndTemplate>> statisticsStorage;
        private static InterviewsChartDenormalizer denormalizer;

        private static IPublishedEvent<InterviewCreated> evnt1;
        private static IPublishedEvent<InterviewStatusChanged> evnt2;
        private static IPublishedEvent<InterviewStatusChanged> evnt3;
    }
}