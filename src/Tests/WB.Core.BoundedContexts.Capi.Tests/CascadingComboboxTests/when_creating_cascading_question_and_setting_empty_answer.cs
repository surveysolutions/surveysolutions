using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;

namespace WB.Core.BoundedContexts.Capi.Tests.CascadingComboboxTests
{
    internal class when_creating_cascading_question_and_setting_empty_answer : CascadingComboboxQuestionViewTestContext
    {
        Establish context = () =>
        {
            Func<decimal[], IEnumerable<AnswerViewModel>> getAnswerOptions = (questionRosterVecor) => new List<AnswerViewModel>
            {
                new AnswerViewModel(Guid.NewGuid(), "o 1", "1", false, null),
                new AnswerViewModel(Guid.NewGuid(), "o 2", "2", false, null),
                new AnswerViewModel(Guid.NewGuid(), "o 3", "3", false, null),
            };
            cascadingCombobox = CreateCascadingComboboxQuestionViewModel(getAnswerOptions);

            cascadingCombobox.HandleAnswerListChange();
        };

        Because of = () =>
            cascadingCombobox.SetAnswer("");

        It should_not_find_any_selected_option = () =>
            cascadingCombobox.filteredAnswers.Any(x => x.Selected).ShouldBeFalse();

        It should_set_non_answered_status = () =>
            cascadingCombobox.Status.ShouldEqual(QuestionStatus.Enabled | QuestionStatus.ParentEnabled | QuestionStatus.Valid);

        private static CascadingComboboxQuestionViewModel cascadingCombobox;
    }
}