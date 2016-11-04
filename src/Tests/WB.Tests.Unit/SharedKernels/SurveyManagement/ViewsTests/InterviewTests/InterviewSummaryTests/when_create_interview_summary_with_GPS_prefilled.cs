using System;
using Machine.Specifications;
using It = Machine.Specifications.It;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Tests.Unit.SharedKernels.SurveyManagement.ViewsTests.InterviewTests.InterviewSummaryTests;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ViewsTests.InterviewTests.InterviewSummaryViewFactoryTests
{
    internal class when_create_interview_summary_with_GPS_prefilled : InterviewSummaryTestsContext
    {
        Establish context = () =>
        {
            questionnaireDocument = CreateQuestionnaireWithTwoPrefieldIncludingOneGPS(questionnaireId);
        };

        Because of = () => viewModel = new InterviewSummary(questionnaireDocument);

        It should_view_model_not_be_null = () =>
            viewModel.ShouldNotBeNull();

        It should_ = () =>
            viewModel.AnswersToFeaturedQuestions.Count.ShouldEqual(1);

        
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static InterviewSummary viewModel;
    }
}
