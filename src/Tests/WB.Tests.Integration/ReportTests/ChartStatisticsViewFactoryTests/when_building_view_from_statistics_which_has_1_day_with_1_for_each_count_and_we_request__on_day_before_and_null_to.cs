using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;

namespace WB.Tests.Integration.ReportTests.ChartStatisticsViewFactoryTests
{
    public class when_building_view_from_statistics_which_has_1_day_with_1_for_each_count_and_we_request__on_day_before_and_null_to : ChartStatisticsViewFactoryTestsContext
    {
        [OneTimeSetUp]
        public void Establish()
        {
            var qId = WB.Tests.Abc.Create.Entity.QuestionnaireIdentity();

            CreateQuestionnaireStatisticsForChartWithSameCountForAllStatuses(qId, new DateTime(2014, 8, 21), 1);

            input = new ChartStatisticsInputModel
            {
                CurrentDate = new DateTime(2014, 8, 22),
                QuestionnaireId = qId.QuestionnaireId,
                QuestionnaireVersion = qId.Version,
                From = new DateTime(2014, 8, 20),
                To = null,
            };

            chartStatisticsViewFactory = CreateChartStatisticsViewFactory();
            Because();
            UnitOfWork.AcceptChanges();
        }

        public void Because() => view = chartStatisticsViewFactory.Load(input);

        [Test]
        public void should_return_5_lines_the_same_as_statuses_count() =>
            view.DataSets.Count.Should().Be(5);

        [Test]
        public void should_set_1st_point_horizontal_coord_of_all_lines_equal_to_2014_08_19() =>
            view.DataSets.Should().OnlyContain(line => (string)line.Data[0].X == "2014-08-20");

        //[Test]
        //public void should_set_2nd_point_horizontal_coord_of_all_lines_equal_to_2014_08_20() =>
        //    view.DataSets.Should().OnlyContain(line => (string)line[1][0] == "2014-08-21");


        //[Test]
        //public void should_set_1st_point_vertical_size_of_all_lines_equal_to_0_as_starting_day_with_no_data() =>
        //    view.DataSets.Should().OnlyContain(line => (int) line[0][1] == 0);

        //[Test]
        //public void should_set_2nd_point_vertical_size_of_all_lines_equal_to_0_as_starting_day_with_no_data() =>
        //    view.DataSets.Should().OnlyContain(line => (int) line[1][1] == 1);

        
        private ChartStatisticsViewFactory chartStatisticsViewFactory;
        private ChartStatisticsInputModel input;
        private ChartStatisticsView view;
    }
}
