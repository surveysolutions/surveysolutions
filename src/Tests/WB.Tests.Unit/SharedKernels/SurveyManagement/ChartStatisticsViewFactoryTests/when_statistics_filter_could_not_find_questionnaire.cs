using System;
using Machine.Specifications;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ChartStatisticsViewFactoryTests
{
    public class when_statistics_filter_could_not_find_questionnaire : ChartStatisticsViewFactoryTestsContext
    {
        [OneTimeSetUp]
        public void Establish()
        {
            var questionnaireId = Guid.Empty;
            var questionnaireVersion = 1;

            baseDate = new DateTime(2014, 8, 22);

            chartStatisticsViewFactory = CreateChartStatisticsViewFactory(statistics: null);

            input = new ChartStatisticsInputModel
            {
                CurrentDate = baseDate,
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = questionnaireVersion,
                From = baseDate.AddDays(-2),
                To = baseDate
            };
            Because();
        }

        public void Because() => view = chartStatisticsViewFactory.Load(input);

        [Test]
        public void should_have_0_lines() => view.Lines.Length.ShouldEqual(0);

        private static ChartStatisticsViewFactory chartStatisticsViewFactory;
        private static ChartStatisticsInputModel input;
        private static ChartStatisticsView view;
        private static DateTime baseDate;
    }
}