using System;
using Machine.Specifications;
using Main.DenormalizerStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Integration.InterviewStatistics
{
    internal class when_chart_statistics_is_empty_and_a_lot_of_events_applying : InterviewChartDenormalizerTestContext
    {
        private Establish context = () =>
        {
            interviewDetailsStorage = new InMemoryReadSideRepositoryAccessor<InterviewDetailsForChart>();
            statisticsStorage = new InMemoryReadSideRepositoryAccessor<StatisticsGroupedByDateAndTemplate>();
            denormalizer = CreateInterviewsChartDenormalizer(interviewDetailsStorage, statisticsStorage);
        };

        Because of = () =>
        {
            //interviewAId
            denormalizer.Handle(Create.InterviewCreatedEvent(interviewAId, day1, userId, questionnaireId, questionnaireVersion));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewAId, day2, InterviewStatus.SupervisorAssigned));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewAId, day3, InterviewStatus.InterviewerAssigned));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewAId, day3, InterviewStatus.Completed));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewAId, day4, InterviewStatus.Restarted));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewAId, day5, InterviewStatus.Completed));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewAId, day5, InterviewStatus.RejectedBySupervisor));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewAId, day5, InterviewStatus.Completed));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewAId, day5, InterviewStatus.ApprovedBySupervisor));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewAId, day5, InterviewStatus.RejectedByHeadquarters));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewAId, day5, InterviewStatus.ApprovedBySupervisor));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewAId, day5, InterviewStatus.ApprovedByHeadquarters));

            //interviewBId
            denormalizer.Handle(Create.InterviewCreatedEvent(interviewBId, day2, userId, questionnaireId, questionnaireVersion));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewBId, day2, InterviewStatus.SupervisorAssigned));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewBId, day3, InterviewStatus.InterviewerAssigned));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewBId, day3, InterviewStatus.Completed));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewBId, day4, InterviewStatus.Restarted));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewBId, day4, InterviewStatus.Completed));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewBId, day4, InterviewStatus.RejectedBySupervisor));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewBId, day4, InterviewStatus.Completed));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewBId, day5, InterviewStatus.ApprovedBySupervisor));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewBId, day5, InterviewStatus.RejectedByHeadquarters));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewBId, day5, InterviewStatus.ApprovedBySupervisor));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewBId, day5, InterviewStatus.ApprovedByHeadquarters));

            //interviewCId
            denormalizer.Handle(Create.InterviewCreatedEvent(interviewCId, day3, userId, questionnaireId, questionnaireVersion));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewCId, day2, InterviewStatus.SupervisorAssigned));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewCId, day3, InterviewStatus.InterviewerAssigned));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewCId, day3, InterviewStatus.Completed));

            //interviewDId
            denormalizer.Handle(Create.InterviewCreatedEvent(interviewDId, day1, userId, questionnaireId, questionnaireVersion));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewDId, day1, InterviewStatus.SupervisorAssigned));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewDId, day1, InterviewStatus.InterviewerAssigned));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewDId, day1, InterviewStatus.Completed));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewDId, day4, InterviewStatus.ApprovedBySupervisor));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewDId, day5, InterviewStatus.ApprovedByHeadquarters));

            //interviewEId
            denormalizer.Handle(Create.InterviewCreatedEvent(interviewEId, day4, userId, questionnaireId, questionnaireVersion));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewEId, day4, InterviewStatus.SupervisorAssigned));

            //interviewFId
            denormalizer.Handle(Create.InterviewCreatedEvent(interviewFId, day4, userId, questionnaireId, questionnaireVersion));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewFId, day4, InterviewStatus.SupervisorAssigned));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewFId, day4, InterviewStatus.InterviewerAssigned));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewFId, day5, InterviewStatus.InterviewerAssigned));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewFId, day5, InterviewStatus.InterviewerAssigned));
        };

        It should_add_one_record_with_statistics = () =>
            statisticsStorage.GetById(storageKey).StatisticsByDate.Count.ShouldEqual(9);

        It should_set_specific_valuest_for_01_09_2014 = () => 
            GetStatisticsPerDay(day1).ShouldContainValues(
                createdCount: 1,
                supervisorAssignedCount: 0,
                interviewerAssignedCount: 0,
                completedCount: 1,
                approvedBySupervisorCount: 0,
                rejectedBySupervisorCount: 0,
                approvedByHeadquartersCount: 0,
                rejectedByHeadquartersCount: 0);

        private static QuestionnaireStatisticsForChart GetStatisticsPerDay(DateTime day)
        {
            return statisticsStorage.GetById(storageKey).StatisticsByDate[day.Date];
        }

        private static InterviewsChartDenormalizer denormalizer;
        private static Guid questionnaireId = Guid.Parse("33333333333333333333333333333333");
        private static int questionnaireVersion = 1;
        private static Guid interviewAId = Guid.Parse("22222222222222222222222222222222");
        private static Guid interviewBId = Guid.Parse("33333333333333333333333333333333");
        private static Guid interviewCId = Guid.Parse("44444444444444444444444444444444");
        private static Guid interviewDId = Guid.Parse("55555555555555555555555555555555");
        private static Guid interviewEId = Guid.Parse("66666666666666666666666666666666");
        private static Guid interviewFId = Guid.Parse("77777777777777777777777777777777");
        private static DateTime day1 = new DateTime(2014, 9, 1, 12, 48, 35);
        private static DateTime day2 = new DateTime(2014, 9, 2, 12, 48, 35);
        private static DateTime day3 = new DateTime(2014, 9, 4, 12, 48, 35);
        private static DateTime day4 = new DateTime(2014, 9, 5, 12, 48, 35);
        private static DateTime day5 = new DateTime(2014, 9, 9, 12, 48, 35);
        private static Guid userId = Guid.Parse("11111111111111111111111111111111");
        private static IReadSideRepositoryWriter<InterviewDetailsForChart> interviewDetailsStorage;
        private static InMemoryReadSideRepositoryAccessor<StatisticsGroupedByDateAndTemplate> statisticsStorage;

        private static string storageKey = String.Format("{0}_{1}$",
            questionnaireId,
            questionnaireVersion.ToString().PadLeft(3, '_'));
    }
}