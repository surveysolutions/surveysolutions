using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.CascadingSingleOptionQuestionViewModelTests
{
    public class when_initializing_cascading_view_model_and_interviewId_is_missing_entityIdentity : CascadingSingleOptionQuestionViewModelTestContext
    {
        Establish context = () =>
        {
            SetUp();
            cascadingModel = CreateCascadingSingleOptionQuestionViewModel();
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                cascadingModel.Init(null, null, null));

        It should_throw_ArgumentNullException_exception = () =>
            exception.ShouldBeOfExactType<ArgumentNullException>();

        It should_throw_exception_with_ParamName_equals_interviewId = () =>
            (exception as ArgumentNullException).ParamName.ShouldEqual("interviewId");

        static CascadingSingleOptionQuestionViewModel cascadingModel;
        private static Exception exception;
    }
}