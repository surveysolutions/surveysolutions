using System;
using Machine.Specifications;
using Main.DenormalizerStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Integration.InterviewStatistics
{
    internal class when_chart_statistics_is_empty : InterviewChartDenormalizerTestContext
    {
        private Establish context = () =>
        {
            interviewDetailsStorage = new InMemoryReadSideRepositoryAccessor<InterviewDetailsForChart>();
            statisticsStorage = new InMemoryReadSideRepositoryAccessor<StatisticsGroupedByDateAndTemplate>();
            denormalizer = CreateInterviewsChartDenormalizer(interviewDetailsStorage, statisticsStorage);
        };

        private Because of = () =>
        {
            denormalizer.Handle(Create.InterviewCreatedEvent(interviewId, eventDate, userId, questionnaireId, questionnaireVersion));
        };

        private It should_add_one_record_with_statistics = () =>
            statisticsStorage.GetById(storageKey).StatisticsByDate.Count.ShouldEqual(1);

        private It should_set_1_to_CreatedCount_field_for_statistics_by_eventDate = () =>
            statisticsStorage.GetById(storageKey).StatisticsByDate[eventDate.Date].CreatedCount.ShouldEqual(1);

        private It should_set_0_to_SupervisorAssignedCount_field_for_statistics_by_eventDate = () =>
            statisticsStorage.GetById(storageKey).StatisticsByDate[eventDate.Date].SupervisorAssignedCount.ShouldEqual(0);

        private It should_set_0_to_InterviewerAssignedCount_field_for_statistics_by_eventDate = () =>
            statisticsStorage.GetById(storageKey).StatisticsByDate[eventDate.Date].InterviewerAssignedCount.ShouldEqual(0);

        private It should_set_0_to_CompletedCount_field_for_statistics_by_eventDate = () =>
            statisticsStorage.GetById(storageKey).StatisticsByDate[eventDate.Date].CompletedCount.ShouldEqual(0);

        private It should_set_0_to_ApprovedBySupervisorCount_field_for_statistics_by_eventDate = () =>
            statisticsStorage.GetById(storageKey).StatisticsByDate[eventDate.Date].ApprovedBySupervisorCount.ShouldEqual(0);

        private It should_set_0_to_RejectedBySupervisorCount_field_for_statistics_by_eventDate = () =>
            statisticsStorage.GetById(storageKey).StatisticsByDate[eventDate.Date].RejectedBySupervisorCount.ShouldEqual(0);

        private It should_set_0_to_ApprovedByHeadquartersCount_field_for_statistics_by_eventDate = () =>
            statisticsStorage.GetById(storageKey).StatisticsByDate[eventDate.Date].ApprovedByHeadquartersCount.ShouldEqual(0);

        private It should_set_0_to_RejectedByHeadquartersCount_field_for_statistics_by_eventDate = () =>
            statisticsStorage.GetById(storageKey).StatisticsByDate[eventDate.Date].RejectedByHeadquartersCount.ShouldEqual(0);

        private static InterviewsChartDenormalizer denormalizer;
        private static Guid questionnaireId = Guid.Parse("33333333333333333333333333333333");
        private static int questionnaireVersion = 1;

        private static string storageKey = String.Format("{0}_{1}$",
            questionnaireId,
            questionnaireVersion.ToString().PadLeft(3, '_'));

        private static Guid interviewId = Guid.Parse("22222222222222222222222222222222");
        private static DateTime eventDate = new DateTime(2014, 9, 1, 12, 48, 35);
        private static Guid userId = Guid.Parse("11111111111111111111111111111111");
        private static IReadSideKeyValueStorage<InterviewDetailsForChart> interviewDetailsStorage;
        private static InMemoryReadSideRepositoryAccessor<StatisticsGroupedByDateAndTemplate> statisticsStorage;
    }
}