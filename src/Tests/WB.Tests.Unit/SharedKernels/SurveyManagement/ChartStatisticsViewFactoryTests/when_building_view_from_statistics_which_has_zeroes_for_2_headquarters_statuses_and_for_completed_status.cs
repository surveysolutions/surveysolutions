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
    internal class when_building_view_from_statistics_which_has_zeroes_for_2_headquarters_statuses_and_for_completed_status : ChartStatisticsViewFactoryTestsContext
    {
        Establish context = () =>
        {
            var statistics = CreateStatisticsGroupedByDateAndTemplate(new Dictionary<DateTime, QuestionnaireStatisticsForChart>
            {
                {
                    new DateTime(2014, 8, 20),
                    CreateQuestionnaireStatisticsForChart(
                        supervisorAssigned: 1,
                        interviewerAssigned: 1,
                        rejectedBySupervisor: 1,
                        approvedBySupervisor: 1,
                        completed: 0,
                        approvedByHeadquarters: 0,
                        rejectedByHeadquarters: 0)
                },
                {
                    new DateTime(2014, 8, 21),
                    CreateQuestionnaireStatisticsForChart(
                        supervisorAssigned: 1,
                        interviewerAssigned: 1,
                        rejectedBySupervisor: 1,
                        approvedBySupervisor: 1,
                        completed: 0,
                        approvedByHeadquarters: 0,
                        rejectedByHeadquarters: 0)
                },
                {
                    new DateTime(2014, 8, 22),
                    CreateQuestionnaireStatisticsForChart(
                        supervisorAssigned: 1,
                        interviewerAssigned: 1,
                        rejectedBySupervisor: 1,
                        approvedBySupervisor: 1,
                        completed: 0,
                        approvedByHeadquarters: 0,
                        rejectedByHeadquarters: 0)
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
                To = new DateTime(2014, 8, 22),
            };

            chartStatisticsViewFactory = CreateChartStatisticsViewFactory(statsStorage);
        };

        Because of = () =>
            view = chartStatisticsViewFactory.Load(input);

        It should_return_5_lines_the_same_as_statuses_count = () =>
            view.Lines.Length.ShouldEqual(5);

        private static ChartStatisticsViewFactory chartStatisticsViewFactory;
        private static ChartStatisticsInputModel input;
        private static ChartStatisticsView view;
    }
}