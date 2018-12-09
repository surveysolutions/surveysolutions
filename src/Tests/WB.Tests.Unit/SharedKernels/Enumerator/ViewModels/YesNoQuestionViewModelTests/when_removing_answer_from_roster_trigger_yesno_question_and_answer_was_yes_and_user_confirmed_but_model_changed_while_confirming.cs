using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.YesNoQuestionViewModelTests
{
    internal class when_removing_answer_from_roster_trigger_yesno_question_and_answer_was_yes_and_user_confirmed_but_model_changed_while_confirming : YesNoQuestionViewModelTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var interviewIdAsString = "hello";
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
                new AnsweredYesNoOption(5, true), // last option set to yes
            });

            var interview = Mock.Of<IStatefulInterview>(x => x.GetYesNoQuestion(questionId) == yesNoAnswer);

            var questionnaireStorage = new Mock<IQuestionnaireStorage>();
            var interviewRepository = new Mock<IStatefulInterviewRepository>();

            questionnaireStorage.SetReturnsDefault(questionnaire);
            interviewRepository.Setup(x => x.Get(interviewIdAsString)).Returns(interview);

            userInteractionServiceMock = new Mock<IUserInteractionService>();

            userInteractionServiceMock
                .Setup(_ => _.ConfirmAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<bool>()))
                .Returns(Task.FromResult(true))
                .Callback(() => yesNoAnswer.SetAnswer(YesNoAnswer.FromAnsweredYesNoOptions(new []
                {
                    new AnsweredYesNoOption(3, true) // option 3 was set to yes while user was thinking on his answer
                })));

            viewModel = CreateViewModel(questionnaireStorage: questionnaireStorage.Object,
                interviewRepository: interviewRepository.Object,
                userInteractionService: userInteractionServiceMock.Object,
                answeringViewModel: answeringViewModelMock.Object,
                filteredOptionsViewModel: filteredOptionsViewModel);

            viewModel.Init(interviewIdAsString, questionId, Create.Other.NavigationState());
            viewModel.Options.Last().Selected = null;
            BecauseOf();
        }

        public void BecauseOf() => viewModel.ToggleAnswerAsync(viewModel.Options.Last(), true).WaitAndUnwrapException();

        [NUnit.Framework.Test] public void should_not_undo_checked_property_change () => viewModel.Options.Last().Selected.Should().BeNull();

        [NUnit.Framework.Test] public void should_call_userInteractionService_for_reduce_roster_size () => 
            userInteractionServiceMock.Verify(s => s.ConfirmAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<bool>()), Times.Once());

        static YesNoQuestionViewModel viewModel;
        static Mock<IUserInteractionService> userInteractionServiceMock;
        static Identity questionId;
        private static Guid questionGuid;
        private static Mock<AnsweringViewModel> answeringViewModelMock = new Mock<AnsweringViewModel>();
    }
}
