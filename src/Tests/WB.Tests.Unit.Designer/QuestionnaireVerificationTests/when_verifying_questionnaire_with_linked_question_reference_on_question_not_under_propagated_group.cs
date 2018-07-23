using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_linked_question_reference_on_question_not_under_propagated_group : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            linkedQuestionId = Guid.Parse("10000000000000000000000000000000");
            notUnderPropagatedGroupLinkingQuestionId = Guid.Parse("12222222222222222222222222222222");
            questionnaire = CreateQuestionnaireDocument(
                Create.NumericIntegerQuestion(
                    notUnderPropagatedGroupLinkingQuestionId,
                    variable: "var1"
                ),
                Create.SingleQuestion(
                    linkedQuestionId,
                    linkedToQuestionId: notUnderPropagatedGroupLinkingQuestionId,
                    variable: "var2",
                    options: new List<Answer>
                    {
                        new Answer() {AnswerValue = "1", AnswerText = "opt 1"},
                        new Answer() {AnswerValue = "2", AnswerText = "opt 2"}
                    }
                ));
            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0013 () =>
            verificationMessages.ShouldContainError("WB0013");

        [NUnit.Framework.Test] public void should_return_message_with_two_references () =>
            verificationMessages.GetError("WB0013").References.Count().Should().Be(2);

        [NUnit.Framework.Test] public void should_return_first_message_reference_with_type_Question () =>
            verificationMessages.GetError("WB0013").References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_first_message_reference_with_id_of_linkedQuestionId () =>
            verificationMessages.GetError("WB0013").References.First().Id.Should().Be(linkedQuestionId);

        [NUnit.Framework.Test] public void should_return_last_message_reference_with_type_Question () =>
            verificationMessages.GetError("WB0013").References.Last().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_last_message_reference_with_id_of_notUnderPropagatedGroupLinkingQuestionId () =>
            verificationMessages.GetError("WB0013").References.Last().Id.Should().Be(notUnderPropagatedGroupLinkingQuestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid linkedQuestionId;
        private static Guid notUnderPropagatedGroupLinkingQuestionId;
    }
}
