using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviews;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ChartStatisticsViewFactoryTests
{
    internal class when_building_view_from_statistics_which_has_3_days_with_incremental_data_for_each_count : ChartStatisticsViewFactoryTestsContext
    {
        Establish context = () =>
        {
            var statistics = new StatisticsGroupedByDateAndTemplate
            {
                StatisticsByDate = new Dictionary<DateTime, QuestionnaireStatisticsForChart>
                {
                    {
                        new DateTime(2014, 8, 20),
                        new QuestionnaireStatisticsForChart
                        {
                            ApprovedByHeadquartersCount = 1,
                            ApprovedBySupervisorCount = 1,
                            CompletedCount = 1,
                            InterviewerAssignedCount = 1,
                            RejectedByHeadquartersCount = 1,
                            RejectedBySupervisorCount = 1,
                            SupervisorAssignedCount = 1,
                        }
                    },
                    {
                        new DateTime(2014, 8, 21),
                        new QuestionnaireStatisticsForChart
                        {
                            ApprovedByHeadquartersCount = 2,
                            ApprovedBySupervisorCount = 2,
                            CompletedCount = 2,
                            InterviewerAssignedCount = 2,
                            RejectedByHeadquartersCount = 2,
                            RejectedBySupervisorCount = 2,
                            SupervisorAssignedCount = 2,
                        }
                    },
                    {
                        new DateTime(2014, 8, 22),
                        new QuestionnaireStatisticsForChart
                        {
                            ApprovedByHeadquartersCount = 3,
                            ApprovedBySupervisorCount = 3,
                            CompletedCount = 3,
                            InterviewerAssignedCount = 3,
                            RejectedByHeadquartersCount = 3,
                            RejectedBySupervisorCount = 3,
                            SupervisorAssignedCount = 3,
                        }
                    },
                }
            };

            var statsStorage = Mock.Of<IReadSideRepositoryReader<StatisticsGroupedByDateAndTemplate>>(_
                => _.GetById(Moq.It.IsAny<string>()) == statistics);

            input = new ChartStatisticsInputModel
            {
                CurrentDate = new DateTime(2014, 8, 22),
                QuestionnaireId = Guid.NewGuid(),
                QuestionnaireVersion = 1,
                From = new DateTime(2014, 8, 20),
                To = new DateTime(2014, 8, 22),
            };

            chartStatisticsViewFactory = CreateChartStatisticsViewFactory(statsStorage);
        };

        Because of = () => view = chartStatisticsViewFactory.Load(input);

        It should_return_7_lines_the_same_as_statuses_count = () =>
            view.Lines.Length.ShouldEqual(7);

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