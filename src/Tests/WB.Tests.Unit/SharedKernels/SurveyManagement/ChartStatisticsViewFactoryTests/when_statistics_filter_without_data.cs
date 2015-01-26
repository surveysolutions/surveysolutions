using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviews;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ChartStatisticsViewFactoryTests
{
    internal class when_statistics_filter_without_data : ChartStatisticsViewFactoryTestsContext
    {
        Establish context = () =>
        {
            var stats = Mock.Of<IQueryableReadSideRepositoryReader<StatisticsLineGroupedByDateAndTemplate>>();

            var questionnaireId = Guid.NewGuid();
            baseDate = new DateTime(2014, 8, 22);
            var questionnaireVersion = 1;

            var data =
                new StatisticsGroupedByDateAndTemplate
                {
                    StatisticsByDate =
                        new Dictionary<DateTime, QuestionnaireStatisticsForChart>()
                        {
                            {
                                baseDate.AddDays(-3),
                                new QuestionnaireStatisticsForChart()
                                {
                                    ApprovedByHeadquartersCount = 1,
                                    ApprovedBySupervisorCount = 1,
                                    CompletedCount = 1,
                                    InterviewerAssignedCount = 1, 
                                    RejectedByHeadquartersCount = 1,
                                    RejectedBySupervisorCount = 1,
                                    SupervisorAssignedCount = 1
                                }
                            }
                        }
                };

            chartStatisticsViewFactory = CreateChartStatisticsViewFactory(
                Mock.Of<IReadSideKeyValueStorage<StatisticsGroupedByDateAndTemplate>>(_
                    => _.GetById(Moq.It.IsAny<string>()) == data));

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

        It should_have_7_lines = () => view.Lines.Length.ShouldEqual(7);

        It should_each_line_has_2_days_inside = () => view.Lines.ShouldEachConformTo(line => line.Length == 2);

        It should_each_line_has_first_record_equal_to_from_date_and_with_count_equal_to_1 = () => view.Lines.ShouldEachConformTo(line => line[0][0].ToString() == baseDate.AddDays(-2).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture) && (int)line[0][1]==1);

        It should_each_line_has_second_record_equal_to_to_date_and_with_count_equal_to_1 = () => view.Lines.ShouldEachConformTo(line => line[1][0].ToString() == baseDate.AddDays(-1).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture) && (int)line[1][1] == 1);

        private static ChartStatisticsViewFactory chartStatisticsViewFactory;
        private static ChartStatisticsInputModel input;
        private static ChartStatisticsView view;
        private static DateTime baseDate;
    }
}
