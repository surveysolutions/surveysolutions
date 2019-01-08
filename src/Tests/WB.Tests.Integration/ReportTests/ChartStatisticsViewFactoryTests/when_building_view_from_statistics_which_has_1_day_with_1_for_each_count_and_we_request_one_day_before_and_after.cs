using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;
using WB.Tests.Abc;

namespace WB.Tests.Integration.ReportTests.ChartStatisticsViewFactoryTests
{
    public class when_building_view_from_statistics_which_has_1_day_with_1_for_each_count_and_we_request_two_days_before_and_one_after : ChartStatisticsViewFactoryTestsContext
    {
        [OneTimeSetUp]
        public void Establish()
        {
            var qid = Create.Entity.QuestionnaireIdentity();

            CreateQuestionnaireStatisticsForChartWithSameCountForAllStatuses(qid, new DateTime(2014, 8, 21), 1);

            input = new ChartStatisticsInputModel
            {
                CurrentDate = new DateTime(2014, 8, 22),
                QuestionnaireId = qid.QuestionnaireId,
                QuestionnaireVersion = qid.Version,
                From = new DateTime(2014, 8, 19),
                To = new DateTime(2014, 8, 22),
            };

            chartStatisticsViewFactory = CreateChartStatisticsViewFactory();
            Because();
        }

        public void Because() => view = chartStatisticsViewFactory.Load(input);

        [Test]
        public void should_return_5_lines_the_same_as_statuses_count() =>
            view.DataSets.Count.Should().Be(5);

        [Test]
        public void should_set_1st_point_horizontal_coord_of_all_lines_equal_to_2014_08_19() =>
            view.DataSets.Should().OnlyContain(line => (string)line.Data[0].X == "2014-08-19");

        [Test]
        public void should_set_2nd_point_horizontal_coord_of_all_lines_equal_to_2014_08_20() =>
            view.DataSets.Should().OnlyContain(line => (string)line.Data[1].X == "2014-08-20");

        [Test]
        public void should_set_3rd_point_horizontal_coord_of_all_lines_equal_to_2014_08_21() =>
            view.DataSets.Should().OnlyContain(line => (string)line.Data[2].X == "2014-08-21");

        [Test]
        public void should_set_4th_point_horizontal_coord_of_all_lines_equal_to_2014_08_22() =>
            view.DataSets.Should().OnlyContain(line => (string)line.Data[3].X == "2014-08-22");

        [Test]
        public void should_set_1st_point_vertical_size_of_all_lines_equal_to_0_as_starting_day_with_no_data() =>
            view.DataSets.Should().OnlyContain(line => (int)line.Data[0].Y == 0);

        [Test]
        public void should_set_2nd_point_vertical_size_of_all_lines_equal_to_0_as_starting_day_with_no_data() =>
            view.DataSets.Should().OnlyContain(line => (int)line.Data[1].Y == 0);

        [Test]
        public void should_set_3rd_point_vertical_size_of_all_lines_equal_to_1() =>
            view.DataSets.Should().OnlyContain(line => (int)line.Data[2].Y == 1);

        [Test]
        public void should_set_4th_point_vertical_size_of_all_lines_equal_to_1_as_prev_day() =>
            view.DataSets.Should().OnlyContain(line => (int)line.Data[3].Y == 1);

        private ChartStatisticsViewFactory chartStatisticsViewFactory;
        private ChartStatisticsInputModel input;
        private ChartStatisticsView view;
    }
}
