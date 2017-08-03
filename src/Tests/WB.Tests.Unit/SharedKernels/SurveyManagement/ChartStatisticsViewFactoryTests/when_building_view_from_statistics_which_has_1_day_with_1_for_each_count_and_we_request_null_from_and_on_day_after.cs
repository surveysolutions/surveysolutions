using System;
using System.Collections.Generic;
using Machine.Specifications;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ChartStatisticsViewFactoryTests
{
    public class when_building_view_from_statistics_which_has_1_day_with_1_for_each_count_and_we_request_null_from_and_on_day_after : ChartStatisticsViewFactoryTestsContext
    {
        [OneTimeSetUp]
        public void Establish()
        {
            var statistics = CreateStatisticsGroupedByDateAndTemplate(new Dictionary<DateTime, QuestionnaireStatisticsForChart>
            {
                {
                    new DateTime(2014, 8, 21),
                    CreateQuestionnaireStatisticsForChartWithSameCountForAllStatuses(count: 1)
                },
            });

            input = new ChartStatisticsInputModel
            {
                CurrentDate = new DateTime(2014, 8, 22),
                QuestionnaireId = Guid.NewGuid(),
                QuestionnaireVersion = 1,
                From = null,
                To = new DateTime(2014, 8, 22),
            };

            chartStatisticsViewFactory = CreateChartStatisticsViewFactory(statistics: statistics);
            Because();
        }

        public void Because() => view = chartStatisticsViewFactory.Load(input);

        [Test]
        public void should_return_8_lines_the_same_as_statuses_count() =>
            view.Lines.Length.ShouldEqual(8);

        [Test]
        public void should_set_1st_point_horizontal_coord_of_all_lines_equal_to_2014_08_20() =>
            view.Lines.ShouldEachConformTo(line => (string)line[0][0] == "2014-08-20");

        [Test]
        public void should_set_2nd_point_horizontal_coord_of_all_lines_equal_to_2014_08_21() =>
            view.Lines.ShouldEachConformTo(line => (string)line[1][0] == "2014-08-21");

        [Test]
        public void should_set_3rd_point_horizontal_coord_of_all_lines_equal_to_2014_08_22() =>
            view.Lines.ShouldEachConformTo(line => (string)line[2][0] == "2014-08-22");

        [Test]
        public void should_set_1st_point_vertical_size_of_all_lines_equal_to_0_as_starting_day_with_no_data() =>
            view.Lines.ShouldEachConformTo(line => (int) line[0][1] == 0);

        [Test]
        public void should_set_2nd_point_vertical_size_of_all_lines_equal_to_0_as_starting_day_with_no_data() =>
            view.Lines.ShouldEachConformTo(line => (int) line[1][1] == 1);

        [Test]
        public void should_set_3rd_point_vertical_size_of_all_lines_equal_to_1() =>
            view.Lines.ShouldEachConformTo(line => (int) line[2][1] == 1);
        
        private static ChartStatisticsViewFactory chartStatisticsViewFactory;
        private static ChartStatisticsInputModel input;
        private static ChartStatisticsView view;
    }
}