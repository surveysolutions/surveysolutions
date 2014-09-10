using System;
using System.Linq;
using Machine.Specifications;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviews;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ChartStatisticsViewFactoryTests
{
    internal class when_building_statistics_view_and_from_is_more_than_to : ChartStatisticsViewFactoryTestsContext
    {
        Establish context = () =>
        {
            input = new ChartStatisticsInputModel
            {
                CurrentDate = new DateTime(2013, 1, 1),
                From = new DateTime(2014, 1, 1),
                To = new DateTime(2012, 1, 1),
            };

            chartStatisticsViewFactory = CreateChartStatisticsViewFactory();
        };

        Because of = () => view = chartStatisticsViewFactory.Load(input);

        It should_return_empty_result = () =>
            view.Lines.ShouldBeEmpty();

        private static ChartStatisticsViewFactory chartStatisticsViewFactory;
        private static ChartStatisticsInputModel input;
        private static ChartStatisticsView view;
    }
}