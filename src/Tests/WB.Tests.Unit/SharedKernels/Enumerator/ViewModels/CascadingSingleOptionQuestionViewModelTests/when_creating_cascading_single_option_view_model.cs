using FluentAssertions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.CascadingSingleOptionQuestionViewModelTests
{
    internal class when_creating_cascading_single_option_view_model : CascadingSingleOptionQuestionViewModelTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            SetUp();
            BecauseOf();
        }

        public void BecauseOf() =>
            cascadingModel = CreateCascadingSingleOptionQuestionViewModel();

        [NUnit.Framework.Test] public void should_set_QuestionState_with_non_null_value () =>
            cascadingModel.QuestionState.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_set_Answering_with_non_null_value () =>
            cascadingModel.Answering.Should().NotBeNull();

        static CascadingSingleOptionQuestionViewModel cascadingModel;
    }
}
