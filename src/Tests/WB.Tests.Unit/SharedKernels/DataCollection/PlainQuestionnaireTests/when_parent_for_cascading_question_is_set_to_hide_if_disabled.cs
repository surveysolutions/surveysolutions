using System;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainQuestionnaireTests
{
    internal class when_parent_for_cascading_question_is_set_to_hide_if_disabled : PlainQuestionnaireTestsContext
    {
        Establish context = () =>
        {
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
        };

        Because of = () => shouldBeHidden = questionnaire.ShouldBeHiddenIfDisabled(cascadingQuestionId);

        It should_mark_child_cascading_question_to_be_hidden_as_parent = () => shouldBeHidden.ShouldBeTrue();

        static PlainQuestionnaire questionnaire;
        static Guid cascadingQuestionId;
        static bool shouldBeHidden;
    }
}