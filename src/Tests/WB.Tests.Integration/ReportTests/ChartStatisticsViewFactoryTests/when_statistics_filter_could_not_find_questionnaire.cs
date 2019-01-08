using System;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Integration.ReportTests.ChartStatisticsViewFactoryTests
{
    public class when_statistics_filter_could_not_find_questionnaire : ChartStatisticsViewFactoryTestsContext
    {
        [OneTimeSetUp]
        public void Establish()
        {
            var questionnaireId = Guid.Empty;
            var questionnaireVersion = 1;

            baseDate = new DateTime(2014, 8, 22);

            chartStatisticsViewFactory = CreateChartStatisticsViewFactory();

            input = new ChartStatisticsInputModel
            {
                CurrentDate = baseDate,
                QuestionnaireName = questionnaireId.FormatGuid(),
                QuestionnaireVersion = questionnaireVersion,
                From = baseDate.AddDays(-2),
                To = baseDate
            };
            Because();
        }

        public void Because() => view = chartStatisticsViewFactory.Load(input);

        [Test]
        public void should_have_0_lines() => view.DataSets.Count.Should().Be(0);

        private ChartStatisticsViewFactory chartStatisticsViewFactory;
        private ChartStatisticsInputModel input;
        private ChartStatisticsView view;
        private DateTime baseDate;
    }
}
