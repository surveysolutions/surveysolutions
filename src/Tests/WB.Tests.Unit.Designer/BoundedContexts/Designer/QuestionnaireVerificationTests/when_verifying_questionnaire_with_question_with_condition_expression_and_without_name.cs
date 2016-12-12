using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_question_with_condition_expression_and_without_name : QuestionnaireVerifierTestsContext
    {
        private Establish context = () =>
        {
            var groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var question1Id = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var question2Id = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                Create.Group(groupId, children: new IComposite[]
                {
                     Create.TextQuestion(question1Id, variable: null, enablementCondition: "b>0"),
                     Create.TextQuestion(question2Id, variable:  "b")
                })
            });

            var expressionProcessor = new Mock<IExpressionProcessor>();

            expressionProcessor
                .Setup(x => x.GetIdentifiersUsedInExpression("b>0"))
                .Returns(new[] { "b" });

            verifier = CreateQuestionnaireVerifier(expressionProcessor.Object);
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_message_with_code__WB0057 = () =>
            verificationMessages.ShouldContainError("WB0057");

        It should_return_message_with_level_general = () =>
            verificationMessages.GetError("WB0057").MessageLevel.ShouldEqual(VerificationMessageLevel.General);

        It should_return_message_with_one_references = () =>
            verificationMessages.GetError("WB0057").References.Count().ShouldEqual(1);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
    }
}