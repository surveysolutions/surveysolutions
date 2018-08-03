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
    internal class when_verifying_questionnaire_with_2_rosters_inside_roster_and_second_roster_has_roster_size_question_from_first_roster : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            Guid rosterSizeQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            Guid rosterSizeQuestionForChildRoster1Id = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                Create.NumericIntegerQuestion(id: rosterSizeQuestionId, variable: "var1"),
                Create.NumericIntegerQuestion(rosterSizeQuestionForChildRoster1Id,variable: "var2"),
                Create.NumericRoster(variable:"a", rosterSizeQuestionId: rosterSizeQuestionId, children: new IComposite[]
                {
                    Create.NumericRoster(variable:"b", rosterSizeQuestionId: rosterSizeQuestionForChildRoster1Id, children: new IComposite[]
                    {
                        Create.NumericIntegerQuestion(id: rosterSizeQuestionWithInvalidRosterLevelId, variable: "var3")
                    }),
                    Create.NumericRoster(groupWithInvalidRosterSizeQuestionId, variable:"c", rosterSizeQuestionId: rosterSizeQuestionWithInvalidRosterLevelId)
                })
            });

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0054__ () =>
            verificationMessages.ShouldContainError("WB0054");

        [NUnit.Framework.Test] public void should_return_message_with_level_general () =>
            verificationMessages.GetError("WB0054").MessageLevel.Should().Be(VerificationMessageLevel.General);

        [NUnit.Framework.Test] public void should_return_message_with_2_references () =>
            verificationMessages.GetError("WB0054").References.Count().Should().Be(2);

        [NUnit.Framework.Test] public void should_return_first_message_reference_with_type_Roster () =>
            verificationMessages.GetError("WB0054").References.ElementAt(0).Type.Should().Be(QuestionnaireVerificationReferenceType.Roster);

        [NUnit.Framework.Test] public void should_return_first_message_reference_with_id_of_rosterId () =>
            verificationMessages.GetError("WB0054").References.ElementAt(0).Id.Should().Be(groupWithInvalidRosterSizeQuestionId);

        [NUnit.Framework.Test] public void should_return_second_message_reference_with_type_question () =>
            verificationMessages.Single().References.ElementAt(1).Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_second_message_reference_with_id_of_rosterSizeQuestionWithInvalidRosterLevelId () =>
            verificationMessages.Single().References.ElementAt(1).Id.Should().Be(rosterSizeQuestionWithInvalidRosterLevelId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid groupWithInvalidRosterSizeQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid rosterSizeQuestionWithInvalidRosterLevelId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}
