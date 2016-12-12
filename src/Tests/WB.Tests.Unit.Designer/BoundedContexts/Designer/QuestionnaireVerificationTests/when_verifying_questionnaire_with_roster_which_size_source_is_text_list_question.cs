using System;
using System.Collections.Generic;
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
        Establish context = () =>
        {
            var textListQuestionId = Guid.Parse("11111111111111111111111111111111");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(new IComposite[]
            {
                new TextListQuestion { QuestionType = QuestionType.TextList, PublicKey = textListQuestionId, StataExportCaption = "var", MaxAnswerCount = 20 },
                new Group { IsRoster = true, RosterSizeSource = RosterSizeSourceType.Question,VariableName = "a", RosterSizeQuestionId = textListQuestionId, PublicKey = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA") },
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_not_produce_any_messages = () =>
            verificationMessages.ShouldBeEmpty();

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
    }
}