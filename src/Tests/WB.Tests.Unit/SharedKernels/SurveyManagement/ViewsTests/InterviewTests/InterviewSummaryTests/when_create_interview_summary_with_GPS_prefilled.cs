using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ViewsTests.InterviewTests.InterviewSummaryTests
{
    internal class when_create_interview_summary_with_GPS_prefilled : InterviewSummaryTestsContext
    {
        [Test]
        public void should_view_model_not_be_null () {
            var questionnaireDocument = CreateQuestionnaireWithTwoPrefieldIncludingOneGPS(Id.g1);

            var plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            
            var viewModel = new InterviewSummary(plainQuestionnaire);
            
            Assert.That(viewModel, Has.Property(nameof(viewModel.IdentifyEntitiesValues)).Count.EqualTo(1));
        }
    }
}
