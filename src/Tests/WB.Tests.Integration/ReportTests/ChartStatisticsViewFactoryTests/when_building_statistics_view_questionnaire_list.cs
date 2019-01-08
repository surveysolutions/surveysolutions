using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Integration.ReportTests.ChartStatisticsViewFactoryTests
{
    internal class when_building_statistics_view_questionnaire_list : ChartStatisticsViewFactoryTestsContext
    {
        private ChartStatisticsViewFactory chartStatisticsViewFactory;
        private List<QuestionnaireVersionsComboboxViewItem> questionnaireList;
        private QuestionnaireIdentity emptyQuestionnaire;

        [OneTimeSetUp]
        public void Establish()
        {
            var questionnaireId = Guid.NewGuid();
            
            var qid = Create.Entity.QuestionnaireIdentity(questionnaireId);
            emptyQuestionnaire = Create.Entity.QuestionnaireIdentity();

            CreateQuestionnaireStatisticsForChartWithSameCountForAllStatuses(qid, DateTime.Now, 1);
            
            this.chartStatisticsViewFactory = CreateChartStatisticsViewFactory();

            EnsureQuestionnaireExists(emptyQuestionnaire);
            
            Because();
        }

        public void Because() => questionnaireList = chartStatisticsViewFactory.GetQuestionnaireListWithData();

        [Test]
        public void should_not_have_questionnaire_without_data() => questionnaireList.Count.Should().Be(1);
    }
}
