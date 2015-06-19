using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Tester.TextListQuestionViewModelTests
{
    public class when_creating_list_view_model : TextListQuestionViewModelTestContext
    {
        Establish context = () =>
        {
            
        };

        Because of = () =>
            listModel = CreateTextListQuestionViewModel(
            questionStateViewModel: QuestionStateMock.Object,
            answering: AnsweringViewModelMock.Object);

        It should_set_QuestionState_with_non_null_value = () =>
            listModel.QuestionState.ShouldNotBeNull();

        It should_set_Answering_with_non_null_value = () =>
            listModel.Answering.ShouldNotBeNull();

        It should_set_Answers_with_non_null_value = () =>
            listModel.Answers.ShouldNotBeNull();

        static TextListQuestionViewModel listModel;

        private static readonly Mock<QuestionStateViewModel<TextListQuestionAnswered>> QuestionStateMock =
           new Mock<QuestionStateViewModel<TextListQuestionAnswered>> { DefaultValue = DefaultValue.Mock };

        private static readonly Mock<AnsweringViewModel> AnsweringViewModelMock =
            new Mock<AnsweringViewModel>() { DefaultValue = DefaultValue.Mock };
    }
}