using System;
using System.Linq;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Nito.AsyncEx.Synchronous;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.YesNoQuestionViewModelTests
{
    internal class when_toggling_no_answer_roster_size_question_as_the_first_time_asnwer : YesNoQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            interviewIdAsString = "hello";
            questionGuid = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionId = Create.Identity(questionGuid, Empty.RosterVector);
           
            var questionnaire = Mock.Of<IQuestionnaire>(_ 
                => _.ShouldQuestionRecordAnswersOrder(questionId.Id) == false
                && _.GetMaxSelectedAnswerOptions(questionId.Id) == null
                && _.ShouldQuestionSpecifyRosterSize(questionId.Id) == true
                && _.GetAnswerOptionsAsValues(questionId.Id) == new decimal[] { 1, 2, 3, 4, 5}
                && _.GetAnswerOptionTitle(questionId.Id, 1) == "item1"
                && _.GetAnswerOptionTitle(questionId.Id, 2) == "item2"
                && _.GetAnswerOptionTitle(questionId.Id, 3) == "item3"
                && _.GetAnswerOptionTitle(questionId.Id, 4) == "item4"
                && _.GetAnswerOptionTitle(questionId.Id, 5) == "item5"
            );

            var yesNoAnswer = Create.YesNoAnswer(questionGuid, Empty.RosterVector);
           
            var interview = Mock.Of<IStatefulInterview>(x => x.GetYesNoAnswer(questionId) == yesNoAnswer);

            var questionnaireStorage = new Mock<IPlainQuestionnaireRepository>();
            interviewRepository = new Mock<IStatefulInterviewRepository>();

            questionnaireStorage.SetReturnsDefault(questionnaire);
            interviewRepository.Setup(x => x.Get(interviewIdAsString)).Returns(interview);
            answeringViewModelMock = new Mock<AnsweringViewModel>();
            answeringViewModelMock
                .Setup(x => x.SendAnswerQuestionCommandAsync(Moq.It.IsAny<AnswerQuestionCommand>()))
                .Callback((AnswerQuestionCommand command) => { answerCommand = command; })
                .Returns(Task.FromResult<bool>(true));

            viewModel = CreateViewModel(questionnaireStorage: questionnaireStorage.Object, interviewRepository: interviewRepository.Object, 
                answeringViewModel: answeringViewModelMock.Object);

            viewModel.Init(interviewIdAsString, questionId, Create.NavigationState());
            viewModel.Options.First().Selected = false;
        };

        Because of = () => viewModel.ToggleAnswerAsync(viewModel.Options.First()).WaitAndUnwrapException();

        It should_send_answering_command = () =>
            answeringViewModelMock.Verify(x => x.SendAnswerQuestionCommandAsync(Moq.It.IsAny<AnswerQuestionCommand>()), Times.Once);
        
        It should_send_command_with_toggled_first_option = () =>
            ((AnswerYesNoQuestion)answerCommand).AnsweredOptions.Single().OptionValue.ShouldEqual(1);

        It should_send_command_with_toggled_NO_answer = () =>
            ((AnswerYesNoQuestion)answerCommand).AnsweredOptions.Single().Yes.ShouldBeFalse();

        private static AnswerQuestionCommand answerCommand;
        private static YesNoQuestionViewModel viewModel;
        private static Identity questionId;
        private static Guid questionGuid;
        private static Mock<IStatefulInterviewRepository> interviewRepository;
        private static string interviewIdAsString;
        private static Mock<AnsweringViewModel> answeringViewModelMock;
    }
}