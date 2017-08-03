using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_roster_referencing_not_existing_roster_size_question : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var notExistingQuestionId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");

            questionnaire = Create.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Chapter(children: new IComposite[]
                {
                    Create.Question(),

                    Create.Roster(
                        rosterType: RosterSizeSourceType.Question,
                        rosterSizeQuestionId: notExistingQuestionId),
                }),
            });

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_1_message () =>
            verificationMessages.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_messages_with_code_WB0009 () =>
            verificationMessages.ShouldContainCritical("WB0009");

        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireVerifier verifier;
        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
    }
}