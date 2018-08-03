using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_2_orphan_roster_groups_they_must_be_treated_as_grops_with_separate_roster_levels : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context() {
            rosterGroupId1 = Guid.Parse("13333333333333333333333333333333");
            rosterGroupId2 = Guid.Parse("11111111111111111111111111111111");
            questionWithSubstitutionsIdFromLevel1 = Guid.Parse("12222222222222222222222222222222");
            questionFromLevel2 = Guid.Parse("66666666666666666666666666666666");

            var rosterQuestionId1 = Guid.Parse("44444444444444444444444444444444");
            var rosterQuestionId2 = Guid.Parse("55555555555555555555555555555555");

            questionnaire = CreateQuestionnaireDocument(
                Create.NumericIntegerQuestion(rosterQuestionId1, variable: "var1"),
                Create.NumericIntegerQuestion(rosterQuestionId2, variable: "var2"),
                Create.NumericRoster(rosterGroupId1, variable: "b", rosterSizeQuestionId: rosterQuestionId1, children: new IComposite[]
                    {
                        Create.NumericIntegerQuestion(
                            questionWithSubstitutionsIdFromLevel1,
                            title: $"hello %i_am_from_level2%",
                            variable: "var3")
                    }
                ),
                Create.NumericRoster(rosterGroupId2, variable: "a", rosterSizeQuestionId: rosterQuestionId2, children: new IComposite[] {
                    Create.SingleOptionQuestion(
                        questionFromLevel2,
                        variable: "i_am_from_level2",
                        answers:new List<Answer> { new Answer() { AnswerValue = "1", AnswerText = "opt 1" }, new Answer() { AnswerValue = "2", AnswerText = "opt 2" } }
                    )}
                )); 

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_first_message_with_code__WB0019 () =>
            verificationMessages.ShouldContainError("WB0019");

        [NUnit.Framework.Test] public void should_return_WB0019_message_with_2_references_on_questions () =>
            verificationMessages.GetError("WB0019")
                .References.ToList()
                .ForEach(question => question.Type.Should().Be(QuestionnaireVerificationReferenceType.Question));

        [NUnit.Framework.Test] public void should_return_WB0019_message_with_first_reference_to_question_with_substitution_text () =>
            verificationMessages.GetError("WB0019").References.ElementAt(0).Id.Should().Be(questionWithSubstitutionsIdFromLevel1);

        [NUnit.Framework.Test] public void should_return_WB0019_message_with_second_reference_to_question_that_used_as_substitution_question () =>
            verificationMessages.GetError("WB0019").References.ElementAt(1).Id.Should().Be(questionFromLevel2);


        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionWithSubstitutionsIdFromLevel1;
        private static Guid questionFromLevel2;
        private static Guid rosterGroupId1;
        private static Guid rosterGroupId2;
    }
}
