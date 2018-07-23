using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;


namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_questionnaire_has_question_statictext_and_group_conditionally_enabled_by_future_question: QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
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
            BecauseOf();
        }

        private void BecauseOf() => errors = verifier.Verify(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_WB0251_warning () => errors.ShouldContainWarning("WB0251", "Enablement condition refers to a future question. Consider reversing the order.");

        [NUnit.Framework.Test] public void should_return_warning_for_static_text () =>
            FindWarningForEntityWithId(errors, "WB0251", staticextId).References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.StaticText);

        [NUnit.Framework.Test] public void should_return_warning_for_static_text_with_reference_on_future_question () =>
            FindWarningForEntityWithId(errors, "WB0251", staticextId).References.Last().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_warning_for_static_text_with_reference_on_future_question_id () =>
            FindWarningForEntityWithId(errors, "WB0251", staticextId).References.Last().Id.Should().Be(futureQuestionId);

        [NUnit.Framework.Test] public void should_return_warning_for_group () =>
            FindWarningForEntityWithId(errors, "WB0251", groupId).References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Group);

        [NUnit.Framework.Test] public void should_return_warning_for_group_with_reference_on_future_question () =>
            FindWarningForEntityWithId(errors, "WB0251", groupId).References.Last().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_warning_for_group_with_reference_on_future_question_id () =>
            FindWarningForEntityWithId(errors, "WB0251", groupId).References.Last().Id.Should().Be(futureQuestionId);

        [NUnit.Framework.Test]
        public void should_return_warning_for_question() =>
            FindWarningForEntityWithId(errors, "WB0251", questionId).References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_warning_for_question_with_reference_on_future_question () =>
            FindWarningForEntityWithId(errors, "WB0251", questionId).References.Last().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_warning_for_question_with_reference_on_future_question_id () =>
            FindWarningForEntityWithId(errors, "WB0251", questionId).References.Last().Id.Should().Be(futureQuestionId);

        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationMessage> errors;
        private static Guid questionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid futureQuestionId = Guid.Parse("1111DDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid staticextId = Guid.Parse("2222DDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid groupId = Guid.Parse("3333DDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}