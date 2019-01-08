using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;
using WB.Tests.Abc;

namespace WB.Tests.Integration.ReportTests.ChartStatisticsViewFactoryTests
{
    internal class when_statistics_filter_from_date_bigger_then_to_date : ChartStatisticsViewFactoryTestsContext
    {
        [OneTimeSetUp]
        public void Establish()
        {
            var qid = Create.Entity.QuestionnaireIdentity();
            baseDate = new DateTime(2014, 8, 22);

            CreateQuestionnaireStatisticsForChartWithSameCountForAllStatuses(qid, baseDate, 7);

            chartStatisticsViewFactory = CreateChartStatisticsViewFactory();

            input = new ChartStatisticsInputModel
            {
                CurrentDate = baseDate,
                QuestionnaireName = qid.Id,
                QuestionnaireVersion = qid.Version,
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
            view.DataSets.Count.Should().Be(0);

        private ChartStatisticsViewFactory chartStatisticsViewFactory;
        private ChartStatisticsInputModel input;
        private ChartStatisticsView view;
        private DateTime baseDate;
    }
}
