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
    internal class when_statistics_filter_from_date_bigger_then_to_date : ChartStatisticsViewFactoryTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.NewGuid();
            baseDate = new DateTime(2014, 8, 22);

            var data = CreateStatisticsGroupedByDateAndTemplate(new Dictionary<DateTime, QuestionnaireStatisticsForChart>
            {
                {
                    baseDate,
                    CreateQuestionnaireStatisticsForChartWithSameCountForAllStatuses(count: 7)
                }
            });

            chartStatisticsViewFactory = CreateChartStatisticsViewFactory(
                Mock.Of<IReadSideKeyValueStorage<StatisticsGroupedByDateAndTemplate>>(_
                    => _.GetById(Moq.It.IsAny<string>()) == data));

            input = new ChartStatisticsInputModel
            {
                CurrentDate = baseDate,
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = 1,
                From = baseDate.AddDays(-1),
                To = baseDate.AddDays(-2)
            };
        };

        Because of = () => view = chartStatisticsViewFactory.Load(input);

        It should_have_from_be_equal_to_formated_date_from_input = () => view.From.ShouldEqual(input.From.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture));

        It should_have_to_be_equal_to_formated_date_to_input = () => view.To.ShouldEqual(input.To.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture));

        It should_have_7_lines = () => view.Lines.Length.ShouldEqual(7);

        It should_each_line_has_1_day_inside = () => view.Lines.ShouldEachConformTo(line => line.Length == 1);

        It should_each_line_has_record_equal_to_from_date = () => view.Lines.ShouldEachConformTo(line => line[0][0].ToString() == baseDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture));

        private static ChartStatisticsViewFactory chartStatisticsViewFactory;
        private static ChartStatisticsInputModel input;
        private static ChartStatisticsView view;
        private static DateTime baseDate;
    }
}
