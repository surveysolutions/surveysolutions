using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.YesNoQuestionViewModelTests
{
    internal class when_toggling_no_answer_and_this_is_roster_trigger_question : YesNoQuestionViewModelTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionGuid = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionId = Create.Entity.Identity(questionGuid, Empty.RosterVector);

            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.ShouldQuestionRecordAnswersOrder(questionId.Id) == false
                && _.GetMaxSelectedAnswerOptions(questionId.Id) == null
                && _.IsRosterSizeQuestion(questionId.Id) == true
            );

            var filteredOptionsViewModel = Setup.FilteredOptionsViewModel(new List<CategoricalOption>
            {
                Create.Entity.CategoricalQuestionOption(1, "item1"),
                Create.Entity.CategoricalQuestionOption(2, "item2"),
                Create.Entity.CategoricalQuestionOption(3, "item3"),
                Create.Entity.CategoricalQuestionOption(4, "item4"),
                Create.Entity.CategoricalQuestionOption(5, "item5"),
            });

            var yesNoAnswer = Create.Entity.InterviewTreeYesNoQuestion(new[]
            {
                new AnsweredYesNoOption(1, true),
            });

            var interview = Mock.Of<IStatefulInterview>(x => x.GetYesNoQuestion(questionId) == yesNoAnswer);

            var questionnaireStorage = new Mock<IQuestionnaireStorage>();
            var interviewRepository = new Mock<IStatefulInterviewRepository>();

            questionnaireStorage.SetReturnsDefault(questionnaire);
            interviewRepository.SetReturnsDefault(interview);
            userInteractionServiceMock = new Mock<IUserInteractionService>();
            viewModel = CreateViewModel(questionnaireStorage: questionnaireStorage.Object,
                interviewRepository: interviewRepository.Object,
                userInteractionService: userInteractionServiceMock.Object,
                filteredOptionsViewModel: filteredOptionsViewModel);

            viewModel.Init("blah", questionId, Create.Other.NavigationState());
            viewModel.Options.Last().Selected = false;

            BecauseOf();
        }

        public void BecauseOf() => viewModel.ToggleAnswerAsync(viewModel.Options.Last(), null).Wait();

        [NUnit.Framework.Test] public void should_undo_checked_property_change () => viewModel.Options.Last().YesSelected.Should().BeFalse();

        [NUnit.Framework.Test] public void should_dont_call_userInteractionService_for_reduce_roster_size () => 
            userInteractionServiceMock.Verify(s => s.ConfirmAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<bool>()), Times.Never());

        static YesNoQuestionViewModel viewModel;
        static Mock<IUserInteractionService> userInteractionServiceMock;
        static Identity questionId;
        private static Guid questionGuid;
    }
}
