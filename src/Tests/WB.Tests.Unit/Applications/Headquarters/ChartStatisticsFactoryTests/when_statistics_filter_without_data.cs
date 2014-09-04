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
    internal class when_statistics_filter_without_data : ChartStatisticsFactoryTestsContext
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
                    Date = baseDate.AddDays(-3),
                    DateTicks = baseDate.AddDays(-3).Ticks,
                    ApprovedByHeadquartersCount = 1,
                    ApprovedBySupervisorCount = 1,
                    CompletedCount = 1,
                    InterviewerAssignedCount = 1,
                    RejectedByHeadquartersCount = 1,
                    RejectedBySupervisorCount = 1,
                    SupervisorAssignedCount = 1
                }
            }.AsQueryable();

            chartStatisticsViewFactory = CreateChartStatisticsFactory(data);

            input = new ChartStatisticsInputModel
            {
                CurrentDate = baseDate,
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = questionnaireVersion,
                From = baseDate.AddDays(-2),
                To = baseDate.AddDays(-1)
            };
        };

        Because of = () => view = chartStatisticsViewFactory.Load(input);

        It should_have_days_count_muliply_two_records = () => view.Ticks.Length.ShouldEqual(2 * 2);

        It should_have_supervisorAssignedData_correct = () => view.Stats[0].ShouldEqual(new[] { 1, 1 });

        private static ChartStatisticsViewFactory chartStatisticsViewFactory;
        private static ChartStatisticsInputModel input;
        private static ChartStatisticsView view;
    }
}