using System;
using System.Linq;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Nito.AsyncEx.Synchronous;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.YesNoQuestionViewModelTests
{
    internal class when_removing_answer_from_roster_trigger_yesno_question_and_answer_was_yes_and_user_confirmed_but_model_changed_while_confirming : YesNoQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            var interviewIdAsString = "hello";
            questionGuid = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionId = Create.Identity(questionGuid, Empty.RosterVector);

            var questionnaire = BuildDefaultQuestionnaire(questionId);
            ((YesNoQuestionModel) questionnaire.Questions.First().Value).IsRosterSizeQuestion = true;

            var yesNoAnswer = Create.YesNoAnswer(questionGuid, Empty.RosterVector);
            yesNoAnswer.SetAnswers(new[]
            {
                new AnsweredYesNoOption(5, true), // last option set to yes
            });

            var interview = Mock.Of<IStatefulInterview>(x => x.GetYesNoAnswer(questionId) == yesNoAnswer);

            var questionnaireStorage = new Mock<IPlainKeyValueStorage<QuestionnaireModel>>();
            var interviewRepository = new Mock<IStatefulInterviewRepository>();

            questionnaireStorage.SetReturnsDefault(questionnaire);
            interviewRepository.Setup(x => x.Get(interviewIdAsString)).Returns(interview);

            userInteractionServiceMock = new Mock<IUserInteractionService>();

            userInteractionServiceMock
                .Setup(_ => _.ConfirmAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns(Task.FromResult(true))
                .Callback(() => yesNoAnswer.SetAnswers(new []
                {
                    new AnsweredYesNoOption(3, true) // option 3 was set to yes while user was thinking on his answer
                }));

            viewModel = CreateViewModel(questionnaireStorage: questionnaireStorage.Object,
                interviewRepository: interviewRepository.Object,
                userInteractionService: userInteractionServiceMock.Object,
                answeringViewModel: answeringViewModelMock.Object);

            viewModel.Init(interviewIdAsString, questionId, Create.NavigationState());
            viewModel.Options.Last().Selected = null;
        };

        Because of = () => viewModel.ToggleAnswerAsync(viewModel.Options.Last()).WaitAndUnwrapException();

        It should_not_undo_checked_property_change = () => viewModel.Options.Last().Selected.ShouldBeNull();

        It should_call_userInteractionService_for_reduce_roster_size = () => 
            userInteractionServiceMock.Verify(s => s.ConfirmAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>()), Times.Once());

        It should_answer_question_with_updated_state_of_options = () =>
            answeringViewModelMock.Verify(_ => _.SendAnswerQuestionCommandAsync(Moq.It.Is<AnswerYesNoQuestion>(
                command => command.AnsweredOptions.Length == 1
                           && command.AnsweredOptions.Single().OptionValue == 3
                           && command.AnsweredOptions.Single().Yes == true)),
                Times.Once());

        static YesNoQuestionViewModel viewModel;
        static Mock<IUserInteractionService> userInteractionServiceMock;
        static Identity questionId;
        private static Guid questionGuid;
        private static Mock<AnsweringViewModel> answeringViewModelMock = new Mock<AnsweringViewModel>();
    }
}