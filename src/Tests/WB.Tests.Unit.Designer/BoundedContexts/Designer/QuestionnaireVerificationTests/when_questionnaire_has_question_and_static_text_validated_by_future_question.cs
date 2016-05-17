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

        Because of = () => errors = verifier.Verify(questionnaire);

        It should_return_WB0250_warning = () => errors.ShouldContainWarning("WB0250");

        It should_return_WB0250_message_with_appropriate_message = () =>
            errors.ShouldContain(
                x => x.Message == "Validation condition #1 refers to a future question. Consider reversing the order.");

        It should_return_first_message_with_first_references_on_question_with_validation = () =>
           errors.Skip(1).First().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.StaticText);

        It should_return_first_message_with_first_references_on_question_id_with_validation = () =>
            errors.Skip(1).First().References.First().Id.ShouldEqual(staticextId);

        It should_return_first_message_with_second_references_on_future_question = () =>
            errors.Skip(1).First().References.Last().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_first_message_with_second_references_on_future_question_id = () =>
            errors.Skip(1).First().References.Last().Id.ShouldEqual(futureQuestionId);

        It should_return_second_message_with_first_references_on_question_with_validation = () =>
            errors.Last().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_second_message_with_first_references_on_question_id_with_validation = () =>
            errors.Last().References.First().Id.ShouldEqual(questionId);

        It should_return_second_message_with_second_references_on_future_question = () =>
            errors.Last().References.Last().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_second_message_with_second_references_on_future_question_id = () =>
            errors.Last().References.Last().Id.ShouldEqual(futureQuestionId);

        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationMessage> errors;
        private static Guid questionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid futureQuestionId = Guid.Parse("1111DDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid staticextId = Guid.Parse("2222DDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}