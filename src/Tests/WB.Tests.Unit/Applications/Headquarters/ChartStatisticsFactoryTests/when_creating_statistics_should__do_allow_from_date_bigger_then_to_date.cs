using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviews;

namespace WB.Tests.Unit.Applications.Headquarters.ChartStatisticsFactoryTests
{
    internal class when_creating_statistics_should__do_allow_from_date_bigger_then_to_date : ChartStatisticsFactoryTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.NewGuid();
            var baseDate = new DateTime(2014, 8, 22);
            var questionnaireVersion = 1;

            var data = new List<StatisticsLineGroupedByDateAndTemplate>
            {
                new StatisticsLineGroupedByDateAndTemplate
                {
                    QuestionnaireId = questionnaireId,
                    QuestionnaireVersion = questionnaireVersion,
                    Date = baseDate,
                    DateTicks = baseDate.Ticks
                }
            }.AsQueryable();
            
            chartStatisticsViewFactory = CreateChartStatisticsFactory(data);

            input = new ChartStatisticsInputModel
            {
                CurrentDate = baseDate,
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = questionnaireVersion,
                From = baseDate.AddDays(-1),
                To = baseDate.AddDays(-2)
            };
        };

        Because of = () => view = chartStatisticsViewFactory.Load(input);

        It should_have_days_count_muliply_two_records = () => view.Ticks.Length.ShouldEqual(0);

        private static ChartStatisticsViewFactory chartStatisticsViewFactory;
        private static ChartStatisticsInputModel input;
        private static ChartStatisticsView view;
    }
}