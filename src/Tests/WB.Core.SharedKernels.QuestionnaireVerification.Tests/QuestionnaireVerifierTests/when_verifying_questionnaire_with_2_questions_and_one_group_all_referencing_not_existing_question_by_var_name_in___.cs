using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    internal class when_verifying_questionnaire_with_2_questions_and_one_group_all_referencing_not_existing_question_by_var_name_in_enablement_condition : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            const string EnablementConditionWithNotExistingQuestion = "[99999999999999999999999999999999] == 2";

            firstIncorrectQuestionId = Guid.Parse("11111111111111111111111111111111");
            secondIncorrectQuestionId = Guid.Parse("22222222222222222222222222222222");
            incorrectGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion { PublicKey = firstIncorrectQuestionId, ConditionExpression = EnablementConditionWithNotExistingQuestion },
                new NumericQuestion { PublicKey = secondIncorrectQuestionId, ConditionExpression = EnablementConditionWithNotExistingQuestion },
                new Group { PublicKey = incorrectGroupId, ConditionExpression = EnablementConditionWithNotExistingQuestion },
                new TextQuestion { PublicKey = Guid.NewGuid() },
                new Group { PublicKey = Guid.NewGuid() }
            );

            var expressionProcessor = Mock.Of<IExpressionProcessor>(processor
                => processor.IsSyntaxValid(EnablementConditionWithNotExistingQuestion) == true
                && processor.GetIdentifiersUsedInExpression(EnablementConditionWithNotExistingQuestion) == new[] { "notExistingVariableName" });

            verifier = CreateQuestionnaireVerifier(expressionProcessor: expressionProcessor);
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_3_errors = () =>
            resultErrors.Count().ShouldEqual(3);

        It should_return_errors_each_with_code__WB0005__ = () =>
            resultErrors.ShouldEachConformTo(error
                => error.Code == "WB0005");

        It should_return_errors_each_having_single_reference = () =>
            resultErrors.ShouldEachConformTo(error
                => error.References.Count() == 1);

        It should_return_error_referencing_first_incorrect_question = () =>
            resultErrors.ShouldContain(error
                => error.References.Single().Type == QuestionnaireVerificationReferenceType.Question
                && error.References.Single().Id == firstIncorrectQuestionId);

        It should_return_error_referencing_second_incorrect_question = () =>
            resultErrors.ShouldContain(error
                => error.References.Single().Type == QuestionnaireVerificationReferenceType.Question
                && error.References.Single().Id == secondIncorrectQuestionId);

        It should_return_error_referencing_incorrect_group = () =>
            resultErrors.ShouldContain(error
                => error.References.Single().Type == QuestionnaireVerificationReferenceType.Group
                && error.References.Single().Id == incorrectGroupId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid firstIncorrectQuestionId;
        private static Guid secondIncorrectQuestionId;
        private static Guid incorrectGroupId;
    }
}
