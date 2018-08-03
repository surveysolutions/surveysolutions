using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_question_with_empty_and_too_long_validation_messages_and_expressions : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.TextQuestion(validationConditions: new[]
                {
                    Create.ValidationCondition("Expression 1", "Message 1"),
                    Create.ValidationCondition(message: "Message 2", expression: string.Empty),
                    Create.ValidationCondition(message: string.Empty, expression: "Expression 3"),
                    Create.ValidationCondition(message: "Message 4", expression: "Expression 4 very long" + Enumerable.Range(1, 1000).Select(_ => " very long").Aggregate(string.Empty, (current, delta) => current + delta)),
                    Create.ValidationCondition(expression: "Expression 5", message: "Message 5 very long very very long very very long very very long very very long very very long very very long very very long very very long very very long very very long very very long very very long very very long very very long very very long very very long very very long very very long"),
                })
            });

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)).ToList();

        [NUnit.Framework.Test] public void should_return_messages_with_codes__WB0104___WB0105___WB0106__ () =>
            verificationMessages.Select(message => message.Code).Should().Contain("WB0104", "WB0105", "WB0106");

        [NUnit.Framework.Test] public void should_return_validation_condition_index_in_message_WB0106 () =>
            verificationMessages.Single(message => message.Code == "WB0106").Message.Should().Contain("#2");
        
        [NUnit.Framework.Test] public void should_return_validation_condition_index_in_message_WB0104 () =>
            verificationMessages.Single(message => message.Code == "WB0104").Message.Should().Contain("#4");

        [NUnit.Framework.Test] public void should_return_validation_condition_index_in_message_WB0105 () =>
            verificationMessages.Single(message => message.Code == "WB0105").Message.Should().Contain("#5");

        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireVerifier verifier;
        private static List<QuestionnaireVerificationMessage> verificationMessages;
    }
}
