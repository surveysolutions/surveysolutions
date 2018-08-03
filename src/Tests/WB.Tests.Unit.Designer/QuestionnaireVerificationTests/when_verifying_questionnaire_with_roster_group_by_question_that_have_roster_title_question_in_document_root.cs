using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_roster_group_by_question_that_have_roster_title_question_in_document_root : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            rosterGroupId = Guid.Parse("10000000000000000000000000000000");
            rosterSizeQuestionId = Guid.Parse("13333333333333333333333333333333");
            rosterTitleQuestionId = Guid.Parse("11333333333333333333333333333333");
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion("question 1")
                {
                    PublicKey = rosterSizeQuestionId,
                    StataExportCaption = "var1",
                    IsInteger = true
                },
                new Group()
                {
                    PublicKey = rosterGroupId,
                    IsRoster = true,
                    VariableName = "a",
                    RosterSizeSource = RosterSizeSourceType.Question,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    RosterTitleQuestionId = rosterTitleQuestionId
                },
                new TextQuestion("question 1")
                {
                    StataExportCaption = "var2",
                    PublicKey = rosterTitleQuestionId
                });
            
            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0035__ () =>
            verificationMessages.ShouldContainError("WB0035");

        [NUnit.Framework.Test] public void should_return_message_with_1_references () =>
            verificationMessages.GetError("WB0035").References.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_message_reference_with_type_Roster () =>
            verificationMessages.GetError("WB0035").References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Roster);

        [NUnit.Framework.Test] public void should_return_message_reference_with_id_of_rosterGroupId () =>
            verificationMessages.GetError("WB0035").References.First().Id.Should().Be(rosterGroupId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid rosterGroupId;
        private static Guid rosterSizeQuestionId;
        private static Guid rosterTitleQuestionId;
    }
}
