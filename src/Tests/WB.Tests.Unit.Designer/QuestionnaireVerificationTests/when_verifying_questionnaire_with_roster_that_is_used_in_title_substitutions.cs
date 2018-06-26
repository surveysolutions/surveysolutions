using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_roster_that_is_used_in_title_substitutions : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                Create.Question(questionId: questionId, title: $"substitution to %{rosterVariableName}%"),
                Create.Roster(rosterId: rosterId, variable: rosterVariableName),
            });

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0019 () =>
            verificationMessages.ShouldContainError("WB0019");

        [NUnit.Framework.Test] public void should_return_message_with_2_references () =>
            verificationMessages.GetError("WB0019").References.Count.Should().Be(2);

        [NUnit.Framework.Test] public void should_return_message_reference_with_type_Question () =>
            verificationMessages.GetError("WB0019").References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_message_reference_with_type_Roster () =>
            verificationMessages.GetError("WB0019").References.Second().Type.Should().Be(QuestionnaireVerificationReferenceType.Roster);

        [NUnit.Framework.Test] public void should_return_message_reference_with_id_of_question () =>
            verificationMessages.GetError("WB0019").References.First().Id.Should().Be(questionId);

        [NUnit.Framework.Test] public void should_return_message_reference_with_id_of_roster () =>
            verificationMessages.GetError("WB0019").References.Second().Id.Should().Be(rosterId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid rosterId = Guid.Parse("10000000000000000000000000000000");
        private static string rosterVariableName = "rosterTheChosen";
        private static Guid questionId = Guid.Parse("11000000000000000000000000000000");
    }
}