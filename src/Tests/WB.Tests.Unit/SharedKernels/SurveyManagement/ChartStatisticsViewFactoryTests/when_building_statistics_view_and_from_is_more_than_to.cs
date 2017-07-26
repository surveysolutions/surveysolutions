using System;
using Machine.Specifications;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ChartStatisticsViewFactoryTests
{
    public class when_building_statistics_view_and_from_is_more_than_to : ChartStatisticsViewFactoryTestsContext
    {
        [OneTimeSetUp]
        public void Establish ()
        {
            input = new ChartStatisticsInputModel
            {
                CurrentDate = new DateTime(2013, 1, 1),
                From = new DateTime(2014, 1, 1),
                To = new DateTime(2012, 1, 1),
            };

            chartStatisticsViewFactory = CreateChartStatisticsViewFactory();
            Because();
        }

        public void Because () => view = chartStatisticsViewFactory.Load(input);

        [Test]
        public void should_return_empty_result() =>
            view.Lines.ShouldBeEmpty();

        private static ChartStatisticsViewFactory chartStatisticsViewFactory;
        private static ChartStatisticsInputModel input;
        private static ChartStatisticsView view;
    }
}