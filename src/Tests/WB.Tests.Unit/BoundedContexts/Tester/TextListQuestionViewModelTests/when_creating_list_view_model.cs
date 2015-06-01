using Machine.Specifications;

using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;

namespace WB.Tests.Unit.BoundedContexts.Tester.TextListQuestionViewModelTests
{
    public class when_creating_list_view_model : TextListQuestionViewModelTestContext
    {
        Because of = () => 
            listModel = CreateTextListQuestionViewModel();

        It should_set_QuestionState_with_non_null_value = () =>
            listModel.QuestionState.ShouldNotBeNull();

        It should_set_Answering_with_non_null_value = () =>
            listModel.Answering.ShouldNotBeNull();

        It should_set_Answers_with_non_null_value = () =>
            listModel.Answers.ShouldNotBeNull();

        static TextListQuestionViewModel listModel;
    }
}