using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using Nito.AsyncEx.Synchronous;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SingleOptionLinkedQuestionViewModelTests
{
    internal class when_selecting_first_option : SingleOptionLinkedQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            var questionnaire = SetupQuestionnaireWithSingleOptionQuestionLinkedToTextQuestion(questionId, linkedToQuestionId);

            var interview = Mock.Of<IStatefulInterview>(_
                => _.FindAnswersOfReferencedQuestionForLinkedQuestion(Moq.It.IsAny<Guid>(), Moq.It.IsAny<Identity>()) == new[]
                {
                    Create.Entity.InterviewTreeTextQuestion("answer1"),
                    Create.Entity.InterviewTreeTextQuestion("answer2"),
                }
                && _.Answers == new Dictionary<string, BaseInterviewAnswer>());

            viewModel = Create.ViewModel.SingleOptionLinkedQuestionViewModel(
                questionnaire: questionnaire,
                interview: interview,
                answering: answeringMock.Object);

            viewModel.Init(interviewId, questionIdentity, navigationState);
        };

        Because of = () =>
            viewModel.OptionSelectedAsync(viewModel.Options.First()).WaitAndUnwrapException();

        It should_execute_AnswerSingleOptionLinkedQuestionCommand_command = () =>
            answeringMock.Verify(x => x.SendAnswerQuestionCommandAsync(Moq.It.IsAny<AnswerSingleOptionLinkedQuestionCommand>()));

        private static SingleOptionLinkedQuestionViewModel viewModel;
        private static string interviewId = "11111111111111111111111111111111";
        private static Guid questionId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        private static Identity questionIdentity = Create.Entity.Identity(questionId, Empty.RosterVector);
        private static Guid linkedToQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static NavigationState navigationState = Create.Other.NavigationState();
        private static Mock<AnsweringViewModel> answeringMock = new Mock<AnsweringViewModel>();
    }
}