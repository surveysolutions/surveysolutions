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
    internal class when_verifying_questionnaire_with_linked_question_reference_on_question_not_under_propagated_group : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            linkedQuestionId = Guid.Parse("10000000000000000000000000000000");
            notUnderPropagatedGroupLinkingQuestionId = Guid.Parse("12222222222222222222222222222222");
            questionnaire = CreateQuestionnaireDocument(
                new NumericQuestion
            {
                PublicKey = notUnderPropagatedGroupLinkingQuestionId,
                StataExportCaption = "var1",
                QuestionType = QuestionType.Numeric
            },
                new SingleQuestion()
            {
                PublicKey = linkedQuestionId,
                LinkedToQuestionId = notUnderPropagatedGroupLinkingQuestionId,
                StataExportCaption = "var2",
                Answers = { new Answer() { AnswerValue = "1", AnswerText = "opt 1" }, new Answer() { AnswerValue = "2", AnswerText = "opt 2" } }
            });
            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(questionnaire);

        It should_return_1_message = () =>
            verificationMessages.Count().ShouldEqual(1);

        It should_return_message_with_code__WB0013 = () =>
            verificationMessages.Single().Code.ShouldEqual("WB0013");

        It should_return_message_with_two_references = () =>
            verificationMessages.Single().References.Count().ShouldEqual(2);

        It should_return_first_message_reference_with_type_Question = () =>
            verificationMessages.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_first_message_reference_with_id_of_linkedQuestionId = () =>
            verificationMessages.Single().References.First().Id.ShouldEqual(linkedQuestionId);

        It should_return_last_message_reference_with_type_Question = () =>
            verificationMessages.Single().References.Last().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_last_message_reference_with_id_of_notUnderPropagatedGroupLinkingQuestionId = () =>
            verificationMessages.Single().References.Last().Id.ShouldEqual(notUnderPropagatedGroupLinkingQuestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid linkedQuestionId;
        private static Guid notUnderPropagatedGroupLinkingQuestionId;
    }
}
