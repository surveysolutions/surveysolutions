using System;
using Machine.Specifications;
using Main.DenormalizerStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Integration.InterviewStatistics
{
    internal class when_chart_statistics_is_in_chess_order : InterviewChartDenormalizerTestContext
    {
        Establish context = () =>
        {
            interviewDetailsStorage = new InMemoryReadSideRepositoryAccessor<InterviewDetailsForChart>();
            statisticsStorage = new InMemoryReadSideRepositoryAccessor<StatisticsGroupedByDateAndTemplate>();
            denormalizer = CreateInterviewsChartDenormalizer(interviewDetailsStorage, statisticsStorage);
        };

        Because of = () =>
        {
            // Create two interviews
            denormalizer.Handle(Create.InterviewCreatedEvent(interviewAId, day0, userId, questionnaireId, questionnaireVersion));
            denormalizer.Handle(Create.InterviewCreatedEvent(interviewBId, day0, userId, questionnaireId, questionnaireVersion));

            // Run first interview
            denormalizer.Handle(Create.InterviewStatusChanged(interviewAId, day1, InterviewStatus.SupervisorAssigned));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewAId, day3, InterviewStatus.InterviewerAssigned));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewAId, day5, InterviewStatus.Completed));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewAId, day7, InterviewStatus.Restarted));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewAId, day9, InterviewStatus.ApprovedBySupervisor));

            //Run second interview
            denormalizer.Handle(Create.InterviewStatusChanged(interviewBId, day2, InterviewStatus.SupervisorAssigned));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewBId, day4, InterviewStatus.InterviewerAssigned));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewBId, day6, InterviewStatus.Completed));
            denormalizer.Handle(Create.InterviewStatusChanged(interviewBId, day8, InterviewStatus.RejectedBySupervisor));
        };

        It should_set_specific_valuest_for_day1 = () =>
            GetStatisticsPerDay(day1).ShouldContainValues(
                createdCount: 2,
                supervisorAssignedCount: 2,
                interviewerAssignedCount: 0,
                completedCount: 0,
                approvedBySupervisorCount: 0,
                rejectedBySupervisorCount: 0,
                approvedByHeadquartersCount: 0,
                rejectedByHeadquartersCount: 0);

        private static QuestionnaireStatisticsForChart GetStatisticsPerDay(DateTime day)
        {
            return statisticsStorage.GetById(storageKey).StatisticsByDate[day.Date];
        }

        private static InterviewsChartDenormalizer denormalizer;
        private static Guid questionnaireId = Guid.Parse("33333333333333333333333333333330");
        private static int questionnaireVersion = 1;
        private static Guid interviewAId = Guid.Parse("22222222222222222222222222222222");
        private static Guid interviewBId = Guid.Parse("33333333333333333333333333333333");
        private static DateTime day0 = new DateTime(2014, 8, 30, 12, 48, 35);
        private static DateTime day1 = new DateTime(2014, 9, 1, 12, 48, 35);
        private static DateTime day2 = new DateTime(2014, 9, 2, 12, 48, 35);
        private static DateTime day3 = new DateTime(2014, 9, 3, 12, 48, 35);
        private static DateTime day4 = new DateTime(2014, 9, 4, 12, 48, 35);
        private static DateTime day5 = new DateTime(2014, 9, 5, 12, 48, 35);
        private static DateTime day6 = new DateTime(2014, 9, 6, 12, 48, 35);
        private static DateTime day7 = new DateTime(2014, 9, 7, 12, 48, 35);
        private static DateTime day8 = new DateTime(2014, 9, 8, 12, 48, 35);
        private static DateTime day9 = new DateTime(2014, 9, 9, 12, 48, 35);
        private static DateTime day10 = new DateTime(2014, 9, 10, 12, 48, 35);
        private static Guid userId = Guid.Parse("11111111111111111111111111111111");
        private static IReadSideRepositoryWriter<InterviewDetailsForChart> interviewDetailsStorage;
        private static InMemoryReadSideRepositoryAccessor<StatisticsGroupedByDateAndTemplate> statisticsStorage;

        private static string storageKey = String.Format("{0}_{1}$",
            questionnaireId,
            questionnaireVersion.ToString().PadLeft(3, '_'));
    }
}
