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
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_questionnaire_has_question_statictext_and_group_conditionally_enabled_by_future_question: QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            var futureCondition = "q2==\"test\"";

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.GpsCoordinateQuestion(),
                Create.StaticText(staticTextId: staticextId, enablementCondition: futureCondition),
                Create.Group(groupId: groupId, enablementCondition: futureCondition, children: new[]
                {
                    Create.Question(questionId: questionId, questionType: QuestionType.DateTime,
                        variable: "q1", enablementCondition: futureCondition)
                }),
                Create.Question(questionId: futureQuestionId, questionType: QuestionType.Text, variable: "q2"));

            var expressionProcessor = Mock.Of<IExpressionProcessor>(processor
                => processor.GetIdentifiersUsedInExpression(futureCondition) ==
                   new[] { "q2" });

            verifier = CreateQuestionnaireVerifier(expressionProcessor: expressionProcessor);
        };

        Because of = () => errors = verifier.Verify(questionnaire);

        It should_return_WB0251_warning = () => errors.ShouldContainWarning("WB0251");

        It should_return_WB0251_message_with_appropriate_message = () =>
            errors.ShouldContain(
                x => x.Message == "Enablement condition refers to a future question. Consider reversing the order.");

        It should_return_first_message_with_first_references_on_static_text_with_enablement_condition = () =>
           errors.Skip(1).First().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.StaticText);

        It should_return_first_message_with_first_references_on_static_text_id_with_enablement_condition = () =>
            errors.Skip(1).First().References.First().Id.ShouldEqual(staticextId);

        It should_return_first_message_with_second_references_on_future_question = () =>
            errors.Skip(1).First().References.Last().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_first_message_with_second_references_on_future_question_id = () =>
            errors.Skip(1).First().References.Last().Id.ShouldEqual(futureQuestionId);

        It should_return_second_message_with_first_references_on_group_with_enablement_condition = () =>
            errors.Skip(2).First().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Group);

        It should_return_second_message_with_first_references_on_group_id_with_enablement_condition = () =>
            errors.Skip(2).First().References.First().Id.ShouldEqual(groupId);

        It should_return_second_message_with_second_references_on_future_question = () =>
            errors.Skip(2).First().References.Last().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_second_message_with_second_references_on_future_question_id = () =>
            errors.Skip(2).First().References.Last().Id.ShouldEqual(futureQuestionId);

        It should_return_third_message_with_first_references_on_question_with_enablement_condition = () =>
           errors.Last().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_third_message_with_first_references_on_question_id_with_enablement_condition = () =>
            errors.Last().References.First().Id.ShouldEqual(questionId);

        It should_return_third_message_with_second_references_on_future_question = () =>
            errors.Last().References.Last().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_third_message_with_second_references_on_future_question_id = () =>
            errors.Last().References.Last().Id.ShouldEqual(futureQuestionId);

        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationMessage> errors;
        private static Guid questionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid futureQuestionId = Guid.Parse("1111DDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid staticextId = Guid.Parse("2222DDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid groupId = Guid.Parse("3333DDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}