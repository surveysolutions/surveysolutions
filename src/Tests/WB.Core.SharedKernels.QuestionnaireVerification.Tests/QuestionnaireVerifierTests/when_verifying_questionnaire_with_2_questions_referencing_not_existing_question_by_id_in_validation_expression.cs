using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Tests.QuestionnaireVerifierTests
{
    internal class when_verifying_questionnaire_with_2_questions_referencing_not_existing_question_by_id_in_validation_expression :  QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            const string ValidationExpressionWithNotExistingQuestion = "[99999999999999999999999999999999] == 2";

            firstIncorrectQuestionId = Guid.Parse("11111111111111111111111111111111");
            secondIncorrectQuestionId = Guid.Parse("22222222222222222222222222222222");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion { PublicKey = firstIncorrectQuestionId, ValidationExpression = ValidationExpressionWithNotExistingQuestion },
                new NumericQuestion { PublicKey = secondIncorrectQuestionId, ValidationExpression = ValidationExpressionWithNotExistingQuestion },
                new TextQuestion { PublicKey = Guid.NewGuid() },
                new Group { PublicKey = Guid.NewGuid() }
            );

            var expressionProcessor = Mock.Of<IExpressionProcessor>(processor
                => processor.IsSyntaxValid(ValidationExpressionWithNotExistingQuestion) == true
                && processor.GetIdentifiersUsedInExpression(ValidationExpressionWithNotExistingQuestion) == new[] { "99999999999999999999999999999999" });

            verifier = CreateQuestionnaireVerifier(expressionProcessor: expressionProcessor);
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_2_errors = () =>
            resultErrors.Count().ShouldEqual(2);

        It should_return_errors_each_with_code__WB0004__ = () =>
            resultErrors.ShouldEachConformTo(error
                => error.Code == "WB0004");

        It should_return_errors_each_having_single_reference = () =>
            resultErrors.ShouldEachConformTo(error
                => error.References.Count() == 1);

        It should_return_errors_each_referencing_question = () =>
            resultErrors.ShouldEachConformTo(error
                => error.References.Single().Type == QuestionnaireVerificationReferenceType.Question);

        It should_return_error_referencing_first_incorrect_question = () =>
            resultErrors.ShouldContain(error
                => error.References.Single().Id == firstIncorrectQuestionId);

        It should_return_error_referencing_second_incorrect_question = () =>
            resultErrors.ShouldContain(error
                => error.References.Single().Id == secondIncorrectQuestionId);

        private static Guid firstIncorrectQuestionId;
        private static Guid secondIncorrectQuestionId;
        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireVerifier verifier;
        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
    }
}