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
    internal class when_verifying_questionnaire_with_static_text_with_title_which_contains_roster_variable_as_substitution_reference : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = Create.QuestionnaireDocument(children: new IComposite[]
            {
                Create.StaticText(staticTextId: staticTextId, text: "hello %r1%"),
                Create.Roster(rosterId: rosterId, variable: "r1"),
            });

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_error_with_code_WB0019 () =>
            verificationMessages.GetError("WB0019").Should().NotBeNull();

        [NUnit.Framework.Test] public void should_return_error_WB0019_with_1_reference () =>
            verificationMessages.GetError("WB0019").References.Count.Should().Be(2);

        [NUnit.Framework.Test] public void should_return_error_WB0019_with_first_reference_with_type_StaticText () =>
            verificationMessages.GetError("WB0019").References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.StaticText);

        [NUnit.Framework.Test] public void should_return_error_WB0019_with_first_reference_with_id_of_static_text () =>
            verificationMessages.GetError("WB0019").References.First().Id.Should().Be(staticTextId);

        [NUnit.Framework.Test] public void should_return_error_WB0019_with_second_reference_with_type_roster () =>
            verificationMessages.GetError("WB0019").References.Second().Type.Should().Be(QuestionnaireVerificationReferenceType.Roster);

        [NUnit.Framework.Test] public void should_return_error_WB0019_with_second_reference_with_id_of_roster () =>
            verificationMessages.GetError("WB0019").References.Second().Id.Should().Be(rosterId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid staticTextId = Guid.Parse("10000000000000000000000000000000");
        private static Guid rosterId = Guid.Parse("20000000000000000000000000000000");
    }
}