using System;
using System.Linq;
using Main.Core.Entities.Composite;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SingleOptionLinkedQuestionViewModelTests
{
    internal class when_selecting_first_option : SingleOptionLinkedQuestionViewModelTestsContext
    {
        [OneTimeSetUp] 
        public void context () {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.SingleOptionQuestion(questionId, linkedToQuestionId: linkedToQuestionId),
                Create.Entity.FixedRoster(fixedTitles: new[] { Create.Entity.FixedTitle(1), Create.Entity.FixedTitle(2), Create.Entity.FixedTitle(3) }, children: new IComposite[]
                {
                    Create.Entity.TextQuestion(linkedToQuestionId)
                }));

            var interview = Setup.StatefulInterview(questionnaire);
            var interviewerId = Guid.Parse("77777777777777777777777777777777");

            interview.AnswerTextQuestion(interviewerId, linkedToQuestionId, Create.Entity.RosterVector(1), DateTime.UtcNow, "answer1");
            interview.AnswerTextQuestion(interviewerId, linkedToQuestionId, Create.Entity.RosterVector(2), DateTime.UtcNow, "answer2");

            viewModel = Create.ViewModel.SingleOptionLinkedQuestionViewModel(
                questionnaire: Create.Entity.PlainQuestionnaire(questionnaire),
                interview: interview,
                answering: answeringMock.Object);

            viewModel.Init(interviewId, questionIdentity, navigationState);

            viewModel.OptionSelectedAsync(viewModel.Options.First()).WaitAndUnwrapException();
        }

        [Test] public void should_execute_AnswerSingleOptionLinkedQuestionCommand_command () =>
            answeringMock.Verify(x => x.SendAnswerQuestionCommandAsync(It.IsAny<AnswerSingleOptionLinkedQuestionCommand>()));

        private static SingleOptionLinkedQuestionViewModel viewModel;
        private static string interviewId = "11111111111111111111111111111111";
        private static Guid questionId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        private static Identity questionIdentity = Create.Entity.Identity(questionId, Empty.RosterVector);
        private static Guid linkedToQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static NavigationState navigationState = Create.Other.NavigationState();
        private static Mock<AnsweringViewModel> answeringMock = new Mock<AnsweringViewModel>();
    }
}
