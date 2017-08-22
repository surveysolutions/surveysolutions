using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireTests
{
    internal class when_questionnaire_has_multi_question_and_we_can_get_number_of_max_answers_for_it
        : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(new IComposite[]
            {
                new MultyOptionsQuestion()
                {
                    PublicKey = validatedQuestionId,
                    MaxAllowedAnswers = proposedSelectedAnswerOptions,
                    QuestionType = QuestionType.MultyOption
                }
            });
        };

        Because of = () =>
            maxSelectedAnswerOptions = Create.Entity.PlainQuestionnaire(questionnaireDocument, 1).GetMaxSelectedAnswerOptions(validatedQuestionId);

        It should_max_selected_answer_options_be_equal_of_proposed_selected_answer_options = () =>
            maxSelectedAnswerOptions.ShouldEqual(proposedSelectedAnswerOptions);

        private static int? maxSelectedAnswerOptions;
        private static int? proposedSelectedAnswerOptions = 5;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid validatedQuestionId = new Guid("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
    }
}