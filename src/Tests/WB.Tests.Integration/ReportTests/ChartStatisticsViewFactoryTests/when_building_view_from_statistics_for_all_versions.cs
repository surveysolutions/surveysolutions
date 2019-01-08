using System;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Integration.ReportTests.ChartStatisticsViewFactoryTests
{
    public class when_building_view_from_statistics_for_all_versions : ChartStatisticsViewFactoryTestsContext
    {
        [OneTimeSetUp]
        public void Establish()
        {
            var dateTime = new DateTime(2014, 8, 20);

            var questionnaire = Create.Entity.QuestionnaireIdentity(questionnaireVersion: 1);
            var questionnaire2 = Create.Entity.QuestionnaireIdentity(questionnaire.QuestionnaireId, 2);
            var questionnaire3 = Create.Entity.QuestionnaireIdentity(questionnaire.QuestionnaireId, 3);

            AddInterviewStatuses(questionnaire, dateTime,
                new[] { InterviewStatus.Completed, InterviewStatus.ApprovedBySupervisor, InterviewStatus.ApprovedByHeadquarters });

            AddInterviewStatuses(questionnaire2, dateTime,
                new[] { InterviewStatus.Completed, InterviewStatus.ApprovedBySupervisor });

            AddInterviewStatuses(questionnaire3, dateTime,
                new[] { InterviewStatus.Completed, InterviewStatus.RejectedBySupervisor });

            input = new ChartStatisticsInputModel
            {
                CurrentDate = dateTime,
                QuestionnaireName = questionnaire.Id,
                From = dateTime.AddDays(-5),
                To = dateTime.AddDays(5)
            };
            
            chartStatisticsViewFactory = CreateChartStatisticsViewFactory();

            Because();
        }

        public void Because() => view = chartStatisticsViewFactory.Load(input);

        [Test]
        public void should_return_3_lines_as_for_all_versions_of_questionnaire() => view.DataSets.Count.Should().Be(3);

        private ChartStatisticsViewFactory chartStatisticsViewFactory;
        private ChartStatisticsInputModel input;
        private ChartStatisticsView view;
    }
}
