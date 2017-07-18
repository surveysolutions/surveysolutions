using System.Collections.Generic;
using System.Threading;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.FilteredSingleOptionQuestionViewModelTests
{
    internal class when_entering_filter_text : FilteredSingleOptionQuestionViewModelTestsContext
    {
        private Establish context = () =>
        {
            var interviewId = "interviewId";
            var singleOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.GetAnswer() == Create.Entity.SingleOptionAnswer(3));
            questionStateMock = new Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> { DefaultValue = DefaultValue.Mock };
            var answerViewModel = new AnsweringViewModel(Mock.Of<ICommandService>(), Mock.Of<IUserInterfaceStateService>());
            
            var interview = Mock.Of<IStatefulInterview>(_
               => _.QuestionnaireIdentity == questionnaireId
                  && _.GetSingleOptionQuestion(questionIdentity) == singleOptionAnswer &&
                  _.GetTopFilteredOptionsForQuestion(questionIdentity, null, answerValue, 200) == new List<CategoricalOption>() {
                    new CategoricalOption() {Title = "abc", Value = 1},
                    new CategoricalOption() {Title = "bac", Value = 2},
                    new CategoricalOption() {Title = "bba", Value = 4}});

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(_ => _.Get(interviewId) == interview);

            var filteredOptionsViewModel = Abc.Setup.FilteredOptionsViewModel(new List<CategoricalOption>()
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
            
        };

        Because of = () => viewModel.FilterCommand.ExecuteAsync(answerValue).Await();

        It should_update_suggestions_list = () =>
            viewModel.AutoCompleteSuggestions.Count.ShouldEqual(3);

        It should_suggestions_list_contains_only_items_after_filtering_text = () =>
        {
            viewModel.AutoCompleteSuggestions.ShouldContain(i => i == "<b>a</b>bc");
            viewModel.AutoCompleteSuggestions.ShouldContain(i => i == "b<b>a</b>c");
            viewModel.AutoCompleteSuggestions.ShouldContain(i => i == "bb<b>a</b>");
        };

        static FilteredSingleOptionQuestionViewModel viewModel;

        private static Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> questionStateMock;
        
        private static string answerValue = "a";
    }
}