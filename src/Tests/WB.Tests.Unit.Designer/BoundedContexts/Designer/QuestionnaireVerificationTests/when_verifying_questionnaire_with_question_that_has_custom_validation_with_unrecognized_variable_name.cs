using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    [Ignore("reference validation is turned off")]
    internal class when_verifying_questionnaire_with_question_that_has_custom_validation_with_unrecognized_variable_name : QuestionnaireVerifierTestsContext
    {
       [NUnit.Framework.OneTimeSetUp] public void context () {
            questionWithCustomValidation = Guid.Parse("10000000000000000000000000000000");
            questionnaire = CreateQuestionnaireDocument(new SingleQuestion()
            {
                PublicKey = questionWithCustomValidation,
                ValidationExpression = "some random expression",
                ValidationMessage = "some random message",
                StataExportCaption = "var",
                Answers = { new Answer() { AnswerValue = "1", AnswerText = "opt 1" }, new Answer() { AnswerValue = "2", AnswerText = "opt 2" } }
            });

            var expressionProcessor = new Mock<IExpressionProcessor>();

            expressionProcessor.Setup(x => x.GetIdentifiersUsedInExpression(Moq.It.IsAny<string>()))
                .Returns(new string[] { UnrecognizableVariableName });

            verifier = CreateQuestionnaireVerifier(expressionProcessor.Object);
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_1_message () =>
            verificationMessages.Count().ShouldEqual(1);

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0020 () =>
            verificationMessages.Single().Code.ShouldEqual("WB0004");

        [NUnit.Framework.Test] public void should_return_message_with_1_references () =>
            verificationMessages.Single().References.Count().ShouldEqual(1);

        [NUnit.Framework.Test] public void should_return_first_message_reference_with_type_Question () =>
            verificationMessages.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_first_message_reference_with_id_of_questionWithCustomValidation () =>
            verificationMessages.Single().References.First().Id.ShouldEqual(questionWithCustomValidation);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionWithCustomValidation;
        private const string UnrecognizableVariableName = "unrecognizable parameter";

    }
}
