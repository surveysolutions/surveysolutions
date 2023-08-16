using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MvvmCross.Plugin.Messenger;
using MvvmCross.Tests;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionQuestionViewModelTests
{
    [TestOf(typeof(CategoricalMultiOptionViewModel))]
    internal class CategoricalMultiOptionViewModelTests : BaseMvvmCrossTest
    {
        [Test]
        public void when_CheckAnswerCommand_and_question_is_not_roster_size_and_was_unchecked_then_external_action_should_be_invoked()
        {
            // arrange
            var mockOfExternalAction = new Mock<Action>();
            var viewModel = Create.ViewModel.CategoricalMultiOptionViewModel();
            viewModel.Init(Create.ViewModel.QuestionState<MultipleOptionsQuestionAnswered>(), "", 1, false, mockOfExternalAction.Object, null);
            // act
            viewModel.Checked = true;
            viewModel.CheckAnswerCommand.Execute();
            // assert
            mockOfExternalAction.Verify(x => x(), Times.Once);
        }

        [Test]
        public void when_CheckAnswerCommand_and_question_is_not_roster_size_and_was_checked_then_external_action_should_be_invoked()
        {
            // arrange
            var mockOfExternalAction = new Mock<Action>();
            var viewModel = Create.ViewModel.CategoricalMultiOptionViewModel();
            viewModel.Init(Create.ViewModel.QuestionState<MultipleOptionsQuestionAnswered>(), "", 1, false, mockOfExternalAction.Object, null);
            // act
            viewModel.Checked = true;
            viewModel.CheckAnswerCommand.Execute();
            // assert
            mockOfExternalAction.Verify(x => x(), Times.Once);
        }

        [Test]
        public void when_CheckAnswerCommand_and_question_is_roster_size_and_checked_then_external_action_should_be_invoked()
        {
            // arrange
            var mockOfExternalAction = new Mock<Action>();
            var viewModel = Create.ViewModel.CategoricalMultiOptionViewModel();
            viewModel.Init(Create.ViewModel.QuestionState<MultipleOptionsQuestionAnswered>(), "", 1, false, mockOfExternalAction.Object, null);
            viewModel.MakeRosterSize();
            viewModel.Checked = true;
            // act
            viewModel.CheckAnswerCommand.Execute();
            // assert
            mockOfExternalAction.Verify(x => x(), Times.Once);
        }

        [Test]
        public void when_CheckAnswerCommand_and_question_is_roster_size_and_unchecked_then_external_action_should_not_be_invoked_and_checked_should_be_true()
        {
            // arrange
            var mockOfExternalAction = new Mock<Action>();
            var userInteraction = Mock.Of<IUserInteractionService>(x => x.HasPendingUserInteractions == true);
            var viewModel = Create.ViewModel.CategoricalMultiOptionViewModel(userInteraction);
            viewModel.Init(Create.ViewModel.QuestionState<MultipleOptionsQuestionAnswered>(), "", 1, false, mockOfExternalAction.Object, null);
            viewModel.Checked = true;
            viewModel.MakeRosterSize();
            // act
            viewModel.Checked = false;
            viewModel.CheckAnswerCommand.Execute();
            // assert
            mockOfExternalAction.Verify(x => x(), Times.Never);
            viewModel.Checked.Should().BeTrue();
        }

        [Test]
        public void when_CheckAnswerCommand_and_question_is_roster_size_and_unchecked_and_user_say_yes_in_popup_then_external_action_should_be_invoked()
        {
            // arrange
            var mockOfExternalAction = new Mock<Action>();
            var userInteraction = Mock.Of<IUserInteractionService>(x =>
                x.ConfirmAsync(Moq.It.IsAny<string>(), "", null, null, true) == Task.FromResult(true));

            var viewModel = Create.ViewModel.CategoricalMultiOptionViewModel(userInteraction);
            viewModel.Init(Create.ViewModel.QuestionState<MultipleOptionsQuestionAnswered>(), "", 1, false, mockOfExternalAction.Object, null);
            viewModel.Checked = true;
            viewModel.MakeRosterSize();
            
            // act
            viewModel.Checked = false;
            viewModel.CheckAnswerCommand.Execute();
            // assert
            mockOfExternalAction.Verify(x => x(), Times.Once);
            viewModel.Checked.Should().BeFalse();
        }

        [Test]
        public void when_CheckAnswerCommand_and_question_is_roster_size_and_unchecked_and_user_say_no_in_popup_then_external_action_should_not_be_invoked_and_option_should_be_unchecked()
        {
            // arrange
            var mockOfExternalAction = new Mock<Action>();
            var userInteraction = Mock.Of<IUserInteractionService>(x =>
                x.ConfirmAsync(Moq.It.IsAny<string>(), "", null, null, true) == Task.FromResult(false));

            var viewModel = Create.ViewModel.CategoricalMultiOptionViewModel(userInteraction);
            viewModel.Init(Create.ViewModel.QuestionState<MultipleOptionsQuestionAnswered>(), "", 1, false, mockOfExternalAction.Object, null);
            viewModel.Checked = true;
            viewModel.MakeRosterSize();
            
            // act
            viewModel.Checked = false;
            viewModel.CheckAnswerCommand.Execute();

            // assert
            mockOfExternalAction.Verify(x => x(), Times.Never);
            viewModel.Checked.Should().BeTrue();
        }

        [Test]
        public void when_CheckAnswerCommand_and_question_is_roster_size_and_user_fast_click_on_option_twice_then_external_action_should_not_be_invoked()
        {
            // arrange
            var mockOfExternalAction = new Mock<Action>();

            var delayedTask = Task.Run(() =>
            {
                Thread.Sleep(5000);
                return false;
            });
            
            var userInteraction = Mock.Of<IUserInteractionService>(x =>
                x.ConfirmAsync(Moq.It.IsAny<string>(), "", null, null, true) == delayedTask);

            var viewModel = Create.ViewModel.CategoricalMultiOptionViewModel(userInteraction);
            viewModel.Init(Create.ViewModel.QuestionState<MultipleOptionsQuestionAnswered>(), "", 1, false, mockOfExternalAction.Object, null);
            viewModel.Checked = true;
            viewModel.MakeRosterSize();
            viewModel.Checked = false;
            viewModel.CheckAnswerCommand.Execute();
            viewModel.Checked = true;

            // act
            viewModel.CheckAnswerCommand.Execute();
            // assert
            mockOfExternalAction.Verify(x => x(), Times.Never);
        }
    }
}
