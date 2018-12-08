using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.YesNoQuestionViewModelTests
{
    [TestOf(typeof(YesNoQuestionViewModel))]
    public class YesNoQuestionViewModelTests : YesNoQuestionViewModelTestsContext
    {
        public YesNoQuestionViewModelTests() => base.Setup();

        [Test]
        public void when_ToggleAnswerAsync_and_ui_dialog_showed()
        {
            // arrange
            var questionId = Create.Entity.Identity("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", Empty.RosterVector);

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.YesNoQuestion(questionId.Id));

            var questionnaireStorage = Abc.Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);
            var interview = Abc.Setup.StatefulInterview(questionnaire);
            var interviewRepository = Create.Fake.StatefulInterviewRepositoryWith(interview);

            var filteredOptionsViewModel = Abc.Setup.FilteredOptionsViewModel(new List<CategoricalOption>
            {
                Create.Entity.CategoricalQuestionOption(1, "item1")
            });

            var throttlingModel = Create.ViewModel.ThrottlingViewModel();
            var mockOfExecuteCommandAction = new Mock<Func<Task>>(); 
            throttlingModel.Init(mockOfExecuteCommandAction.Object);

            var viewModel = CreateViewModel(
                questionnaireStorage: questionnaireStorage,
                interviewRepository: interviewRepository,
                filteredOptionsViewModel: filteredOptionsViewModel,
                throttlingModel: throttlingModel,
                userInteractionService: Mock.Of<IUserInteractionService>(x => x.HasPendingUserInterations == true));
            viewModel.Init("", questionId, Create.Other.NavigationState(interviewRepository));

            // act
            viewModel.Options.Last().YesSelected = true;

            // assert
            viewModel.Options.Last().Selected.Should().BeNull();
            mockOfExecuteCommandAction.Verify(x => x(), Times.Never);
        }

        [Test]
        public void when_ToggleAnswerAsync_and_yes_answers_more_than_max_allowed_answers()
        {
            // arrange
            var questionId = Create.Entity.Identity("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", Empty.RosterVector);

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.YesNoQuestion(questionId.Id, maxAnswersCount: 2));

            var questionnaireStorage = Abc.Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);
            var interview = Abc.Setup.StatefulInterview(questionnaire);
            var interviewRepository = Create.Fake.StatefulInterviewRepositoryWith(interview);

            var filteredOptionsViewModel = Abc.Setup.FilteredOptionsViewModel(new List<CategoricalOption>
            {
                Create.Entity.CategoricalQuestionOption(1, "item1"),
                Create.Entity.CategoricalQuestionOption(2, "item2"),
                Create.Entity.CategoricalQuestionOption(3, "item3")
            });

            var throttlingModel = Create.ViewModel.ThrottlingViewModel();

            var viewModel = CreateViewModel(
                questionnaireStorage: questionnaireStorage,
                interviewRepository: interviewRepository,
                filteredOptionsViewModel: filteredOptionsViewModel,
                throttlingModel: throttlingModel);
            viewModel.Init("", questionId, Create.Other.NavigationState(interviewRepository));

            var mockOfExecuteCommandAction = new Mock<Func<Task>>();
            throttlingModel.Init(mockOfExecuteCommandAction.Object);

            viewModel.Options[0].YesSelected = true;
            viewModel.Options[1].YesSelected = true;

            // act
            viewModel.Options[2].YesSelected = true;

            // assert
            viewModel.Options[2].Selected.Should().BeNull();
            mockOfExecuteCommandAction.Verify(x => x(), Times.Exactly(2));
        }

        [Test]
        public void when_ToggleAnswerAsync_and_roster_size_question_and_yes_option_changed_to_no_and_user_say_no()
        {
            // arrange
            var questionId = Create.Entity.Identity("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", Empty.RosterVector);

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.YesNoQuestion(questionId.Id, maxAnswersCount: 2),
                Create.Entity.MultiRoster(rosterSizeQuestionId: questionId.Id));

            var questionnaireStorage = Abc.Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);
            var interview = Abc.Setup.StatefulInterview(questionnaire);
            var interviewRepository = Create.Fake.StatefulInterviewRepositoryWith(interview);

            var filteredOptionsViewModel = Abc.Setup.FilteredOptionsViewModel(new List<CategoricalOption>
            {
                Create.Entity.CategoricalQuestionOption(1, "item1"),
                Create.Entity.CategoricalQuestionOption(2, "item2")
            });

            var throttlingModel = Create.ViewModel.ThrottlingViewModel();

            var viewModel = CreateViewModel(
                questionnaireStorage: questionnaireStorage,
                interviewRepository: interviewRepository,
                filteredOptionsViewModel: filteredOptionsViewModel,
                throttlingModel: throttlingModel,
                userInteractionService: Mock.Of<IUserInteractionService>(x =>
                    x.ConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                        It.IsAny<bool>()) == Task.FromResult(false)));
            viewModel.Init("", questionId, Create.Other.NavigationState(interviewRepository));

            var mockOfExecuteCommandAction = new Mock<Func<Task>>();
            throttlingModel.Init(mockOfExecuteCommandAction.Object);

            viewModel.Options[0].YesSelected = true;

            // act
            viewModel.Options[0].NoSelected = true;

            // assert
            viewModel.Options[0].Selected.Should().BeTrue();
            mockOfExecuteCommandAction.Verify(x => x(), Times.Once);
        }

        [Test]
        public void when_ToggleAnswerAsync_and_roster_size_question_and_yes_option_changed_to_no_and_user_say_yes()
        {
            // arrange
            var questionId = Create.Entity.Identity("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", Empty.RosterVector);

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.YesNoQuestion(questionId.Id, maxAnswersCount: 2),
                Create.Entity.MultiRoster(rosterSizeQuestionId: questionId.Id));

            var questionnaireStorage = Abc.Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);
            var interview = Abc.Setup.StatefulInterview(questionnaire);
            var interviewRepository = Create.Fake.StatefulInterviewRepositoryWith(interview);

            var filteredOptionsViewModel = Abc.Setup.FilteredOptionsViewModel(new List<CategoricalOption>
            {
                Create.Entity.CategoricalQuestionOption(1, "item1"),
                Create.Entity.CategoricalQuestionOption(2, "item2")
            });

            var throttlingModel = Create.ViewModel.ThrottlingViewModel();

            var viewModel = CreateViewModel(
                questionnaireStorage: questionnaireStorage,
                interviewRepository: interviewRepository,
                filteredOptionsViewModel: filteredOptionsViewModel,
                throttlingModel: throttlingModel,
                userInteractionService: Mock.Of<IUserInteractionService>(x =>
                    x.ConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                        It.IsAny<bool>()) == Task.FromResult(true)));

            viewModel.Init("", questionId, Create.Other.NavigationState(interviewRepository));

            var mockOfExecuteCommandAction = new Mock<Func<Task>>();
            throttlingModel.Init(mockOfExecuteCommandAction.Object);

            viewModel.Options[0].YesSelected = true;

            // act
            viewModel.Options[0].NoSelected = true;

            // assert
            viewModel.Options[0].Selected.Should().BeFalse();
            mockOfExecuteCommandAction.Verify(x => x(), Times.Exactly(2));
        }

        [Test]
        public void when_ToggleAnswerAsync_and_and_yes_option_changed_to_no()
        {
            // arrange
            var questionId = Create.Entity.Identity("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", Empty.RosterVector);

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.YesNoQuestion(questionId.Id));

            var questionnaireStorage = Abc.Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);
            var interview = Abc.Setup.StatefulInterview(questionnaire);
            var interviewRepository = Create.Fake.StatefulInterviewRepositoryWith(interview);

            var filteredOptionsViewModel = Abc.Setup.FilteredOptionsViewModel(new List<CategoricalOption>
            {
                Create.Entity.CategoricalQuestionOption(1, "item1"),
                Create.Entity.CategoricalQuestionOption(2, "item2")
            });

            var throttlingModel = Create.ViewModel.ThrottlingViewModel();

            var viewModel = CreateViewModel(
                questionnaireStorage: questionnaireStorage,
                interviewRepository: interviewRepository,
                filteredOptionsViewModel: filteredOptionsViewModel,
                throttlingModel: throttlingModel);

            viewModel.Init("", questionId, Create.Other.NavigationState(interviewRepository));

            var mockOfExecuteCommandAction = new Mock<Func<Task>>();
            throttlingModel.Init(mockOfExecuteCommandAction.Object);

            viewModel.Options[0].YesSelected = true;

            // act
            viewModel.Options[0].NoSelected = true;

            // assert
            viewModel.Options[0].Selected.Should().BeFalse();
            mockOfExecuteCommandAction.Verify(x => x(), Times.Exactly(2));
        }

        [Test]
        public void should_not_set_answers_order()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.YesNoQuestion(Id.g1, answers: new int[] { 1, 2 }, ordered: false, maxAnswersCount: 2)
            );
            var questionnaireStorage = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);

            var filteredOptionsViewModel = Abc.Setup.FilteredOptionsViewModel(new List<CategoricalOption>
            {
                Create.Entity.CategoricalQuestionOption(1, "item1"),
                Create.Entity.CategoricalQuestionOption(2, "item2"),
            });

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            var interviewRepository = Create.Fake.StatefulInterviewRepositoryWith(interview);

            var viewModel = CreateViewModel(questionnaireStorage: questionnaireStorage,
                interviewRepository: interviewRepository,
                filteredOptionsViewModel: filteredOptionsViewModel);

            viewModel.Init(interview.Id.FormatGuid(), Create.Identity(Id.g1), Create.Other.NavigationState(interviewRepository));

            // Act
            viewModel.Handle(Create.Event.YesNoQuestionAnswered(Id.g1, new[]
            {
                Create.Entity.AnsweredYesNoOption(1, true)
            }));

            // Assert
            var firstOption = viewModel.Options.First();
            Assert.That(firstOption, Has.Property(nameof(firstOption.YesAnswerCheckedOrder)).Null);
        }
    }
}
