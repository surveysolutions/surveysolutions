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
    internal class when_verifying_questionnaire_with_roster_group_by_numeric_question_that_have_2_rosters_by_different_size_question_and_1__the_same_title_question : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaireDocument(
                new NumericQuestion("roster size question 1")
            {
                PublicKey = rosterSizeQuestion1Id,
                StataExportCaption = "var1",
                IsInteger = true
            },
                new NumericQuestion("roster size question 2")
            {
                PublicKey = rosterSizeQuestion2Id,
                StataExportCaption = "var2",
                IsInteger = true
            },
                new Group()
            {
                PublicKey = rosterGroup1Id,
                IsRoster = true,
                VariableName = "a",
                RosterSizeSource = RosterSizeSourceType.Question,
                RosterSizeQuestionId = rosterSizeQuestion1Id,
                RosterTitleQuestionId = rosterTitleQuestionId
            },
                new Group()
            {
                PublicKey = rosterGroup2Id,
                IsRoster = true,
                VariableName = "b",
                RosterSizeSource = RosterSizeSourceType.Question,
                RosterSizeQuestionId = rosterSizeQuestion2Id,
                RosterTitleQuestionId = rosterTitleQuestionId
            });
            verifier = CreateQuestionnaireVerifier();

            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));


        [NUnit.Framework.Test]
        public void should_return_2_errors_with_code__WB0035__() =>
            verificationMessages.ShouldContainError("WB0035");

        [NUnit.Framework.Test] public void should_return_2_errors_with_1_references () =>
            verificationMessages.Should().OnlyContain(error => error.References.Count() == 1);

        [NUnit.Framework.Test] public void should_return_message_reference_with_type_Roster () =>
            verificationMessages.Should().OnlyContain(error => error.References.First().Type == QuestionnaireVerificationReferenceType.Roster);

        [NUnit.Framework.Test] public void should_return_message_reference_with_id_of_rosterGroup1Id () =>
            verificationMessages.ElementAt(0).References.First().Id.Should().Be(rosterGroup1Id);

        [NUnit.Framework.Test] public void should_return_message_reference_with_id_of_rosterGroup2Id () =>
            verificationMessages.ElementAt(1).References.First().Id.Should().Be(rosterGroup2Id);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid rosterGroup1Id = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterGroup2Id = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid rosterSizeQuestion1Id = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid rosterSizeQuestion2Id = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid rosterTitleQuestionId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
    }
}
