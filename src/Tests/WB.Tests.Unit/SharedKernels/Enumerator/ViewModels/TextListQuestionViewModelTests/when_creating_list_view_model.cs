using FluentAssertions;
using Moq;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.TextListQuestionViewModelTests
{
    internal class when_creating_list_view_model : TextListQuestionViewModelTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            BecauseOf();
        }

        public void BecauseOf() =>
            listModel = CreateTextListQuestionViewModel(
            questionStateViewModel: QuestionStateMock.Object,
            answering: AnsweringViewModelMock.Object);

        [NUnit.Framework.Test] public void should_set_QuestionState_with_non_null_value () =>
            listModel.QuestionState.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_set_Answering_with_non_null_value () =>
            listModel.Answering.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_set_Answers_with_non_null_value () =>
            listModel.Answers.Should().NotBeNull();

        static TextListQuestionViewModel listModel;

        private static readonly Mock<QuestionStateViewModel<TextListQuestionAnswered>> QuestionStateMock =
           new Mock<QuestionStateViewModel<TextListQuestionAnswered>> { DefaultValue = DefaultValue.Mock };

        private static readonly Mock<AnsweringViewModel> AnsweringViewModelMock =
            new Mock<AnsweringViewModel>() { DefaultValue = DefaultValue.Mock };
    }
}
