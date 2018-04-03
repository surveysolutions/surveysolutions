using FluentAssertions;
using Moq;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.FilteredSingleOptionQuestionViewModelTests
{
    internal class when_creating_filtered_single_option_view_model : FilteredSingleOptionQuestionViewModelTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionStateMock = new Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> { DefaultValue = DefaultValue.Mock };
            answeringViewModelMock = new Mock<AnsweringViewModel>() { DefaultValue = DefaultValue.Mock };
            BecauseOf();
        }

        public void BecauseOf() =>
            viewModel = CreateFilteredSingleOptionQuestionViewModel(
            questionStateViewModel: questionStateMock.Object,
            answering: answeringViewModelMock.Object);

        [NUnit.Framework.Test] public void should_set_QuestionState_with_non_null_value () =>
            viewModel.QuestionState.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_set_Answering_with_non_null_value () =>
            viewModel.Answering.Should().NotBeNull();


        static FilteredSingleOptionQuestionViewModel viewModel;

        private static Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> questionStateMock;

        private static Mock<AnsweringViewModel> answeringViewModelMock;
    }
}
