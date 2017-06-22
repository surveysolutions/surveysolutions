using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    class when_verifying_questionnaire_with_categorical_multi_answers_question_that_has_max_allowed_answers_count_more_than_options_count : QuestionnaireVerifierTestsContext
    {

        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaireDocument(Create.MultyOptionsQuestion(
                multyOptionsQuestionId,
                options: new List<Answer>() { new Answer() { AnswerValue = "2", AnswerText = "2" }, new Answer() { AnswerValue = "1", AnswerText = "1" } },
                maxAllowedAnswers: 3,
                variable: "var1"
            ));

            verifier = CreateQuestionnaireVerifier();
        }

        private void BecauseOf() => 
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)).ToList();

        [NUnit.Framework.Test] public void should_return_1_message () => 
            verificationMessages.Count().ShouldEqual(1);

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0021__ () =>
            verificationMessages.Single().Code.ShouldEqual("WB0021");

        [NUnit.Framework.Test] public void should_return_message_with_level_general () =>
            verificationMessages.Single().MessageLevel.ShouldEqual(VerificationMessageLevel.General);


        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;

        private static Guid multyOptionsQuestionId = Guid.Parse("10000000000000000000000000000000");
    }
}
