using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_categorical_one_answer_question_with_1_option : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument(new SingleQuestion()
            {
                PublicKey = questionId,
                StataExportCaption = "var",
                Answers = new List<Answer>() {new Answer() {AnswerValue = "1", AnswerText = "option text"}}
            });
         
            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_1_message = () =>
            verificationMessages.Count().ShouldEqual(1);

        It should_return_message_with_code__WB0060 = () =>
            verificationMessages.Single().Code.ShouldEqual("WB0060");

        It should_return_message_with_1_references = () =>
            verificationMessages.Single().References.Count().ShouldEqual(1);

        It should_return_message_reference_with_type_Question = () =>
            verificationMessages.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_message_reference_with_id_of_questionId = () =>
            verificationMessages.Single().References.First().Id.ShouldEqual(questionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
    }
}