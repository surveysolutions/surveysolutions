using System;
using System.Collections.Generic;
using Machine.Specifications;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ChartStatisticsViewFactoryTests
{
    public class when_building_view_from_statistics_which_has_zeroes_for_2_headquarters_statuses_and_for_completed_status : ChartStatisticsViewFactoryTestsContext
    {
        [OneTimeSetUp]
        public void Establish ()
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
                        restarted: 2,
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
                        restarted: 5,
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
                        restarted: 7,
                        completed: 0,
                        approvedByHeadquarters: 0,
                        rejectedByHeadquarters: 0)
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

            Because();
        }

        public void Because() =>
            view = chartStatisticsViewFactory.Load(input);

        [Test]
        public void should_return_6_lines_the_same_as_statuses_count() => view.Lines.Length.ShouldEqual(6);

        private static ChartStatisticsViewFactory chartStatisticsViewFactory;
        private static ChartStatisticsInputModel input;
        private static ChartStatisticsView view;
    }
}