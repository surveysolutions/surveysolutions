using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionQuestionViewModelTests
{
    internal class when_toggling_answer_and_user_agrees_to_reduce_roster_size: MultiOptionQuestionViewModelTestsContext
    {
        [OneTimeSetUp] 
        public async Task context () {
            var options = new[]
            {
                Create.Entity.Option(1, "item1"),
                Create.Entity.Option(2, "item2")
            };

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultyOptionsQuestion(Id.g1, options),
                Create.Entity.MultiRoster(Id.g2, rosterSizeQuestionId: Id.g1)
            );
            var questionnaireStorage = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);
            
            var filteredOptionsViewModel = Setup.FilteredOptionsViewModel(options);

            var interview = Setup.StatefulInterview(questionnaire);
            var interviewRepository = Setup.StatefulInterviewRepository(interview);

            userInteractionService = new Mock<IUserInteractionService>();
            userInteractionService
                .Setup(x => x.ConfirmAsync(It.IsAny<string>(), "", null, null, true))
                .ReturnsAsync(true);

            viewModel = CreateViewModel(
                questionnaireStorage: questionnaireStorage,
                interviewRepository: interviewRepository,
                filteredOptionsViewModel: filteredOptionsViewModel,
                userInteractionService: userInteractionService.Object);

            interview.AnswerMultipleOptionsQuestion(Id.gF, Id.g1, RosterVector.Empty, DateTimeOffset.UtcNow, new [] { 1, 2 });
            
            viewModel.Init("blah", Create.Entity.Identity(Id.g1, Empty.RosterVector), Create.Other.NavigationState());

            var option = viewModel.Options.First();
            option.Checked = false;
            await viewModel.ToggleAnswerAsync(option);
        }

        [Test] 
        public void should_undo_checked_property_change ()
        {
            viewModel.Options.First().Checked.Should().BeFalse();
        }

        [Test] 
        public void should_show_confirmation_dialog ()
        {
            var message = string.Format(UIResources.Interview_Questions_RemoveRowFromRosterMessage, 1);
            userInteractionService.Verify(x => x.ConfirmAsync(message, "", null, null, true), Times.Once);
        }

        static MultiOptionQuestionViewModel viewModel;
        private Mock<IUserInteractionService> userInteractionService;
    }
}
