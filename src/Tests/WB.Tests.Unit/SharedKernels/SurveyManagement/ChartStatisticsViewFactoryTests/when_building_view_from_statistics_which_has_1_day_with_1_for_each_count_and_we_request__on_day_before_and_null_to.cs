using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviews;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ChartStatisticsViewFactoryTests
{
    internal class when_building_view_from_statistics_which_has_1_day_with_1_for_each_count_and_we_request__on_day_before_and_null_to : ChartStatisticsViewFactoryTestsContext
    {
        Establish context = () =>
        {
            var statistics = CreateStatisticsGroupedByDateAndTemplate(new Dictionary<DateTime, QuestionnaireStatisticsForChart>
            {
                {
                    new DateTime(2014, 8, 21),
                    CreateQuestionnaireStatisticsForChartWithSameCountForAllStatuses(count: 1)
                },
            });

            var statsStorage = Mock.Of<IReadSideKeyValueStorage<StatisticsGroupedByDateAndTemplate>>(_
                => _.GetById(Moq.It.IsAny<string>()) == statistics);

            input = new ChartStatisticsInputModel
            {
                CurrentDate = new DateTime(2014, 8, 22),
                QuestionnaireId = Guid.NewGuid(),
                QuestionnaireVersion = 1,
                From = new DateTime(2014, 8, 20),
                To = null,
            };

            chartStatisticsViewFactory = CreateChartStatisticsViewFactory(statsStorage);
        };

        Because of = () => view = chartStatisticsViewFactory.Load(input);

        It should_return_7_lines_the_same_as_statuses_count = () =>
            view.Lines.Length.ShouldEqual(7);

        It should_set_1st_point_horizontal_coord_of_all_lines_equal_to_2014_08_19 = () =>
            view.Lines.ShouldEachConformTo(line => (string)line[0][0] == "08/20/2014");

        It should_set_2nd_point_horizontal_coord_of_all_lines_equal_to_2014_08_20 = () =>
            view.Lines.ShouldEachConformTo(line => (string)line[1][0] == "08/21/2014");

        
        It should_set_1st_point_vertical_size_of_all_lines_equal_to_0_as_starting_day_with_no_data = () =>
            view.Lines.ShouldEachConformTo(line => (int) line[0][1] == 0);

        It should_set_2nd_point_vertical_size_of_all_lines_equal_to_0_as_starting_day_with_no_data = () =>
            view.Lines.ShouldEachConformTo(line => (int) line[1][1] == 1);

        
        private static ChartStatisticsViewFactory chartStatisticsViewFactory;
        private static ChartStatisticsInputModel input;
        private static ChartStatisticsView view;
    }
}