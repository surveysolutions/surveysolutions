using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_option_filter_using_own_variable_name : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = Create.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Chapter(children: new IComposite[]
                {
                    Create.MultyOptionsQuestion(variable: "q1", title: "MultiOption",
                        optionsFilterExpression: "q1.Length > 5",
                        options: new List<Answer>
                        {
                            Create.Option(value: "1", text: "option 1"),
                            Create.Option(value: "2", text: "option 2"),
                        })
                })
            });

            var expressionProcessor = Mock.Of<IExpressionProcessor>(_
                    => _.GetIdentifiersUsedInExpression("q1.Length > 5") == new[] { "q1", "Length" }
            );

            verifier = CreateQuestionnaireVerifier(expressionProcessor);
            BecauseOf();
        }

        private void BecauseOf() =>
                verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_not_return_contain_error_WB0056 () =>
                verificationMessages.ShouldNotContainError("WB0056");

        [NUnit.Framework.Test] public void should_not_return_message_with_level_general () =>
                verificationMessages.GetError("WB0056").ShouldBeNull();
        
        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
    }
}