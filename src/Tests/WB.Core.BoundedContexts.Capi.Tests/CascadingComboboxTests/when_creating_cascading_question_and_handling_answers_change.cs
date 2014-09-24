using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;

namespace WB.Core.BoundedContexts.Capi.Tests.CascadingComboboxTests
{
    internal class when_creating_cascading_question_and_handling_answers_change : CascadingComboboxQuestionViewTestContext
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
        };

        Because of = () =>
            cascadingCombobox.HandleAnswerListChange();

        It should_return_3_options = () =>
            cascadingCombobox.filteredAnswers.Count().ShouldEqual(3);

        It should_return_options_with_values_1_2_3 = () =>
            cascadingCombobox.filteredAnswers.Select(x => x.Value).ShouldContainOnly(1, 2, 3);

        It should_return_options_with_titles__1__2__3 = () =>
            cascadingCombobox.filteredAnswers.Select(x => x.Title).ShouldContainOnly("o 1", "o 2", "o 3");

        It should_return_options_with_titles__1__2__31 = () =>
            cascadingCombobox.filteredAnswers.Select(x => x.Title).ShouldContainOnly("o 1", "o 2", "o 3");

        private static CascadingComboboxQuestionViewModel cascadingCombobox;
    }
}