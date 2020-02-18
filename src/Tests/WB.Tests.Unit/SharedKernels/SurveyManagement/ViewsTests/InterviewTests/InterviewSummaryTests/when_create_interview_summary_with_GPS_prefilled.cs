using System;
using FluentAssertions;

using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Unit.SharedKernels.SurveyManagement.ViewsTests.InterviewTests.InterviewSummaryTests;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ViewsTests.InterviewTests.InterviewSummaryViewFactoryTests
{
    internal class when_create_interview_summary_with_GPS_prefilled : InterviewSummaryTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaireDocument = CreateQuestionnaireWithTwoPrefieldIncludingOneGPS(questionnaireId);
            BecauseOf();
        }

        public void BecauseOf() => viewModel = new InterviewSummary(new PlainQuestionnaire(questionnaireDocument, 1, null, new SubstitutionService()));

        [NUnit.Framework.Test] public void should_view_model_not_be_null () =>
            viewModel.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_ () =>
            viewModel.AnswersToFeaturedQuestions.Count.Should().Be(1);

        
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static InterviewSummary viewModel;
    }
}
