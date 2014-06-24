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

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.Interview.StatisticsDenormalizerTests
{
    internal class when_handling_interview_status_change_event : StatisticsDenormalizerTestContext
    {
        private Establish context = () =>
        {
            var interviewBriefMock = Mock.Of<InterviewBrief>(i => i.QuestionnaireId == questionnaireId && 
                                                                  i.QuestionnaireVersion == 1 && 
                                                                  i.ResponsibleId == responsibleId &&
                                                                  i.Status == InterviewStatus.ApprovedBySupervisor);

            interviewBriefStorage = Mock.Of<IReadSideRepositoryWriter<InterviewBrief>>(x => x.GetById(interviewId.FormatGuid()) == interviewBriefMock);
            var statisticsMock = Mock.Of<StatisticsLineGroupedByUserAndTemplate>(i => i.QuestionnaireId == questionnaireId &&
                                                                                      i.ApprovedBySupervisorCount == approvedBySupervisorCount);

            statisticsStorage = new Mock<IReadSideRepositoryWriter<StatisticsLineGroupedByUserAndTemplate>>();
            statisticsStorage.Setup(x => x.GetById(Moq.It.IsAny<string>())).Returns(statisticsMock);

            statisticsStorage.Setup(x => x.Store(Moq.It.IsAny<StatisticsLineGroupedByUserAndTemplate>(), Moq.It.IsAny<string>()))
                             .Callback((StatisticsLineGroupedByUserAndTemplate stats, string id) => statistics = stats);
            
            denormalizer = CreateStatisticsDenormalizer(interviewBriefStorage : interviewBriefStorage,
                                                        statisticsStorage: statisticsStorage.Object);

            evnt = CreateInterviewStatusChangedEvent(InterviewStatus.ApprovedByHeadquarters, interviewId);
        };

        private Because of = () =>
            denormalizer.Handle(evnt);

        private It should_statistics_storage_stores_new_state = () =>
            statisticsStorage.Verify(x => x.Store(Moq.It.IsAny<StatisticsLineGroupedByUserAndTemplate>(), Moq.It.IsAny<string>()), Times.Once);

        private It should_statistics_approved_by_headquarters_count_equals_1 = () =>
            statistics.ApprovedByHeadquartersCount.ShouldEqual(1);

        private It should_statistics_approved_by_supervisor_count_equals_0 = () =>
            statistics.ApprovedBySupervisorCount.ShouldEqual(0);
        
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static Guid interviewId = Guid.Parse("22222222222222222222222222222222");
        private static Guid responsibleId = Guid.Parse("32222222222222222222222222222222");

        private static StatisticsLineGroupedByUserAndTemplate statistics;
        private static IReadSideRepositoryWriter<InterviewBrief> interviewBriefStorage;
        private static int approvedBySupervisorCount = 1;
        private static Mock<IReadSideRepositoryWriter<StatisticsLineGroupedByUserAndTemplate>> statisticsStorage;
        private static StatisticsDenormalizer denormalizer;
        private static IPublishedEvent<InterviewStatusChanged> evnt;

    }
}
