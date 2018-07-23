using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionQuestionViewModelTests
{
    internal class when_interview_throws_exception_on_answer_processing: MultiOptionQuestionViewModelTestsContext
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
                Create.Entity.MultyOptionsQuestion(Id.g1, options),
                Create.Entity.MultiRoster(Id.g2, rosterSizeQuestionId: Id.g1)
            );
            var questionnaireStorage = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);

            var interview = Setup.StatefulInterview(questionnaire);
            var interviewRepository = Setup.StatefulInterviewRepository(interview);

            var answeringViewModel = new Mock<AnsweringViewModel>();
            answeringViewModel
                .Setup(x => x.SendAnswerQuestionCommandAsync(It.IsAny<AnswerQuestionCommand>()))
                .ThrowsAsync(new InterviewException("Test exception"));

            viewModel = CreateViewModel(
                questionnaireStorage: questionnaireStorage,
                interviewRepository: interviewRepository,
                filteredOptionsViewModel: Setup.FilteredOptionsViewModel(options),
                answeringViewModel: answeringViewModel.Object);

            interview.AnswerMultipleOptionsQuestion(Id.gF, Id.g1, RosterVector.Empty, DateTimeOffset.UtcNow, new [] { 1, 3 });
            
            viewModel.Init("blah", Create.Entity.Identity(Id.g1, Empty.RosterVector), Create.Other.NavigationState());

            var option = viewModel.Options.Second();
            option.Checked = true;
            await viewModel.ToggleAnswerAsync(option);
        }

        [Test] 
        public void should_reset_all_options_to_interview_state()
        {
            viewModel.Options.ElementAt(0).Checked.Should().BeTrue();
            viewModel.Options.ElementAt(1).Checked.Should().BeFalse();
            viewModel.Options.ElementAt(2).Checked.Should().BeTrue();
            viewModel.Options.ElementAt(3).Checked.Should().BeFalse();
        }

        static MultiOptionQuestionViewModel viewModel;
    }
}