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
    internal class when_verifying_questionnaire_with_static_text_with_title_which_contains_roster_title_as_substitution_reference : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = Create.QuestionnaireDocument(children: new IComposite[]
            {
                Create.StaticText(staticTextId: staticTextId, text: "hello %rostertitle%"),
            });

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_error_with_code_WB0059 () =>
            verificationMessages.GetError("WB0059").Should().NotBeNull();

        [NUnit.Framework.Test] public void should_return_error_WB0059_with_1_reference () =>
            verificationMessages.GetError("WB0059").References.Count.Should().Be(1);

        [NUnit.Framework.Test] public void should_return_error_WB0059_with_reference_with_type_StaticText () =>
            verificationMessages.GetError("WB0059").References.Single().Type.Should().Be(QuestionnaireVerificationReferenceType.StaticText);

        [NUnit.Framework.Test] public void should_return_error_WB0059_with_reference_with_id_of_static_text () =>
            verificationMessages.GetError("WB0059").References.Single().Id.Should().Be(staticTextId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid staticTextId = Guid.Parse("10000000000000000000000000000000");
    }
}