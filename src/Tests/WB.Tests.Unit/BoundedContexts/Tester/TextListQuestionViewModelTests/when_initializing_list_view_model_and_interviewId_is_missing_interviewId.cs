using System;

using Machine.Specifications;

using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;

namespace WB.Tests.Unit.BoundedContexts.Tester.TextListQuestionViewModelTests
{
    public class when_initializing_list_view_model_and_interviewId_is_missing_interviewId : TextListQuestionViewModelTestContext
    {
        Establish context = () =>
        {
            listModel = CreateTextListQuestionViewModel();
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                listModel.Init(null, null, null));

        It should_throw_ArgumentNullException_exception = () =>
            exception.ShouldBeOfExactType<ArgumentNullException>();

        It should_throw_exception_with_ParamName_equals_interviewId = () =>
            (exception as ArgumentNullException).ParamName.ShouldEqual("interviewId");

        static TextListQuestionViewModel listModel;
        private static Exception exception;
    }
}