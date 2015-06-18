using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.FilteredSingleOptionQuestionViewModelTests
{
    [Ignore("temp solution")]
    public class when_entering_filter_text : FilteredSingleOptionQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            questionStateMock = new Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> { DefaultValue = DefaultValue.Mock };
            answeringViewModelMock = new Mock<AnsweringViewModel>() { DefaultValue = DefaultValue.Mock };
            
            viewModel = CreateFilteredSingleOptionQuestionViewModel(
                questionStateViewModel: questionStateMock.Object,
                answering: answeringViewModelMock.Object);
            viewModel.Options = new List<FilteredSingleOptionQuestionViewModel.FilteredComboboxItemViewModel>()
            {
                new FilteredSingleOptionQuestionViewModel.FilteredComboboxItemViewModel() {Text = "abc", Value = 1},
                new FilteredSingleOptionQuestionViewModel.FilteredComboboxItemViewModel() {Text = "bac", Value = 2},
                new FilteredSingleOptionQuestionViewModel.FilteredComboboxItemViewModel() {Text = "bbc", Value = 3},
                new FilteredSingleOptionQuestionViewModel.FilteredComboboxItemViewModel() {Text = "bba", Value = 4},
                new FilteredSingleOptionQuestionViewModel.FilteredComboboxItemViewModel() {Text = "ccc", Value = 5},
            };
        };

        Because of = () =>
            viewModel.FilterText = "a";

        It should_update_suggestions_list = () =>
            viewModel.AutoCompleteSuggestions.Count.ShouldEqual(3);

        It should_suggestions_list_contains_only_items_after_filtering_text = () =>
        {
            viewModel.AutoCompleteSuggestions.ShouldContain(i => i.Value == 1);
            viewModel.AutoCompleteSuggestions.ShouldContain(i => i.Value == 2);
            viewModel.AutoCompleteSuggestions.ShouldContain(i => i.Value == 4);
        };

        static FilteredSingleOptionQuestionViewModel viewModel;

        private static Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> questionStateMock;

        private static Mock<AnsweringViewModel> answeringViewModelMock;
    }
}