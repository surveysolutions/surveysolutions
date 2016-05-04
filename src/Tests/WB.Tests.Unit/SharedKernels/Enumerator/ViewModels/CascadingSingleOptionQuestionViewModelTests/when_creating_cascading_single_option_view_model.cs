using Machine.Specifications;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.CascadingSingleOptionQuestionViewModelTests
{
    internal class when_creating_cascading_single_option_view_model : CascadingSingleOptionQuestionViewModelTestContext
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