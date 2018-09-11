using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionQuestionViewModelTests
{
    internal class when_toggling_answer_and_max_answers_count_reached : MultiOptionQuestionViewModelTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public async Task context () {
            questionGuid = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionId = Create.Entity.Identity(questionGuid, Empty.RosterVector);

            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.ShouldQuestionRecordAnswersOrder(questionId.Id) == true
                && _.GetMaxSelectedAnswerOptions(questionId.Id) == 1
                && _.IsRosterSizeQuestion(questionId.Id) == false
            );

            var filteredOptionsViewModel = Setup.FilteredOptionsViewModel(new List<CategoricalOption>
            {
                Create.Entity.CategoricalQuestionOption(1, "item1"),
                Create.Entity.CategoricalQuestionOption(2, "item2"),
            });

            var multiOptionAnswer = Create.Entity.InterviewTreeMultiOptionQuestion(new[] { 1m });

            var interview = Mock.Of<IStatefulInterview>(x => x.GetMultiOptionQuestion(questionId) == multiOptionAnswer);

            var questionnaireStorage = new Mock<IQuestionnaireStorage>();
            var interviewRepository = new Mock<IStatefulInterviewRepository>();

            questionnaireStorage.SetReturnsDefault(questionnaire);
            interviewRepository.SetReturnsDefault(interview);

            viewModel = CreateViewModel(questionnaireStorage: questionnaireStorage.Object,
                interviewRepository: interviewRepository.Object,
                filteredOptionsViewModel: filteredOptionsViewModel);

            viewModel.Init("blah", questionId, Create.Other.NavigationState());
            viewModel.Options.Second().Checked = true;

            await BecauseOf();
        }

        public async Task BecauseOf() => await viewModel.ToggleAnswerAsync(viewModel.Options.Second());

        [NUnit.Framework.Test] public void should_undo_checked_property () => viewModel.Options.Second().Checked.Should().BeFalse();

        private static MultiOptionQuestionViewModel viewModel;
        private static Identity questionId;
        private static Guid questionGuid;
    }
}
