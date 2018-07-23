using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_linked_question_referencing_on_group_which_is_not_roster : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            linkedQuestionId = Guid.Parse("10000000000000000000000000000000");
            questionnaire = CreateQuestionnaireDocument(
                Create.Group(groupId: groupId, title: "title"),
                Create.SingleQuestion(
                    linkedQuestionId,
                    linkedToRosterId: groupId,
                    variable: "var"
                ));
            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_1_message () =>
            verificationMessages.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0103__ () =>
            verificationMessages.Single().Code.Should().Be("WB0103");

        [NUnit.Framework.Test] public void should_return_message_with_one_references () =>
            verificationMessages.Single().References.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_message_reference_with_type_Question () =>
            verificationMessages.Single().References.Single().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_message_reference_with_id_of_linkedQuestionId () =>
            verificationMessages.Single().References.Single().Id.Should().Be(linkedQuestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid linkedQuestionId;
        private static Guid groupId=Guid.NewGuid();
    }
}