using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.MultiOptionLinkedQuestionOptionViewModelTests
{
    public class when_setting_checked_property
    {
        Establish context = () =>
        {
            viewModel = new MultiOptionLinkedQuestionOptionViewModel(Mock.Of<MultiOptionLinkedQuestionViewModel>());
        };

        Because of = () => viewModel.Checked = true;

        It should_set_checked_timestamp = () => viewModel.CheckedTimeStamp.ShouldNotEqual(DateTime.MinValue);
        
        static MultiOptionLinkedQuestionOptionViewModel viewModel;
    }
}

