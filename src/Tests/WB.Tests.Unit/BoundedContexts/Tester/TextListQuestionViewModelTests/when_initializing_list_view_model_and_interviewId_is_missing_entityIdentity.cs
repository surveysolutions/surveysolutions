using System;

using Machine.Specifications;

using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;

namespace WB.Tests.Unit.BoundedContexts.Tester.TextListQuestionViewModelTests
{
    public class when_initializing_list_view_model_and_interviewId_is_missing_entityIdentity : TextListQuestionViewModelTestContext
    {
        Establish context = () =>
        {
            listModel = CreateTextListQuestionViewModel();
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                listModel.Init("Some interviewId", null, null));

        It should_throw_ArgumentNullException_exception = () =>
            exception.ShouldBeOfExactType<ArgumentNullException>();

        It should_throw_exception_with_ParamName_equals_interviewId = () =>
            (exception as ArgumentNullException).ParamName.ShouldEqual("entityIdentity");

        static TextListQuestionViewModel listModel;
        private static Exception exception;
    }
}