using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_single_question_with_not_unique_titles_and_values : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            singleQuestionId = Guid.Parse("10000000000000000000000000000000");
            questionnaire = CreateQuestionnaireDocument(
                Create.SingleOptionQuestion(
                singleQuestionId,
                variable: "var",
                answers: new List<Answer>()
                {
                    new Answer { AnswerValue = "1", AnswerText = "1" }, 
                    new Answer { AnswerValue = "1", AnswerText = "1" }
                }
            ));

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_2_messages () =>
            verificationMessages.Count().Should().Be(2);

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0022 () =>
            verificationMessages.Select(x => x.Code).Should().BeEquivalentTo("WB0072", "WB0073");

        [NUnit.Framework.Test] public void should_return_first_error_with_1_references () =>
            verificationMessages.First().References.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_second_error_with_1_references () =>
            verificationMessages.Last().References.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_first_message_reference_with_type_Question () =>
            verificationMessages.First().References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_second_message_reference_with_type_Question () =>
            verificationMessages.Last().References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_first_message_reference_with_id_equals_singleQuestionId () =>
            verificationMessages.First().References.First().Id.Should().Be(singleQuestionId);

        [NUnit.Framework.Test] public void should_return_second_message_reference_with_id_equals_singleQuestionId () =>
            verificationMessages.Last().References.First().Id.Should().Be(singleQuestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid singleQuestionId;
    }
}
