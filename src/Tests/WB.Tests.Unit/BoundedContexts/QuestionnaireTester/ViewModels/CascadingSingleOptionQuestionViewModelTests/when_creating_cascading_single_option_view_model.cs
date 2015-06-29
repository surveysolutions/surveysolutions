using Machine.Specifications;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.CascadingSingleOptionQuestionViewModelTests
{
    public class when_creating_cascading_single_option_view_model : CascadingSingleOptionQuestionViewModelTestContext
    {
        Establish context = () =>
        {
            SetUp();
        };

        Because of = () =>
            cascadingModel = CreateCascadingSingleOptionQuestionViewModel();

        It should_set_QuestionState_with_non_null_value = () =>
            cascadingModel.QuestionState.ShouldNotBeNull();

        It should_set_Answering_with_non_null_value = () =>
            cascadingModel.Answering.ShouldNotBeNull();

        static CascadingSingleOptionQuestionViewModel cascadingModel;
    }
}