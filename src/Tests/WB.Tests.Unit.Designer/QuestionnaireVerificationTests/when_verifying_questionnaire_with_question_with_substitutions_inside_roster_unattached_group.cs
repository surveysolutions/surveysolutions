using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_question_with_substitutions_inside_roster_unattached_group : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionWithSubstitutionsId = Guid.Parse("10000000000000000000000000000000");
            underDeeperRosterLevelQuestionId = Guid.Parse("12222222222222222222222222222222");
            var rosterGroupId = Guid.Parse("13333333333333333333333333333333");
            rosterSizeQuestionId = Guid.Parse("11133333333333333333333333333333");

            questionnaire = CreateQuestionnaireDocument(
                Create.NumericIntegerQuestion(
                    rosterSizeQuestionId,
                    variable: "rosterSize"
                ),
                new Group()
                {
                    PublicKey = rosterGroupId,
                    IsRoster = true,
                    VariableName = "a",
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new IComposite[]{
                        Create.NumericIntegerQuestion(
                        underDeeperRosterLevelQuestionId,
                        variable: underDeeperRosterLevelQuestionVariableName
                    )}.ToReadOnlyCollection()
                },
                Create.SingleOptionQuestion(
                    questionWithSubstitutionsId,
                    variable: "var2",
                    title: string.Format("hello %{0}%", underDeeperRosterLevelQuestionVariableName),
                    answers: new List<Answer> { new Answer() { AnswerValue = "1", AnswerText = "opt 1" }, new Answer() { AnswerValue = "2", AnswerText = "opt 2" } }
                ));

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0019 () =>
            verificationMessages.ShouldContainError("WB0019");

        [NUnit.Framework.Test] public void should_return_WB0019_error_with_2_references_on_questions () =>
            verificationMessages.GetError("WB0019")
                .References.ToList()
                .ForEach(question => question.Type.Should().Be(QuestionnaireVerificationReferenceType.Question));

        [NUnit.Framework.Test] public void should_return_WB0019_error_with_first_reference_to_question_with_substitution_text () =>
            verificationMessages.GetError("WB0019").References.ElementAt(0).Id.Should().Be(questionWithSubstitutionsId);

        [NUnit.Framework.Test] public void should_return_WB0019_error_with_second_reference_to_question_that_used_as_substitution_question () =>
            verificationMessages.GetError("WB0019").References.ElementAt(1).Id.Should().Be(underDeeperRosterLevelQuestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionWithSubstitutionsId;
        private static Guid underDeeperRosterLevelQuestionId;
        private static Guid rosterSizeQuestionId;
        private const string underDeeperRosterLevelQuestionVariableName = "i_am_deeper_ddddd_deeper";
    }
}
