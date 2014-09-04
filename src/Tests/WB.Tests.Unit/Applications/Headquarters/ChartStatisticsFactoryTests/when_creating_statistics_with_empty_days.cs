using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviews;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Headquarters.ChartStatisticsFactoryTests
{
    internal class when_creating_statistics_with_empty_days : ChartStatisticsFactoryTestsContext
    {
        Establish context = () =>
        {
            var stats = Mock.Of<IQueryableReadSideRepositoryReader<StatisticsLineGroupedByDateAndTemplate>>();

            var questionnaireId = Guid.NewGuid();
            var baseDate = new DateTime(2014, 8, 22);
            var questionnaireVersion = 1;

            var data = new List<StatisticsLineGroupedByDateAndTemplate>
            {
                new StatisticsLineGroupedByDateAndTemplate
                {
                    QuestionnaireId = questionnaireId,
                    QuestionnaireVersion = questionnaireVersion,
                    Date = baseDate.AddDays(-2),
                    DateTicks = baseDate.AddDays(-2).Ticks,
                    ApprovedByHeadquartersCount = 1,
                    ApprovedBySupervisorCount = 1,
                    CompletedCount = 1,
                    InterviewerAssignedCount = 1,
                    RejectedByHeadquartersCount = 1,
                    RejectedBySupervisorCount = 1,
                    SupervisorAssignedCount = 1
                },
                new StatisticsLineGroupedByDateAndTemplate
                {
                    QuestionnaireId = questionnaireId,
                    QuestionnaireVersion = questionnaireVersion,
                    Date = baseDate.AddDays(0),
                    DateTicks = baseDate.Date.Ticks,
                    ApprovedByHeadquartersCount = 2,
                    ApprovedBySupervisorCount = 2,
                    CompletedCount = 2,
                    InterviewerAssignedCount = 2,
                    RejectedByHeadquartersCount = 2,
                    RejectedBySupervisorCount = 2,
                    SupervisorAssignedCount = 2
                }
            }.AsQueryable();

            chartStatisticsViewFactory = CreateChartStatisticsFactory(data);

            input = new ChartStatisticsInputModel
            {
                CurrentDate = baseDate,
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = questionnaireVersion,
                From = baseDate.AddDays(-100),
                To = baseDate.AddDays(100)
            };
        };

        Because of = () => view = chartStatisticsViewFactory.Load(input);

        It should_have_days_count_muliply_two_records = () => view.Ticks.Length.ShouldEqual(3*2);

        It should_have_0_rownumber_equals_1 = () => view.Ticks[0, 0].ShouldEqual("1");
        It should_have_0_date_correct = () => view.Ticks[0, 1].ShouldEqual(new DateTime(2014, 8, 20).ToShortDateString());

        It should_have_1_rownumber_equals_2 = () => view.Ticks[1, 0].ShouldEqual("2");
        It should_have_1_date_correct = () => view.Ticks[1, 1].ShouldEqual(new DateTime(2014, 8, 21).ToShortDateString());

        It should_have_2_rownumber_equals_3 = () => view.Ticks[2, 0].ShouldEqual("3");
        It should_have_2_date_correct = () => view.Ticks[2, 1].ShouldEqual(new DateTime(2014, 8, 22).ToShortDateString());

        It should_have_supervisorAssignedData_correct = () => view.Stats[0].ShouldEqual(new[] { 1, 1, 2 });
        It should_have_interviewerAssignedData_correct = () => view.Stats[1].ShouldEqual(new[] { 1, 1, 2 });
        It should_have_completedData_correct = () => view.Stats[2].ShouldEqual(new[] { 1, 1, 2 });
        It should_have_rejectedBySupervisor_correct = () => view.Stats[3].ShouldEqual(new[] { 1, 1, 2 });
        It should_have_approvedBySupervisor_correct = () => view.Stats[4].ShouldEqual(new[] { 1, 1, 2 });
        It should_have_rejectedByHeadquarters_correct = () => view.Stats[5].ShouldEqual(new[] { 1, 1, 2 });
        It should_have_approvedByHeadquarters_correct = () => view.Stats[6].ShouldEqual(new[] { 1, 1, 2 });

        private static ChartStatisticsViewFactory chartStatisticsViewFactory;
        private static ChartStatisticsInputModel input;
        private static ChartStatisticsView view;
    }
}