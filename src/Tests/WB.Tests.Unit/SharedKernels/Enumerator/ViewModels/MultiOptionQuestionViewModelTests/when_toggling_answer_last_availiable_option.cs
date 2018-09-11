using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionQuestionViewModelTests
{
    internal class when_toggling_answer_last_availiable_option: MultiOptionQuestionViewModelTestsContext
    {
        [OneTimeSetUp] 
        public async Task context () {
            var options = new[]
            {
                Create.Entity.Option(1, "item1"),
                Create.Entity.Option(2, "item2"),
                Create.Entity.Option(3, "item3"),
                Create.Entity.Option(4, "item4"),
            };

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultyOptionsQuestion(Id.g1, options, maxAllowedAnswers: 2),
                Create.Entity.MultiRoster(Id.g2, rosterSizeQuestionId: Id.g1)
            );
            var questionnaireStorage = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);

            var interview = Setup.StatefulInterview(questionnaire);
            var interviewRepository = Setup.StatefulInterviewRepository(interview);

            viewModel = CreateViewModel(
                questionnaireStorage: questionnaireStorage,
                interviewRepository: interviewRepository,
                filteredOptionsViewModel: Setup.FilteredOptionsViewModel(options));

            interview.AnswerMultipleOptionsQuestion(Id.gF, Id.g1, RosterVector.Empty, DateTimeOffset.UtcNow, new [] { 1 });
            
            viewModel.Init("blah", Create.Entity.Identity(Id.g1, Empty.RosterVector), Create.Other.NavigationState());

            var option = viewModel.Options.Second();
            option.Checked = true;
            await viewModel.ToggleAnswerAsync(option);
        }

        [Test] 
        public void should_mark_all_unselected_options_as_not_available ()
        {
            viewModel.Options.ElementAt(2).Checked.Should().BeFalse();
            viewModel.Options.ElementAt(2).CanBeChecked.Should().BeFalse();

            viewModel.Options.ElementAt(3).Checked.Should().BeFalse();
            viewModel.Options.ElementAt(3).CanBeChecked.Should().BeFalse();
        }

        static MultiOptionQuestionViewModel viewModel;
    }
}
