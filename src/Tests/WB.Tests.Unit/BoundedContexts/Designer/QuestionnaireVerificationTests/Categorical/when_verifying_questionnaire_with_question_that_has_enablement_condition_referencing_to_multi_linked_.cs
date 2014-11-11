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

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests.Categorical
{
    internal class when_verifying_questionnaire_with_question_that_has_enablement_condition_referencing_to_categorical_multi_linked_question : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            var linkedSourceQuestionId = Guid.Parse("33333333333333333333333333333333");
            questionnaire = CreateQuestionnaireDocument(
                new Group()
                {
                    IsRoster = true,
                    VariableName = "a",
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    RosterFixedTitles = new[] { "fixed title 1" },
                    Children =
                    {
                        new TextQuestion()
                        {
                            PublicKey = linkedSourceQuestionId,
                            QuestionType = QuestionType.Text,
                            StataExportCaption = "var"
                        }
                    }
                },
                new MultyOptionsQuestion()
                {
                    PublicKey = categoricalQuestionId,
                    StataExportCaption = "var1",
                    LinkedToQuestionId = linkedSourceQuestionId
                },
                new NumericQuestion
                {
                    PublicKey = questionWithEnablementConditionId,
                    ConditionExpression = "some condition",
                    StataExportCaption = "var2"
                });

            var expressionProcessor = Mock.Of<IExpressionProcessor>(processor
                => processor.GetIdentifiersUsedInExpression(Moq.It.IsAny<string>()) == new[] { categoricalQuestionId.ToString() });

            verifier = CreateQuestionnaireVerifier(expressionProcessor);
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0064__ = () =>
            resultErrors.Single().Code.ShouldEqual("WB0064");

        It should_return_error_with_two_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(2);

        It should_return_first_error_reference_with_type_Question = () =>
            resultErrors.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_first_error_reference_with_id_of_question_with_enablement_condition = () =>
            resultErrors.Single().References.First().Id.ShouldEqual(questionWithEnablementConditionId);

        It should_return_second_error_reference_with_type_Question = () =>
            ShouldExtensionMethods.ShouldEqual(resultErrors.Single().References.Second().Type, QuestionnaireVerificationReferenceType.Question);

        It should_return_second_error_reference_with_id_of_categorical_multi_linked_question = () =>
            ShouldExtensionMethods.ShouldEqual(resultErrors.Single().References.Second().Id, categoricalQuestionId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionWithEnablementConditionId = Guid.Parse("10000000000000000000000000000000");
        private static Guid categoricalQuestionId = Guid.Parse("12222222222222222222222222222222");
    }
}