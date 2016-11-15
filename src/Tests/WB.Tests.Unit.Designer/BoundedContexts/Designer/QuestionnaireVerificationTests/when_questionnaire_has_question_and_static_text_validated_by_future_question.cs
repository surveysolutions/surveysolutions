using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_questionnaire_has_question_and_static_text_validated_by_future_question :
        QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            var futureVerifiction = "q2==\"test\"";

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.GpsCoordinateQuestion(),
                Create.StaticText(staticTextId: staticextId,
                    validationConditions: new[] {Create.ValidationCondition(futureVerifiction)}),
                Create.Question(questionId: questionId, questionType: QuestionType.DateTime,
                    validationExpression: futureVerifiction, variable: "q1", validationMessage: "test"),
                Create.Question(questionId: futureQuestionId, questionType: QuestionType.Text, variable: "q2"));

            var expressionProcessor = Mock.Of<IExpressionProcessor>(processor
                => processor.GetIdentifiersUsedInExpression(futureVerifiction) ==
                   new[] {"q2"});

            verifier = CreateQuestionnaireVerifier(expressionProcessor: expressionProcessor);
        };

        Because of = () => errors = verifier.Verify(Create.QuestionnaireView(questionnaire));

        It should_return_WB0250_warning = () => errors.ShouldContainWarning("WB0250", "Validation condition #1 refers to a future question. Consider reversing the order.");

        It should_return_warning_for_static_text = () =>
            FindWarningForEntityWithId(errors, "WB0250", staticextId).References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.StaticText);

        It should_return_warning_for_static_text_with_reference_on_future_question = () =>
            FindWarningForEntityWithId(errors, "WB0250", staticextId).References.Last().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_warning_for_static_text_with_reference_on_future_question_id = () =>
            FindWarningForEntityWithId(errors, "WB0250", staticextId).References.Last().Id.ShouldEqual(futureQuestionId);

        It should_return_warning_for_question = () =>
            FindWarningForEntityWithId(errors, "WB0250", questionId).References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_warning_for_question_with_reference_on_future_question = () =>
            FindWarningForEntityWithId(errors, "WB0250", questionId).References.Last().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_warning_for_question_with_reference_on_future_question_id = () =>
            FindWarningForEntityWithId(errors, "WB0250", questionId).References.Last().Id.ShouldEqual(futureQuestionId);

        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationMessage> errors;
        private static Guid questionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid futureQuestionId = Guid.Parse("1111DDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid staticextId = Guid.Parse("2222DDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}