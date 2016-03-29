using System;
using Machine.Specifications;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.CascadingSingleOptionQuestionViewModelTests
{
    internal class when_initializing_cascading_view_model_and_interviewId_is_missing_entityIdentity : CascadingSingleOptionQuestionViewModelTestContext
    {
        Establish context = () =>
        {
            SetUp();
            cascadingModel = CreateCascadingSingleOptionQuestionViewModel();
        };

        Because of = () =>
            exception = Catch.Exception(() => cascadingModel.InitAsync(null, null, null).WaitAndUnwrapException());

        It should_throw_ArgumentNullException_exception = () =>
            exception.ShouldBeOfExactType<ArgumentNullException>();

        It should_throw_exception_with_ParamName_equals_interviewId = () =>
            (exception as ArgumentNullException).ParamName.ShouldEqual("interviewId");

        static CascadingSingleOptionQuestionViewModel cascadingModel;
        private static Exception exception;
    }
}