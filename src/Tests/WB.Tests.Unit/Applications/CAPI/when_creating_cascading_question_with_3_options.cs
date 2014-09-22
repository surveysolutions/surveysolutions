using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;

namespace WB.Tests.Unit.Applications.CAPI
{
    internal class when_creating_cascading_question_with_3_options : CascadingComboboxQuestionViewTestContext
    {
        Establish context = () =>
        {
            Func<decimal[], IEnumerable<AnswerViewModel>> getAnswerOptions = (questionRosterVecor) => new List<AnswerViewModel>
            {
                new AnswerViewModel(Guid.NewGuid(), "1", "1", false, null),
                new AnswerViewModel(Guid.NewGuid(), "2", "2", false, null),
                new AnswerViewModel(Guid.NewGuid(), "3", "3", false, null),
            };
            cascadingCombobox = CreateCascadingComboboxQuestionViewModel(getAnswerOptions);
        };

        Because of = () =>
            options = cascadingCombobox.AnswerOptions.ToList();

        It should_return_3_options = () => 
            options.Count.ShouldEqual(3);

        It should_return_options_with_values_1_2_3 = () =>
            options.Select(x => x.Value).ShouldContainOnly(1,2,3);

        It should_return_options_with_titles__1__2__3 = () =>
            options.Select(x => x.Title).ShouldContainOnly("1", "2", "3");

        private static CascadingComboboxQuestionViewModel cascadingCombobox;
        private static List<AnswerViewModel> options;
    }
}