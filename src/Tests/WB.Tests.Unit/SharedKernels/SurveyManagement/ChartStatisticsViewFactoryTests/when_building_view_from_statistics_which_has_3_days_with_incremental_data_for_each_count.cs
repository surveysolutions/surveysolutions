﻿using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ChartStatisticsViewFactoryTests
{
    internal class when_building_view_from_statistics_which_has_3_days_with_incremental_data_for_each_count : ChartStatisticsViewFactoryTestsContext
    {
        Establish context = () =>
        {
            var statistics = CreateStatisticsGroupedByDateAndTemplate(new Dictionary<DateTime, QuestionnaireStatisticsForChart>
            {
                {
                    new DateTime(2014, 8, 20),
                    CreateQuestionnaireStatisticsForChartWithSameCountForAllStatuses(count: 1)
                },
                {
                    new DateTime(2014, 8, 21),
                    CreateQuestionnaireStatisticsForChartWithSameCountForAllStatuses(count: 2)
                },
                {
                    new DateTime(2014, 8, 22),
                    CreateQuestionnaireStatisticsForChartWithSameCountForAllStatuses(count: 3)
                },
            });

            input = new ChartStatisticsInputModel
            {
                CurrentDate = new DateTime(2014, 8, 22),
                QuestionnaireId = Guid.NewGuid(),
                QuestionnaireVersion = 1,
                From = new DateTime(2014, 8, 20),
                To = new DateTime(2014, 8, 22),
            };

            chartStatisticsViewFactory = CreateChartStatisticsViewFactory(statistics: statistics);
        };

        Because of = () => view = chartStatisticsViewFactory.Load(input);

        It should_return_7_lines_the_same_as_statuses_count = () =>
            view.Lines.Length.ShouldEqual(7);

        It should_set_1st_point_horizontal_coord_of_all_lines_equal_to_2014_08_20 = () =>
            view.Lines.ShouldEachConformTo(line => (string)line[0][0] == "2014-08-20");

        It should_set_2nd_point_horizontal_coord_of_all_lines_equal_to_2014_08_21 = () =>
            view.Lines.ShouldEachConformTo(line => (string)line[1][0] == "2014-08-21");

        It should_set_3rd_point_horizontal_coord_of_all_lines_equal_to_2014_08_22 = () =>
            view.Lines.ShouldEachConformTo(line => (string)line[2][0] == "2014-08-22");

        It should_set_1st_point_vertical_size_of_all_lines_equal_to_1 = () =>
            view.Lines.ShouldEachConformTo(line => (int) line[0][1] == 1);

        It should_set_2nd_point_vertical_size_of_all_lines_equal_to_2 = () =>
            view.Lines.ShouldEachConformTo(line => (int) line[1][1] == 2);

        It should_set_3rd_point_vertical_size_of_all_lines_equal_to_3 = () =>
            view.Lines.ShouldEachConformTo(line => (int) line[2][1] == 3);

        private static ChartStatisticsViewFactory chartStatisticsViewFactory;
        private static ChartStatisticsInputModel input;
        private static ChartStatisticsView view;
    }
}