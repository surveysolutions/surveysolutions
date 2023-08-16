using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.YesNoQuestionViewModelTests
{
    [TestOf(typeof(CategoricalYesNoOptionViewModel))]
    public class YesNoQuestionOptionViewModelTests : BaseMvvmCrossTest
    {
        [Test]
        public void when_roster_size_question_and_set_yes_and_ui_has_pending_user_interactions_then_answer_should_not_be_set()
        {
            // arrange
            var userInteractionService = Mock.Of<IUserInteractionService>(x => x.HasPendingUserInteractions == true);
            var mockOfAction = new Mock<Action>();
            var viewModel = Create.ViewModel.YesNoQuestionOptionViewModel(userInteractionService);

            viewModel.Init(Create.ViewModel.QuestionState<YesNoQuestionAnswered>(), "title", 1, false, mockOfAction.Object, null);
            viewModel.MakeRosterSize();

            viewModel.Checked = true;

            // act
            viewModel.NoSelected = true;
            viewModel.SetNoAnswerCommand.Execute();

            // assert
            viewModel.NoSelected.Should().BeFalse();
            mockOfAction.Verify(x => x(), Times.Never);
        }

        [Test]
        public void when_roster_size_question_and_yes_option_changed_to_no_and_user_say_no_then_answer_should_not_be_set()
        {
            // arrange
            var userInteractionService = Mock.Of<IUserInteractionService>(x =>
                x.ConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<bool>()) == Task.FromResult(false));

            var mockOfAction = new Mock<Action>();
            var viewModel = Create.ViewModel.YesNoQuestionOptionViewModel(userInteractionService);

            viewModel.Init(Create.ViewModel.QuestionState<YesNoQuestionAnswered>(), "title", 1, false, mockOfAction.Object, null);
            viewModel.MakeRosterSize();

            viewModel.Checked = true;

            // act
            viewModel.NoSelected = true;
            viewModel.SetNoAnswerCommand.Execute();

            // assert
            viewModel.NoSelected.Should().BeFalse();
            mockOfAction.Verify(x => x(), Times.Never);
        }

        [Test]
        public void when_roster_size_question_and_yes_option_changed_to_no_and_user_say_yes_then_answer_should_set_to_yes_and_command_action_should_be_executed()
        {
            // arrange
            var userInteractionService = Mock.Of<IUserInteractionService>(x =>
                x.ConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<bool>()) == Task.FromResult(true));

            var mockOfAction = new Mock<Action>();
            var viewModel = Create.ViewModel.YesNoQuestionOptionViewModel(userInteractionService);

            viewModel.Init(Create.ViewModel.QuestionState<YesNoQuestionAnswered>(), "title", 1, false, mockOfAction.Object, null);
            viewModel.MakeRosterSize();

            viewModel.Checked = true;

            // act
            viewModel.NoSelected = true;
            viewModel.SetNoAnswerCommand.Execute();

            // assert
            viewModel.Checked.Should().BeFalse();
            mockOfAction.Verify(x => x(), Times.Once);
        }

        [Test]
        public void when_roster_size_question_and_remove_option_and_user_say_no_for_removing_roster_instance_then_answer_should_not_be_removed()
        {
            // arrange
            var userInteractionService = Mock.Of<IUserInteractionService>(x =>
                x.ConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<bool>()) == Task.FromResult(false));

            var mockOfAction = new Mock<Action>();
            var viewModel = Create.ViewModel.YesNoQuestionOptionViewModel(userInteractionService);

            viewModel.Init(Create.ViewModel.QuestionState<YesNoQuestionAnswered>(), "title", 1, false, mockOfAction.Object, null);
            viewModel.MakeRosterSize();

            viewModel.Checked = true;

            // act
            viewModel.RemoveAnswerCommand.Execute();

            // assert
            viewModel.Checked.Should().BeTrue();
            mockOfAction.Verify(x => x(), Times.Never);
        }

        [Test]
        public void when_roster_size_question_and_remove_option_and_user_say_yes_for_removing_roster_instance_then_answer_should_be_removed()
        {
            // arrange
            var userInteractionService = Mock.Of<IUserInteractionService>(x =>
                x.ConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<bool>()) == Task.FromResult(true));

            var mockOfAction = new Mock<Action>();
            var viewModel = Create.ViewModel.YesNoQuestionOptionViewModel(userInteractionService);

            viewModel.Init(Create.ViewModel.QuestionState<YesNoQuestionAnswered>(), "title", 1, false, mockOfAction.Object, null);
            viewModel.MakeRosterSize();

            viewModel.Checked = true;

            // act
            viewModel.RemoveAnswerCommand.Execute();

            // assert
            viewModel.Checked.Should().BeFalse();
            viewModel.NoSelected.Should().BeFalse();
            mockOfAction.Verify(x => x(), Times.Once);
        }
    }
}
