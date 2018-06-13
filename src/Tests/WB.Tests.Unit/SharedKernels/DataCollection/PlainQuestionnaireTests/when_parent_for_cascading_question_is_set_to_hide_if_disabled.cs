using System;
using FluentAssertions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainQuestionnaireTests
{
    internal class when_parent_for_cascading_question_is_set_to_hide_if_disabled : PlainQuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var parentQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            cascadingQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                children: new[]
                {
                    Create.Entity.SingleOptionQuestion(
                        questionId: parentQuestionId,
                        hideIfDisabled: true),
                    Create.Entity.SingleOptionQuestion(
                        questionId: cascadingQuestionId,
                        cascadeFromQuestionId: parentQuestionId)
                }));
            BecauseOf();
        }

        public void BecauseOf() => shouldBeHidden = questionnaire.ShouldBeHiddenIfDisabled(cascadingQuestionId);

        [NUnit.Framework.Test] public void should_mark_child_cascading_question_to_be_hidden_as_parent () => shouldBeHidden.Should().BeTrue();

        static PlainQuestionnaire questionnaire;
        static Guid cascadingQuestionId;
        static bool shouldBeHidden;
    }
}
