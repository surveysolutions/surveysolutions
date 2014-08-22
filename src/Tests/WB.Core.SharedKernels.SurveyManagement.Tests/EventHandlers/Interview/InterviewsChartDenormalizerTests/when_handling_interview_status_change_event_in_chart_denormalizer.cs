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
    internal class when_handling_interview_status_change_event_in_chart_denormalizer : InterviewsChartDenormalizerTestContext
    {
        Establish context = () =>
        {
            var interviewDetailsForChart = Mock.Of<InterviewDetailsForChart>(i => i.QuestionnaireId == questionnaireId &&
                i.QuestionnaireVersion == 1 &&
                i.Status == InterviewStatus.ApprovedBySupervisor);

            interviewBriefStorage =
                Mock.Of<IReadSideRepositoryWriter<InterviewDetailsForChart>>(x => x.GetById(interviewId.FormatGuid()) == interviewDetailsForChart);
            
            var statisticsMock = Mock.Of<StatisticsLineGroupedByDateAndTemplate>(i => i.QuestionnaireId == questionnaireId &&
                i.ApprovedBySupervisorCount == approvedBySupervisorCount);

            statisticsStorage = new Mock<IReadSideRepositoryWriter<StatisticsLineGroupedByDateAndTemplate>>();
            statisticsStorage.Setup(x => x.GetById(Moq.It.IsAny<string>())).Returns(statisticsMock);

            statisticsStorage.Setup(x => x.Store(Moq.It.IsAny<StatisticsLineGroupedByDateAndTemplate>(), Moq.It.IsAny<string>()))
                .Callback((StatisticsLineGroupedByDateAndTemplate stats, string id) => statistics = stats);

            denormalizer = CreateStatisticsDenormalizer(interviewBriefStorage: interviewBriefStorage,
                statisticsStorage: statisticsStorage.Object);

            evnt = CreateInterviewStatusChangedEvent(InterviewStatus.ApprovedByHeadquarters, interviewId);
        };

        Because of = () =>
            denormalizer.Handle(evnt);

        It should_statistics_storage_stores_new_state = () =>
            statisticsStorage.Verify(x => x.Store(Moq.It.IsAny<StatisticsLineGroupedByDateAndTemplate>(), Moq.It.IsAny<string>()),
                Times.Once);

        It should_statistics_approved_by_headquarters_count_equals_1 = () =>
            statistics.ApprovedByHeadquartersCount.ShouldEqual(1);

        It should_statistics_approved_by_supervisor_count_equals_1 = () =>
            statistics.ApprovedBySupervisorCount.ShouldEqual(1);

        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static Guid interviewId = Guid.Parse("22222222222222222222222222222222");
        private static Guid responsibleId = Guid.Parse("32222222222222222222222222222222");

        private static StatisticsLineGroupedByDateAndTemplate statistics;
        private static IReadSideRepositoryWriter<InterviewDetailsForChart> interviewBriefStorage;
        private static int approvedBySupervisorCount = 1;
        private static Mock<IReadSideRepositoryWriter<StatisticsLineGroupedByDateAndTemplate>> statisticsStorage;
        private static InterviewsChartDenormalizer denormalizer;
        private static IPublishedEvent<InterviewStatusChanged> evnt;
    }
}