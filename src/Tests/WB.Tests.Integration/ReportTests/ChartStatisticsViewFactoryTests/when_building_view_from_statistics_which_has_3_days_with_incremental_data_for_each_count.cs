using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;
using WB.Tests.Abc;

namespace WB.Tests.Integration.ReportTests.ChartStatisticsViewFactoryTests
{
    public class when_building_view_from_statistics_which_has_3_days_with_incremental_data_for_each_count : ChartStatisticsViewFactoryTestsContext
    {
        [OneTimeSetUp]
        public void Establish()
        {

            var qid = Create.Entity.QuestionnaireIdentity();

            CreateQuestionnaireStatisticsForChartWithSameCountForAllStatuses(qid, new DateTime(2014, 8, 20), 1);
            CreateQuestionnaireStatisticsForChartWithSameCountForAllStatuses(qid, new DateTime(2014, 8, 21), 1);
            CreateQuestionnaireStatisticsForChartWithSameCountForAllStatuses(qid, new DateTime(2014, 8, 22), 1);

            input = new ChartStatisticsInputModel
            {
                CurrentDate = new DateTime(2014, 8, 22),
                QuestionnaireName = qid.Id,
                QuestionnaireVersion = qid.Version,
                From = new DateTime(2014, 8, 20),
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
        public void should_set_1st_point_horizontal_coord_of_all_lines_equal_to_2014_08_20() =>
            view.DataSets.Should().OnlyContain(line => (string)line.Data[0].X == "2014-08-20");

        [Test]
        public void should_set_2nd_point_horizontal_coord_of_all_lines_equal_to_2014_08_21() =>
            view.DataSets.Should().OnlyContain(line => (string)line.Data[1].X == "2014-08-21");

        [Test]
        public void should_set_3rd_point_horizontal_coord_of_all_lines_equal_to_2014_08_22() =>
            view.DataSets.Should().OnlyContain(line => (string)line.Data[2].X == "2014-08-22");

        [Test]
        public void should_set_1st_point_vertical_size_of_all_lines_equal_to_1() =>
            view.DataSets.Should().OnlyContain(line => (int)line.Data[0].Y == 1);

        [Test]
        public void should_set_2nd_point_vertical_size_of_all_lines_equal_to_2() =>
            view.DataSets.Should().OnlyContain(line => (int)line.Data[1].Y == 2);

        [Test]
        public void should_set_3rd_point_vertical_size_of_all_lines_equal_to_3() =>
            view.DataSets.Should().OnlyContain(line => (int)line.Data[2].Y == 3);

        private ChartStatisticsViewFactory chartStatisticsViewFactory;
        private ChartStatisticsInputModel input;
        private ChartStatisticsView view;
    }
}
