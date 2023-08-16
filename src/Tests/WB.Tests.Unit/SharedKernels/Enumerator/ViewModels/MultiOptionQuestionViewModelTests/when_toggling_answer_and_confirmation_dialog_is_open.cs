using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionQuestionViewModelTests
{
    internal class when_toggling_answer_and_confirmation_dialog_is_open: MultiOptionQuestionViewModelTestsContext
    {
        [OneTimeSetUp] 
        public void context () {
            var options = new[]
            {
                Create.Entity.Option(1, "item1"),
                Create.Entity.Option(2, "item2"),
            };

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultyOptionsQuestion(Id.g1, options, maxAllowedAnswers: 2),
                Create.Entity.MultiRoster(Id.g2, rosterSizeQuestionId: Id.g1)
            );

            var questionnaireStorage = SetUp.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);

            var interview = SetUp.StatefulInterview(questionnaire);
            var interviewRepository = SetUp.StatefulInterviewRepository(interview);

            userInteractionService = new Mock<IUserInteractionService>();
            userInteractionService.Setup(x => x.HasPendingUserInteractions).Returns(true);

            viewModel = CreateViewModel(
                questionnaireStorage: questionnaireStorage,
                interviewRepository: interviewRepository,
                filteredOptionsViewModel: SetUp.FilteredOptionsViewModel(options),
                userInteraction: userInteractionService.Object);

            interview.AnswerMultipleOptionsQuestion(Id.gF, Id.g1, RosterVector.Empty, DateTimeOffset.UtcNow, new [] { 1, 2 });
            
            viewModel.Init("blah", Create.Entity.Identity(Id.g1, Empty.RosterVector), Create.Other.NavigationState());

            var option = viewModel.Options.First();
            option.Checked = false;
            option.CheckAnswerCommand.Execute();
        }

        [Test] 
        public void should_reset_all_options_as_in_interview ()
        {
            viewModel.Options.First().Checked.Should().BeTrue();
            viewModel.Options.Second().Checked.Should().BeTrue();
        }

        static CategoricalMultiViewModel viewModel;
        private Mock<IUserInteractionService> userInteractionService;
    }
}
