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
    internal class when_verifying_questionnaire_with_question_inside_roster_that_has_substitutions_references_with_deeper_roster_level : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var rosterGroupId1 =       Guid.Parse("AAAAAAAAAAAAAAAA1111111111111111");
            var rosterGroupId2 =       Guid.Parse("AAAAAAAAAAAAAAAA2222222222222222");
            var rosterSizeQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                Create.NumericIntegerQuestion(rosterSizeQuestionId, variable: "var1"),
                new Group()
                {
                    PublicKey = rosterGroupId1,
                    IsRoster = true,
                    VariableName = "a",
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>()
                    {
                        Create.SingleOptionQuestion(
                            questionWithSubstitutionsId,
                            variable: "var2",
                            title: string.Format("hello %{0}%", underDeeperRosterLevelQuestionVariableName),
                            answers: new List<Answer> { new Answer(){ AnswerValue = "1", AnswerText = "opt 1" }, new Answer(){ AnswerValue = "2", AnswerText = "opt 2" }}
                        ),
                        new Group()
                        {
                            PublicKey = rosterGroupId2,
                            IsRoster = true,
                            VariableName = "c",
                            RosterSizeQuestionId = rosterSizeQuestionId,
                            Children = new List<IComposite>()
                            {
                                Create.NumericRealQuestion(
                                    underDeeperRosterLevelQuestionId,
                                    variable: underDeeperRosterLevelQuestionVariableName
                                )
                            }.ToReadOnlyCollection()

                        }
                    }.ToReadOnlyCollection()

                }
            });

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0019 () =>
            verificationMessages.ShouldContainError("WB0019");

        [NUnit.Framework.Test] public void should_return_message_with_two_references () =>
            verificationMessages.GetError("WB0019").References.Count().Should().Be(2);

        [NUnit.Framework.Test] public void should_return_first_message_reference_with_type_Question () =>
            verificationMessages.GetError("WB0019").References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_first_message_reference_with_id_of_underDeeperPropagationLevelQuestionId () =>
            verificationMessages.GetError("WB0019").References.First().Id.Should().Be(questionWithSubstitutionsId);

        [NUnit.Framework.Test] public void should_return_last_message_reference_with_type_Question () =>
            verificationMessages.GetError("WB0019").References.Last().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_last_message_reference_with_id_of_underDeeperPropagationLevelQuestionVariableName () =>
            verificationMessages.GetError("WB0019").References.Last().Id.Should().Be(underDeeperRosterLevelQuestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionWithSubstitutionsId = Guid.Parse("10000000000000000000000000000000");
        private static Guid underDeeperRosterLevelQuestionId = Guid.Parse("12222222222222222222222222222222");
        private const string underDeeperRosterLevelQuestionVariableName = "i_am_deeper_ddddd_deeper";
    }
}
