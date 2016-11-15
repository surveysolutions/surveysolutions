using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_question_with_empty_and_too_long_validation_messages_and_expressions : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
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
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)).ToList();

        It should_return_messages_with_codes__WB0104___WB0105___WB0106__and__WB0107__ = () =>
            verificationMessages.Select(message => message.Code).ShouldContain("WB0104", "WB0105", "WB0106", "WB0107");

        It should_return_validation_condition_index_in_message_WB0106 = () =>
            verificationMessages.Single(message => message.Code == "WB0106").Message.ShouldContain("#2");

        It should_return_validation_condition_index_in_message_WB0107 = () =>
            verificationMessages.Single(message => message.Code == "WB0107").Message.ShouldContain("#3");

        It should_return_validation_condition_index_in_message_WB0104 = () =>
            verificationMessages.Single(message => message.Code == "WB0104").Message.ShouldContain("#4");

        It should_return_validation_condition_index_in_message_WB0105 = () =>
            verificationMessages.Single(message => message.Code == "WB0105").Message.ShouldContain("#5");

        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireVerifier verifier;
        private static List<QuestionnaireVerificationMessage> verificationMessages;
    }
}