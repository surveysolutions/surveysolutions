using System;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Integration.ReportTests.ChartStatisticsViewFactoryTests
{
    public class when_building_view_from_statistics_which_has_deleted_questionnaires : ChartStatisticsViewFactoryTestsContext
    {
        [OneTimeSetUp]
        public void Establish()
        {
            var dateTime = new DateTime(2014, 8, 20);

            var qid = Create.Entity.QuestionnaireIdentity(questionnaireVersion: 5);
            var deleted = Create.Entity.QuestionnaireIdentity(qid.QuestionnaireId, 4);

            AddInterviewStatuses(qid, dateTime,
                new[] { InterviewStatus.Completed, InterviewStatus.RejectedBySupervisor });

            AddInterviewStatuses(deleted, dateTime,
                new[] { InterviewStatus.Completed, InterviewStatus.ApprovedBySupervisor });
            
            input = new ChartStatisticsInputModel
            {
                CurrentDate = dateTime,
                QuestionnaireName = qid.Id,
                QuestionnaireVersion = qid.Version,
                From = dateTime.AddDays(-5),
                To = dateTime.AddDays(5)
            };

            MarkQuestionnaireDeleted(deleted);

            chartStatisticsViewFactory = CreateChartStatisticsViewFactory();

            Because();
        }

        public void Because() => view = chartStatisticsViewFactory.Load(input);

        [Test]
        public void should_return_1_lines_as_for_enabled_questionnaire() => view.DataSets.Count.Should().Be(1);

        private ChartStatisticsViewFactory chartStatisticsViewFactory;
        private ChartStatisticsInputModel input;
        private ChartStatisticsView view;
    }
}
