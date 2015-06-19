using Machine.Specifications;

using Moq;

using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.CascadingSingleOptionQuestionViewModelTests
{
    public class when_creating_cascading_single_option_view_model : CascadingSingleOptionQuestionViewModelTestContext
    {
        Establish context = () =>
        {
        };

        Because of = () =>
            cascadingModel = CreateCascadingSingleOptionQuestionViewModel(
                questionStateViewModel: QuestionStateMock.Object,
                answering: AnsweringViewModelMock.Object);

        It should_set_QuestionState_with_non_null_value = () =>
            cascadingModel.QuestionState.ShouldNotBeNull();

        It should_set_Answering_with_non_null_value = () =>
            cascadingModel.Answering.ShouldNotBeNull();

        static CascadingSingleOptionQuestionViewModel cascadingModel;

        private static readonly Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> QuestionStateMock =
            new Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> { DefaultValue = DefaultValue.Mock };

        private static readonly Mock<AnsweringViewModel> AnsweringViewModelMock =
            new Mock<AnsweringViewModel> { DefaultValue = DefaultValue.Mock };
    }
}