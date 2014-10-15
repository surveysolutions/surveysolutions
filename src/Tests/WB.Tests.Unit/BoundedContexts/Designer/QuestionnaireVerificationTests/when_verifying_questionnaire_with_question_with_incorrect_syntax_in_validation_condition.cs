using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_question_with_incorrect_syntax_in_validation_condition : QuestionnaireVerifierTestsContext
    {
        private Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new TextQuestion
                {
                    PublicKey = questionId,
                    ValidationExpression = "[hehe] &=+< 5",
                    ValidationMessage = "validation message",
                    StataExportCaption = "var1"
                });

            var expressionProcessorGeneratorMock = new Mock<IExpressionProcessorGenerator>();
            string resultAssembly;
            expressionProcessorGeneratorMock.Setup(
                _ => _.GenerateProcessorStateAssembly(Moq.It.IsAny<QuestionnaireDocument>(), out resultAssembly))
                .Returns(new GenerationResult(){ Success = false});

            verifier = CreateQuestionnaireVerifier(expressionProcessorGenerator: expressionProcessorGeneratorMock.Object);
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0002__ = () =>
            resultErrors.First().Code.ShouldEqual("WB0002");

        It should_return_error_with_single_reference = () =>
            resultErrors.First().References.Count().ShouldEqual(1);

        It should_return_error_referencing_with_type_of_question = () =>
            resultErrors.First().References.Single().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_error_referencing_with_specified_question_id = () =>
            resultErrors.First().References.Single().Id.ShouldEqual(questionId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
    }
}