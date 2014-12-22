using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;

namespace WB.Tests.Unit.BoundedContexts.Capi.CascadingComboboxTests
{
    internal class when_creating_cascading_question_and_setting_answer : CascadingComboboxQuestionViewTestContext
    {
        Establish context = () =>
        {
            Func<decimal[], object,  IEnumerable<AnswerViewModel>> getAnswerOptions = (questionRosterVecor, selectedAnswer) => new List<AnswerViewModel>
            {
                new AnswerViewModel(Guid.NewGuid(), "o 1", "1", false, null),
                new AnswerViewModel(Guid.NewGuid(), "o 2", "2", false, null),
                new AnswerViewModel(Guid.NewGuid(), "o 3", "3", false, null),
            };
            cascadingCombobox = CreateCascadingComboboxQuestionViewModel(getAnswerOptions);

            cascadingCombobox.HandleAnswerListChange();
            cascadingCombobox.PropertyChanged += cascadingCombobox_PropertyChanged;
        };

        Because of = () =>
            cascadingCombobox.SetAnswer(3);

        It should_set_selected_the_3rd_option = () =>
            cascadingCombobox.filteredAnswers.Single(x => x.Selected).Value.ShouldEqual(3);

        It should_set_answered_status = () =>
            cascadingCombobox.Status.ShouldEqual(QuestionStatus.Answered | QuestionStatus.Enabled | QuestionStatus.ParentEnabled | QuestionStatus.Valid);

        It should_raise_status_changed = () =>
           statusChangedRaised.ShouldBeTrue();

        private static CascadingComboboxQuestionViewModel cascadingCombobox;

        private static bool statusChangedRaised = false;

        private static void cascadingCombobox_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Status")
                statusChangedRaised = true;
        }
    }
}