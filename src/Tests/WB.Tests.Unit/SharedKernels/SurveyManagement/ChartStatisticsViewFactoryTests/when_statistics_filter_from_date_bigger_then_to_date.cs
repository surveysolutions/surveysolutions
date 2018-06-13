using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ChartStatisticsViewFactoryTests
{
    internal class when_statistics_filter_from_date_bigger_then_to_date : ChartStatisticsViewFactoryTestsContext
    {
        [OneTimeSetUp]
        public void Establish()
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

            chartStatisticsViewFactory = CreateChartStatisticsViewFactory(statistics: data);

            input = new ChartStatisticsInputModel
            {
                CurrentDate = baseDate,
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = 1,
                From = baseDate.AddDays(-1),
                To = baseDate.AddDays(-2)
            };
            Because();
        }

        public void Because() => view = chartStatisticsViewFactory.Load(input);

        [Test]
        public void should_have_from_be_equal_to_formated_date_from_input() =>
            view.From.Should().Be(input.To.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

        [Test]
        public void should_have_to_be_equal_to_formated_date_to_input() =>
            view.To.Should().Be(input.From.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

        [Test]
        public void should_have_0_lines() =>
            view.Lines.Length.Should().Be(0);

        private static ChartStatisticsViewFactory chartStatisticsViewFactory;
        private static ChartStatisticsInputModel input;
        private static ChartStatisticsView view;
        private static DateTime baseDate;
    }
}
