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
    internal class when_verifying_questionnaire_with_linked_question_referencing_on_not_existing_question : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            linkedQuestionId = Guid.Parse("10000000000000000000000000000000");
            questionnaire = CreateQuestionnaireDocument(
                Create.SingleQuestion(
                    linkedQuestionId,
                    linkedToQuestionId: Guid.NewGuid(),
                    variable: "var",
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

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0011__ () =>
            verificationMessages.ShouldContainError("WB0074");

        [NUnit.Framework.Test] public void should_return_message_with_one_references () =>
            verificationMessages.GetError("WB0074").References.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_message_reference_with_type_Question () =>
            verificationMessages.GetError("WB0074").References.Single().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_message_reference_with_id_of_linkedQuestionId () =>
            verificationMessages.GetError("WB0074").References.Single().Id.Should().Be(linkedQuestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid linkedQuestionId;
    }
}
