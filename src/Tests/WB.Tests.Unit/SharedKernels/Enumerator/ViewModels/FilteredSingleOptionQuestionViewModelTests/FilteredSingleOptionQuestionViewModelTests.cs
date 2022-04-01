using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MvvmCross.Plugin.Messenger;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.FilteredSingleOptionQuestionViewModelTests
{
    public class FilteredSingleOptionQuestionViewModelTests : FilteredSingleOptionQuestionViewModelTestsContext
    {
        [Test]
        public async Task when_entering_filter_text()
        {
            FilteredSingleOptionQuestionViewModel viewModel;
            Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> questionStateMock;
            string answerValue = "a";
            var interviewId = "interviewId";

            var singleOptionAnswer =
                Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.GetAnswer() == Create.Entity.SingleOptionAnswer(3));
            questionStateMock = new Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>>
                {DefaultValue = DefaultValue.Mock};
            var answerViewModel = new AnsweringViewModel(Mock.Of<ICommandService>(),
                Mock.Of<IUserInterfaceStateService>(),  Mock.Of<ILogger>());

            var interview = Mock.Of<IStatefulInterview>(_
                => _.QuestionnaireIdentity == questionnaireId
                   && _.GetSingleOptionQuestion(questionIdentity) == singleOptionAnswer &&
                   _.GetTopFilteredOptionsForQuestion(questionIdentity, null, answerValue, 200, It.IsAny<int[]>()) ==
                   new List<CategoricalOption>()
                   {
                       new CategoricalOption() {Title = "abc", Value = 1},
                       new CategoricalOption() {Title = "bac", Value = 2},
                       new CategoricalOption() {Title = "bba", Value = 4}
                   });

            var interviewRepository = Create.Storage.InterviewRepository(interview);

            var filteredOptionsViewModel = Abc.SetUp.FilteredOptionsViewModel(new List<CategoricalOption>()
            {
                new CategoricalOption() {Title = "abc", Value = 1},
                new CategoricalOption() {Title = "bac", Value = 2},
                new CategoricalOption() {Title = "bbc", Value = 3},
                new CategoricalOption() {Title = "bba", Value = 4},
                new CategoricalOption() {Title = "ccc", Value = 5},
            });

            viewModel = CreateFilteredSingleOptionQuestionViewModel(
                questionStateViewModel: questionStateMock.Object,
                answering: answerViewModel,
                interviewRepository: interviewRepository,
                filteredOptionsViewModel: filteredOptionsViewModel);

            var navigationState = Create.Other.NavigationState();

            viewModel.Init(interviewId, questionIdentity, navigationState);

            var autocomplete = viewModel.Children[1] as CategoricalComboboxAutocompleteViewModel;

            //act
            await autocomplete.FilterCommand.ExecuteAsync(answerValue);

            //assert
            autocomplete.AutoCompleteSuggestions.Count.Should().Be(3);
            autocomplete.AutoCompleteSuggestions.Should().Contain(i => i.Title == "abc");
            autocomplete.AutoCompleteSuggestions.Should().Contain(i => i.Title == "bac");
            autocomplete.AutoCompleteSuggestions.Should().Contain(i => i.Title == "bba");
        }

        [Test]
        public async Task when_setting_text_existing_option()
        {
            string interviewId = "interviewId";
            Guid userId = Guid.NewGuid();
            string answerValue = "e";

            var singleOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.GetAnswer() == Create.Entity.SingleOptionAnswer(3));
            var option = new CategoricalOption() { Value = 1, Title = $"dfdf{answerValue}"};

            var interview = Mock.Of<IStatefulInterview>(_
                => _.QuestionnaireIdentity == questionnaireId
                   && _.GetSingleOptionQuestion(questionIdentity) == singleOptionAnswer &&
                   _.GetTopFilteredOptionsForQuestion(questionIdentity, null, answerValue, 200, It.IsAny<int[]>()) == new List<CategoricalOption>() { option });

            var interviewRepository = Create.Storage.InterviewRepository(interview);

            var userIdentity = Mock.Of<IUserIdentity>(_ => _.UserId == userId);
            var principal = Mock.Of<IPrincipal>(_ => _.CurrentUserIdentity == userIdentity);

            var questionStateMock = new Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> { DefaultValue = DefaultValue.Mock };
            var answerViewModel = Create.ViewModel.AnsweringViewModel();

            var filteredOptionsViewModel = Abc.SetUp.FilteredOptionsViewModel();

            var viewModel = CreateFilteredSingleOptionQuestionViewModel(
                questionStateViewModel: questionStateMock.Object,
                answering: answerViewModel,
                principal: principal,
                interviewRepository: interviewRepository,
                filteredOptionsViewModel: filteredOptionsViewModel);

            var navigationState = Create.Other.NavigationState();
            viewModel.Init(interviewId, questionIdentity, navigationState);

            CategoricalComboboxAutocompleteViewModel autocomplete = (CategoricalComboboxAutocompleteViewModel) viewModel.Children[1];

            //act
            await autocomplete.FilterCommand.ExecuteAsync(answerValue);
            
            //assert
            autocomplete.FilterText.Should().Be(answerValue);
            autocomplete.AutoCompleteSuggestions.Count.Should().Be(1);
        }

        [Test]
        public async Task when_selecting_one_from_option_list()
        {

            FilteredSingleOptionQuestionViewModel viewModel;
            Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> questionStateMock;
            Mock<AnsweringViewModel> answeringViewModelMock;
            string interviewId;
            Guid userId;
            interviewId = Id.g1.FormatGuid();
            userId = Guid.NewGuid();

            var singleOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.GetAnswer() == Create.Entity.SingleOptionAnswer(3));

            var interview = Mock.Of<IStatefulInterview>(_
                => _.QuestionnaireIdentity == questionnaireId
                   && _.GetSingleOptionQuestion(questionIdentity) == singleOptionAnswer
                   && _.GetOptionForQuestionWithFilter(questionIdentity, "html", null) == Create.Entity.CategoricalQuestionOption(4, "html", null));

            var interviewRepository = Create.Storage.InterviewRepository(interview);

            var userIdentity = Mock.Of<IUserIdentity>(_ => _.UserId == userId);
            var principal = Mock.Of<IPrincipal>(_ => _.CurrentUserIdentity == userIdentity);

            questionStateMock = new Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> { DefaultValue = DefaultValue.Mock };
            answeringViewModelMock = new Mock<AnsweringViewModel>() { DefaultValue = DefaultValue.Mock };

            var filteredOptionsViewModel = Abc.SetUp.FilteredOptionsViewModel();

            viewModel = CreateFilteredSingleOptionQuestionViewModel(
                questionStateViewModel: questionStateMock.Object,
                answering: answeringViewModelMock.Object,
                principal: principal,
                interviewRepository: interviewRepository,
                filteredOptionsViewModel: filteredOptionsViewModel);

            var navigationState = Create.Other.NavigationState();

            viewModel.Init(interviewId, questionIdentity, navigationState);
            await viewModel.SaveAnswerAsync(4);

            answeringViewModelMock.Verify(x => x.SendQuestionCommandAsync(Moq.It.Is<AnswerSingleOptionQuestionCommand>(y => y.SelectedValue == 4)), Times.Once);
        }

        [Test]
        public void when_initing_view_model()
        {
            FilteredSingleOptionQuestionViewModel viewModel;
            NavigationState navigationState;
            Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> questionStateMock;
            Mock<AnsweringViewModel> answeringViewModelMock;
            string interviewId;
            Guid userId;

            interviewId = "interviewId";
            userId = Guid.NewGuid();

            var singleOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.GetAnswer() == Create.Entity.SingleOptionAnswer(3) && _.IsAnswered() == true);

            var interview = Mock.Of<IStatefulInterview>(_
                => _.QuestionnaireIdentity == questionnaireId
                   && _.GetSingleOptionQuestion(questionIdentity) == singleOptionAnswer
                   && _.GetOptionForQuestionWithoutFilter(questionIdentity, 3, null) == new CategoricalOption() { Title = "3", Value = 3 });

            var interviewRepository =Create.Storage.InterviewRepository(interview);

            var userIdentity = Mock.Of<IUserIdentity>(_ => _.UserId == userId);
            var principal = Mock.Of<IPrincipal>(_ => _.CurrentUserIdentity == userIdentity);

            questionStateMock = new Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> { DefaultValue = DefaultValue.Mock };
            answeringViewModelMock = new Mock<AnsweringViewModel>() { DefaultValue = DefaultValue.Mock };

            var filteredOptionsViewModel = Abc.SetUp.FilteredOptionsViewModel(new List<CategoricalOption>() { new CategoricalOption() { Title = "3", Value = 3 } });

            viewModel = CreateFilteredSingleOptionQuestionViewModel(
                questionStateViewModel: questionStateMock.Object,
                answering: answeringViewModelMock.Object,
                principal: principal,
                interviewRepository: interviewRepository,
                filteredOptionsViewModel: filteredOptionsViewModel);

            navigationState = Create.Other.NavigationState();

            viewModel.Init(interviewId, questionIdentity, navigationState);

            var autocomplete = viewModel.Children[1] as CategoricalComboboxAutocompleteViewModel;

            autocomplete.FilterText.Should().Be("3");
        }

        [Test]
        public async Task when_execute_FilterCommand_and_single_question_answered_and_filter_is_empty_and_focus_out_should_validity_have_not_saved_state_with_message()
        {
            // arrange
            var questionId = Identity.Create(Guid.Parse("11111111111111111111111111111111"), RosterVector.Empty);
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
            {
                Create.Entity.SingleOptionQuestion(questionId.Id, showAsListThreshold: 50,
                    answerCodes: new[] {1m, 2m, 3m})
            });
            var interview = SetUp.StatefulInterview(questionnaire);
            interview.AnswerSingleOptionQuestion(Guid.Empty, questionId.Id, questionId.RosterVector, DateTimeOffset.UtcNow, 1);

            var vm = Create.ViewModel.FilteredSingleOptionQuestionViewModel(questionId, questionnaire, interview);
            vm.Init(interview.Id.ToString("N"), questionId, Create.Other.NavigationState());

            var combobox = vm.Children.OfType<CategoricalComboboxAutocompleteViewModel>().First();
            await combobox.FilterCommand.ExecuteAsync(string.Empty);
            // act
            await combobox.OnFocusChangeCommand.ExecuteAsync(false);
            // assert
            Assert.That(vm.QuestionState.Validity.Error.ValidationErrors, Has.One.Items);
        }
    }
}
