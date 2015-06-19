using System;

using Machine.Specifications;

using Moq;

using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.CascadingSingleOptionQuestionViewModelTests
{
    public class when_initializing_cascading_view_model_and_interviewId_is_missing : CascadingSingleOptionQuestionViewModelTestContext
    {
        Establish context = () =>
        {
            cascadingModel = CreateCascadingSingleOptionQuestionViewModel(QuestionStateMock.Object, AnsweringViewModelMock.Object);
        };

        Because of = () =>
            exception = Catch.Exception(() => cascadingModel.Init("Some interviewId", null, null));

        It should_throw_ArgumentNullException_exception = () =>
            exception.ShouldBeOfExactType<ArgumentNullException>();

        It should_throw_exception_with_ParamName_equals_interviewId = () =>
            (exception as ArgumentNullException).ParamName.ShouldEqual("entityIdentity");

        static CascadingSingleOptionQuestionViewModel cascadingModel;
        private static Exception exception;

        private static readonly Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> QuestionStateMock =
            new Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> { DefaultValue = DefaultValue.Mock };

        private static readonly Mock<AnsweringViewModel> AnsweringViewModelMock =
            new Mock<AnsweringViewModel> { DefaultValue = DefaultValue.Mock };
    }
}