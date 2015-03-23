using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Tests.Unit.BoundedContexts.Capi.InterviewViewModelTests
{
    internal class when_cteating_interview_on_CAPI_and_questionnaire_has_questions_that_reference_prefilled_one_in_title : InterviewViewModelTestContext
    {
        Establish context = () =>
        {
            questionnarie = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion
                {
                    PublicKey = prefilledNumeric,
                    QuestionType = QuestionType.Numeric,
                    StataExportCaption = "numeric",
                    Featured = true
                },
                new TextQuestion
                {
                    PublicKey = questionReferencePrefilledNumericId,
                    QuestionType = QuestionType.Text,
                    QuestionText = "Hello, %numeric%"
                });

            rosterStructure = CreateQuestionnaireRosterStructure(questionnarie);
            interviewViewModel = CreateInterviewViewModel(questionnarie, rosterStructure);
        };

        private Because of = () =>
            interviewViewModel.SetAnswer(ConversionHelper.ConvertIdAndRosterVectorToString(prefilledNumeric, new decimal[0]), 2);

        It should_substituted_title_of_question_with_substitution_with_answer_on_prefilled_question = () =>
            GetQuestion(questionReferencePrefilledNumericId, new decimal[0]).Text.ShouldEqual("Hello, 2");

        private static QuestionViewModel GetQuestion(Guid questionId, decimal[] rosterVector)
        {
            return interviewViewModel.FindQuestion(q => q.PublicKey == Create.InterviewItemId(questionId, rosterVector)).FirstOrDefault();
        }

        private static InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure rosterStructure;

        private static Guid prefilledNumeric = Guid.Parse("33333333333533333333333333333333");
        private static Guid questionReferencePrefilledNumericId = Guid.Parse("30000000000000000000000000000000");
    }
}