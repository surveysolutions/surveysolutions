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
using WB.Core.GenericSubdomains.Portable;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_2_orphan_roster_groups_they_must_be_treated_as_grops_with_separate_roster_levels : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            rosterGroupId1 = Guid.Parse("13333333333333333333333333333333");
            rosterGroupId2 = Guid.Parse("10000000000000000000000000000000");
            questionWithSubstitutionsIdFromLevel1 = Guid.Parse("12222222222222222222222222222222");
            questionFromLevel2 = Guid.Parse("11133333333333333333333333333333");

            var rosterQuestionId1 = Guid.Parse("11000000000000000000000000000000");
            var rosterQuestionId2 = Guid.Parse("11100000000000000000000000000000");

            questionnaire = CreateQuestionnaireDocument(
                Create.NumericIntegerQuestion(
                    rosterQuestionId1,
                    variable: "var1"
                ),
                Create.NumericIntegerQuestion(
                    rosterQuestionId2,
                    variable: "var2"
                ),
                new Group()
                {
                    PublicKey = rosterGroupId1, IsRoster = true, VariableName = "b", RosterSizeQuestionId = rosterQuestionId1,
                    Children = new IComposite[]
                    {
                        Create.NumericIntegerQuestion(
                            questionWithSubstitutionsIdFromLevel1,
                            title: $"hello %{questionSubstitutionsSourceFromLevel2VariableName}%",
                            variable: "var3"
                        )
                    }.ToReadOnlyCollection()
                },
                
                new Group()
                {
                    PublicKey = rosterGroupId2, IsRoster = true, VariableName = "a", RosterSizeQuestionId = rosterQuestionId2,
                    Children = new IComposite[] {
                        Create.SingleOptionQuestion(
                        
                            questionFromLevel2,
                        variable: questionSubstitutionsSourceFromLevel2VariableName,
                        answers:new List<Answer> { new Answer() { AnswerValue = "1", AnswerText = "opt 1" }, new Answer() { AnswerValue = "2", AnswerText = "opt 2" } }
                        )}.ToReadOnlyCollection()
                }); 

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_1_message () =>
            verificationMessages.Count().ShouldEqual(1);

        [NUnit.Framework.Test] public void should_return_first_message_with_code__WB0019 () =>
            verificationMessages.Single().Code.ShouldEqual("WB0019");

        [NUnit.Framework.Test] public void should_return_WB0019_message_with_2_references_on_questions () =>
            verificationMessages.Single()
                .References.ToList()
                .ForEach(question => question.Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question));

        [NUnit.Framework.Test] public void should_return_WB0019_message_with_first_reference_to_question_with_substitution_text () =>
            verificationMessages.Single().References.ElementAt(0).Id.ShouldEqual(questionWithSubstitutionsIdFromLevel1);

        [NUnit.Framework.Test] public void should_return_WB0019_message_with_second_reference_to_question_that_used_as_substitution_question () =>
            verificationMessages.Single().References.ElementAt(1).Id.ShouldEqual(questionFromLevel2);


        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionWithSubstitutionsIdFromLevel1;
        private static Guid questionFromLevel2;
        private static Guid rosterGroupId1;
        private static Guid rosterGroupId2;
        private const string questionSubstitutionsSourceFromLevel2VariableName = "i_am_from_level2";
    }
}
