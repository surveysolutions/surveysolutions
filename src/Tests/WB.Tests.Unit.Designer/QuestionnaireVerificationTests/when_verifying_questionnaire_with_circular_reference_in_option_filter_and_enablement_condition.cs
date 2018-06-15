using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;


namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_circular_reference_in_option_filter_and_enablement_condition : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = Create.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Chapter(children: new IComposite[]
                {
                    Create.NumericIntegerQuestion(question1Id, "q1", enablementCondition: "q2.Count() > 2"),
                    Create.SingleQuestion(question2Id, variable: "q2", optionsFilter: "@optioncode < q1")
                })
            });

            var expressionProcessor = Mock.Of<IExpressionProcessor>(_
                => _.GetIdentifiersUsedInExpression("q2.Count() > 2") == new[] { "q2" }
                && _.GetIdentifiersUsedInExpression("@optioncode < q1") == new[] { "@optioncode", "q1" }
            );

            verifier = CreateQuestionnaireVerifier(expressionProcessor);
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_contain_error_WB0056 () =>
            verificationMessages.ShouldContainError("WB0056");

        [NUnit.Framework.Test] public void should_return_message_with_level_general () =>
            verificationMessages.GetError("WB0056").MessageLevel.Should().Be(VerificationMessageLevel.General);

        [NUnit.Framework.Test] public void should_return_message_with_two_references () =>
            verificationMessages.GetError("WB0056").References.Count().Should().Be(2);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static readonly Guid question1Id = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid question2Id = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}