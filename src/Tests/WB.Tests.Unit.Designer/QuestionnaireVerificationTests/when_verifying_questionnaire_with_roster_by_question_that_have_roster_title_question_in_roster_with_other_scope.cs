using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_roster_by_question_that_have_roster_title_question_in_roster_with_other_scope : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion("rosterSizeQuestion1")
                {
                    PublicKey = rosterSizeQuestion1Id,
                    StataExportCaption = "var1",
                    IsInteger = true
                },
                new NumericQuestion("rosterSizeQuestion2")
                {
                    PublicKey = rosterSizeQuestion2Id,
                    StataExportCaption = "var2",
                    IsInteger = true
                },
                new Group
                {
                    Title = "Roster 1. Triggered by rosterSizeQuestion2",
                    PublicKey = roster1Id,
                    IsRoster = true,
                    VariableName = "a",
                    RosterSizeSource = RosterSizeSourceType.Question,
                    RosterSizeQuestionId = rosterSizeQuestion2Id,
                    RosterTitleQuestionId = rosterTitleQuestionId,
                    Children = new List<IComposite>()
                    {
                        new TextQuestion("rosterTitleQuestion")
                        {
                            StataExportCaption = "var3",
                            PublicKey = rosterTitleQuestionId
                        }
                    }.ToReadOnlyCollection()
                },
                new Group
                {
                    Title = "Roster 2. Triggered by rosterSizeQuestion1",
                    PublicKey = roster2Id,
                    IsRoster = true,
                    VariableName = "b",
                    RosterSizeSource = RosterSizeSourceType.Question,
                    RosterSizeQuestionId = rosterSizeQuestion1Id,
                    RosterTitleQuestionId = rosterTitleQuestionId
                }
                );

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_1_message () =>
            verificationMessages.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0035__ () =>
            verificationMessages.Single().Code.Should().Be("WB0035");

        [NUnit.Framework.Test] public void should_return_message_with_1_references () =>
            verificationMessages.Single().References.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_message_reference_with_type_Roster () =>
            verificationMessages.Single().References.Single().Type.Should().Be(QuestionnaireVerificationReferenceType.Roster);

        [NUnit.Framework.Test] public void should_return_message_reference_with_id_of_roster2Id () =>
            verificationMessages.Single().References.First().Id.Should().Be(roster2Id);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid roster1Id = Guid.Parse("10000000000000000000000000000000");
        private static Guid roster2Id = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterSizeQuestion1Id = Guid.Parse("13333333333333333333333333333333");
        private static Guid rosterSizeQuestion2Id = Guid.Parse("22222222222222222222222222222222");
        private static Guid rosterTitleQuestionId = Guid.Parse("11333333333333333333333333333333");
    }
}