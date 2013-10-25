using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects.Verification;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireVerifierTests
{
    internal class when_verifying_questionnaire_with_3_questions_with_invalid_validation_expression_and_with_2_with_correct : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            const string InvalidExpression = "[hehe] &=+< 5";
            const string ValidExpression = "[33333333333333333333333333333333] == 2";

            firstIncorrectQuestionId = Guid.Parse("1111CCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            secondIncorrectQuestionId = Guid.Parse("2222CCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            thirdIncorrectQuestionId = Guid.Parse("3333CCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            firstCorrectQuestionId = Guid.Parse("1111EEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            secondCorrectQuestionId = Guid.Parse("2222EEEEEEEEEEEEEEEEEEEEEEEEEEEE");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion { PublicKey = firstIncorrectQuestionId, ValidationExpression = InvalidExpression },
                new NumericQuestion { PublicKey = secondIncorrectQuestionId, ValidationExpression = InvalidExpression },
                new NumericQuestion { PublicKey = thirdIncorrectQuestionId, ValidationExpression = InvalidExpression },
                new NumericQuestion { PublicKey = firstCorrectQuestionId, ValidationExpression = ValidExpression },
                new NumericQuestion { PublicKey = secondCorrectQuestionId, ValidationExpression = ValidExpression }
            );

            var expressionProcessor = Mock.Of<IExpressionProcessor>(processor
                => processor.IsSyntaxValid(InvalidExpression) == false
                && processor.IsSyntaxValid(ValidExpression) == true);

            verifier = CreateQuestionnaireVerifier(expressionProcessor: expressionProcessor);
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_3_errors = () =>
            resultErrors.Count().ShouldEqual(3);

        It should_return_errors_each_with_code__WB0002__ = () =>
            resultErrors.ShouldEachConformTo(error
                => error.Code == "WB0002");

        It should_return_errors_each_having_single_reference = () =>
            resultErrors.ShouldEachConformTo(error
                => error.References.Count() == 1);

        It should_return_errors_each_referencing_question = () =>
            resultErrors.ShouldEachConformTo(error
                => error.References.Single().Type == QuestionnaireVerificationReferenceType.Question);

        It should_return_error_referencing_first_incorrect_question = () =>
            resultErrors.ShouldContain(error
                => error.References.Single().Id == firstIncorrectQuestionId);

        It should_return_error_referencing_secong_incorrect_question = () =>
            resultErrors.ShouldContain(error
                => error.References.Single().Id == secondIncorrectQuestionId);

        It should_return_error_referencing_third_incorrect_question = () =>
            resultErrors.ShouldContain(error
                => error.References.Single().Id == thirdIncorrectQuestionId);

        It should_not_return_error_referencing_first_correct_question = () =>
            resultErrors.ShouldNotContain(error
                => error.References.Single().Id == firstCorrectQuestionId);

        It should_not_return_error_referencing_second_correct_question = () =>
            resultErrors.ShouldNotContain(error
                => error.References.Single().Id == secondCorrectQuestionId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid firstIncorrectQuestionId;
        private static Guid secondIncorrectQuestionId;
        private static Guid thirdIncorrectQuestionId;
        private static Guid firstCorrectQuestionId;
        private static Guid secondCorrectQuestionId;
    }
}