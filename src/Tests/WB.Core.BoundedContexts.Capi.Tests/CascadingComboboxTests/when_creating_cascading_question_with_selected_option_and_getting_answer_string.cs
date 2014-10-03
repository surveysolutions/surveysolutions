using System;
using System.Collections.Generic;
using Machine.Specifications;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;

namespace WB.Core.BoundedContexts.Capi.Tests.CascadingComboboxTests
{
    internal class when_creating_cascading_question_with_selected_option_and_getting_answer_string : CascadingComboboxQuestionViewTestContext
    {
        Establish context = () =>
        {
            Func<decimal[], object, IEnumerable<AnswerViewModel>> getAnswerOptions = (questionRosterVecor, selectedAnswer) => new List<AnswerViewModel>
            {
                new AnswerViewModel(Guid.NewGuid(), "o 1", "1", false, null),
                new AnswerViewModel(Guid.NewGuid(), "o 2", "2", false, null),
                new AnswerViewModel(Guid.NewGuid(), "o 3", "3", false, null),
            };
            cascadingCombobox = CreateCascadingComboboxQuestionViewModel(getAnswerOptions);

            cascadingCombobox.HandleAnswerListChange();

            cascadingCombobox.SetAnswer(3);
        };

        Because of = () =>
            answerString = cascadingCombobox.AnswerString;

        It should_return_title_of_the_3rd_option = () =>
            answerString.ShouldEqual("o 3");

        private static CascadingComboboxQuestionViewModel cascadingCombobox;
        private static string answerString;
    }
}