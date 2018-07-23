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
    internal class when_verifying_questionnaire_in_which_3_static_texts_have_same_id_and_2_rosters_have_same_other_id : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = Create.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Chapter(children: new IComposite[]
                {
                    Create.StaticText(staticTextId: staticTextSharedId),
                    Create.Roster(rosterId: rosterSharedId, variable: "roster1"),
                    Create.StaticText(staticTextId: staticTextSharedId),
                    Create.Roster(rosterId: rosterSharedId, variable: "roster2"),
                    Create.StaticText(staticTextId: staticTextSharedId),

                    Create.Question(),
                }),
            });

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_2_messages () =>
            verificationMessages.Count().Should().Be(2);

        [NUnit.Framework.Test] public void should_return_messages_with_code_WB0102 () =>
            verificationMessages.Should().OnlyContain(error => error.Code == "WB0102");

        [NUnit.Framework.Test] public void should_return_message_with_2_references () =>
            verificationMessages.Should().Contain(error => error.References.Count == 2);

        [NUnit.Framework.Test] public void should_return_message_with_3_references () =>
            verificationMessages.Should().Contain(error => error.References.Count == 3);

        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireVerifier verifier;
        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static Guid rosterSharedId = Guid.Parse("00000000000000001111111111111111");
        private static Guid staticTextSharedId = Guid.Parse("00000000000000002222222222222222");
    }
}
