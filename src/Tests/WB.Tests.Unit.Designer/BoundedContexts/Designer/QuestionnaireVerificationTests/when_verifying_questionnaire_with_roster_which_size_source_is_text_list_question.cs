using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_roster_which_size_source_is_text_list_question : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var textListQuestionId = Guid.Parse("11111111111111111111111111111111");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(new IComposite[]
            {
                Create.TextListQuestion (textListQuestionId, maxAnswerCount: 20, variable: "list" ),
                new Group { IsRoster = true, RosterSizeSource = RosterSizeSourceType.Question,VariableName = "a", RosterSizeQuestionId = textListQuestionId, PublicKey = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA") },
            });

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)).ToList();

        [NUnit.Framework.Test] public void should_not_produce_any_messages () =>
            verificationMessages.ShouldBeEmpty();

        private static List<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
    }
}