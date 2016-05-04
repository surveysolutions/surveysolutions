using System;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainQuestionnaireTests
{
    internal class when_parent_for_cascading_question_is_set_to_hide_if_disabled : PlainQuestionnaireTestsContext
    {
        Establish context = () =>
        {
            var parentQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            cascadingQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            questionnaire = Create.PlainQuestionnaire(Create.QuestionnaireDocumentWithOneChapter(
                children: new[]
                {
                    Create.SingleOptionQuestion(
                        questionId: parentQuestionId,
                        hideIfDisabled: true),
                    Create.SingleOptionQuestion(
                        questionId: cascadingQuestionId,
                        cascadeFromQuestionId: parentQuestionId)
                }));
        };

        Because of = () => shouldBeHidden = questionnaire.ShouldBeHiddenIfDisabled(cascadingQuestionId);

        It should_mark_child_cascading_question_to_be_hidden_as_parent = () => shouldBeHidden.ShouldBeTrue();

        static PlainQuestionnaire questionnaire;
        static Guid cascadingQuestionId;
        static bool shouldBeHidden;
    }
}