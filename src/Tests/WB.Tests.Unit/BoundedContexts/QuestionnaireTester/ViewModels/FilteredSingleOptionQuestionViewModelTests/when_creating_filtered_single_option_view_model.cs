using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.FilteredSingleOptionQuestionViewModelTests
{
    public class when_creating_filtered_single_option_view_model : FilteredSingleOptionQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            questionStateMock = new Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> { DefaultValue = DefaultValue.Mock };
            answeringViewModelMock = new Mock<AnsweringViewModel>() { DefaultValue = DefaultValue.Mock };
        };

        Because of = () =>
            viewModel = CreateFilteredSingleOptionQuestionViewModel(
            questionStateViewModel: questionStateMock.Object,
            answering: answeringViewModelMock.Object);

        It should_set_QuestionState_with_non_null_value = () =>
            viewModel.QuestionState.ShouldNotBeNull();

        It should_set_Answering_with_non_null_value = () =>
            viewModel.Answering.ShouldNotBeNull();


        static FilteredSingleOptionQuestionViewModel viewModel;

        private static Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> questionStateMock;

        private static Mock<AnsweringViewModel> answeringViewModelMock;
    }
}