using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    [Ignore("by TLK temporary")]
    internal class when_verifying_questionnaire_with_question_with_incorrect_syntax_in_validation_and_condition : QuestionnaireVerifierTestsContext
    {
        private Establish context = () =>
        {
            const string invalidExpression = "[hehe] &=+< 5";
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new TextQuestion
                {
                    PublicKey = questionId,
                    ValidationExpression = invalidExpression,
                    ConditionExpression = invalidExpression, 
                    ValidationMessage = "validation message",
                    StataExportCaption = "var1"
                });

            verifier = CreateQuestionnaireVerifier(expressionProcessorGenerator: CreateExpressionProcessorGenerator());
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_2_errors = () =>
            resultErrors.Count().ShouldEqual(2);

        It should_return_first_error_with_code__WB0003__ = () =>
            resultErrors.ElementAt(0).Code.ShouldEqual("WB0003");

        It should_return_second_error_with_code__WB0002__ = () =>
            resultErrors.ElementAt(1).Code.ShouldEqual("WB0002");

        It should_return_error_with_single_reference = () =>
            resultErrors.ShouldEachConformTo(error=>error.References.Count() == 1);

        It should_return_error_referencing_with_type_of_question = () =>
            resultErrors.ShouldEachConformTo(error=>error.References.Single().Type == QuestionnaireVerificationReferenceType.Question);

        It should_return_error_referencing_with_specified_question_id = () =>
            resultErrors.ShouldEachConformTo(error=>error.References.Single().Id == questionId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
    }
}