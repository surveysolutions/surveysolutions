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
    internal class when_questionnaire_has_fixed_roster_with_3_items : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.FixedRoster(rosterId: rosterId, fixedTitles: new[] { "1", "2", "3" }),
            });

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            messages = verifier.Verify(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_warning_WB0207 () =>
            messages.ShouldContainWarning("WB0207");

        [NUnit.Framework.Test] public void should_put_reference_to_that_roster_to_message_WB0207 () =>
            messages.GetWarning("WB0207").References.Single().Id.Should().Be(rosterId);

        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireVerifier verifier;
        private static IEnumerable<QuestionnaireVerificationMessage> messages;
        private static Guid rosterId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
    }
}