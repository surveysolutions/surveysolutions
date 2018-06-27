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
    class when_verifying_questionnaire_with_categorical_multi_answers_question_that_has_max_allowed_answers_less_than_2 : QuestionnaireVerifierTestsContext
    {

        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaireDocument(Create.MultyOptionsQuestion(
                id: multyOptionsQuestionId,
                variable: "var",
                options: new List<Answer> { new Answer { AnswerValue = "2", AnswerText = "2" }, new Answer { AnswerValue = "1", AnswerText = "1" } },
                maxAllowedAnswers: -1
            ));

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() => 
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_1_message () => 
            verificationMessages.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0061__ () =>
            verificationMessages.Single().Code.Should().Be("WB0061");

        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;

        private static readonly Guid multyOptionsQuestionId = Guid.Parse("10000000000000000000000000000000");
    }
}
