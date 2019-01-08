using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Integration.ReportTests.ChartStatisticsViewFactoryTests
{
    internal class when_building_statistics_view_questionnaire_list : ChartStatisticsViewFactoryTestsContext
    {
        private ChartStatisticsViewFactory chartStatisticsViewFactory;
        private List<QuestionnaireVersionsComboboxViewItem> view;
        private QuestionnaireIdentity emptyQuestionnaire;

        [OneTimeSetUp]
        public void Establish()
        {
            var questionnaireId = Guid.NewGuid();
            var baseDate = new DateTime(2014, 8, 22);

            var qid = Create.Entity.QuestionnaireIdentity(questionnaireId);
            emptyQuestionnaire = Create.Entity.QuestionnaireIdentity();

            CreateQuestionnaireStatisticsForChartWithSameCountForAllStatuses(qid, baseDate.AddDays(-3), 1);
            
            this.chartStatisticsViewFactory = CreateChartStatisticsViewFactory();

            EnsureQuestionnaireExists(emptyQuestionnaire);
            
            Because();
        }

        public void Because() => view = chartStatisticsViewFactory.GetQuestionnaireListWithData();

        [Test]
        public void should_not_have_questionnaire_without_data() => this.questionnaires
            .Verify(q => q.GetQuestionnaireComboboxViewItems(
                It.Is<List<QuestionnaireBrowseItem>>(l => l.Single().Id != emptyQuestionnaire.Id)), Times.Once);
    }
}
