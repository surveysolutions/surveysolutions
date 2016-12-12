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

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () => errors = verifier.Verify(Create.QuestionnaireView(questionnaire));

        It should_return_WB0251_warning = () => errors.ShouldContainWarning("WB0251", "Enablement condition refers to a future question. Consider reversing the order.");

        It should_return_warning_for_static_text = () =>
            FindWarningForEntityWithId(errors, "WB0251", staticextId).References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.StaticText);

        It should_return_warning_for_static_text_with_reference_on_future_question = () =>
            FindWarningForEntityWithId(errors, "WB0251", staticextId).References.Last().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_warning_for_static_text_with_reference_on_future_question_id = () =>
            FindWarningForEntityWithId(errors, "WB0251", staticextId).References.Last().Id.ShouldEqual(futureQuestionId);

        It should_return_warning_for_group = () =>
            FindWarningForEntityWithId(errors, "WB0251", groupId).References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Group);

        It should_return_warning_for_group_with_reference_on_future_question = () =>
            FindWarningForEntityWithId(errors, "WB0251", groupId).References.Last().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_warning_for_group_with_reference_on_future_question_id = () =>
            FindWarningForEntityWithId(errors, "WB0251", groupId).References.Last().Id.ShouldEqual(futureQuestionId);

        It should_return_warning_for_question= () =>
            FindWarningForEntityWithId(errors, "WB0251", questionId).References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_warning_for_question_with_reference_on_future_question = () =>
            FindWarningForEntityWithId(errors, "WB0251", questionId).References.Last().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_warning_for_question_with_reference_on_future_question_id = () =>
            FindWarningForEntityWithId(errors, "WB0251", questionId).References.Last().Id.ShouldEqual(futureQuestionId);

        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationMessage> errors;
        private static Guid questionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid futureQuestionId = Guid.Parse("1111DDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid staticextId = Guid.Parse("2222DDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid groupId = Guid.Parse("3333DDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}