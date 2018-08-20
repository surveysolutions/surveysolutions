using System;
using System.Linq;
using Main.Core.Entities.Composite;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionLinkedQuestionViewModelTests
{
    internal class when_answer_throws_an_exception : MultiOptionLinkedQuestionViewModelTestsContext
    {
        [OneTimeSetUp] 
        public void context () {
            questionId = Create.Entity.Identity(Id.g1, Empty.RosterVector);

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(new IComposite[]
            {
                Create.Entity.MultipleOptionsQuestion(questionId.Id, linkedToQuestionId: Id.g2),
                Create.Entity.FixedRoster(fixedTitles: Create.Entity.FixedTitles(1, 2, 3), children: new[]
                {
                    Create.Entity.TextQuestion(Id.g2)
                })
            });

            var interview = Setup.StatefulInterview(questionnaire);


            var interviews = Create.Fake.StatefulInterviewRepositoryWith(interview);
            var questionnaires = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);

            answering = new Mock<AnsweringViewModel>();
            answering.Setup(x =>
                    x.SendAnswerQuestionCommandAsync(It.IsAny<AnswerQuestionCommand>()))
                .Throws(new InterviewException("Error", InterviewDomainExceptionType.AnswerNotAccepted));

            interview.AnswerTextQuestion(Id.gA, Id.g2, Create.Entity.RosterVector(1), DateTime.UtcNow, "answer 1");
            interview.AnswerTextQuestion(Id.gA, Id.g2, Create.Entity.RosterVector(2), DateTime.UtcNow, "answer 2");
            interview.AnswerTextQuestion(Id.gA, Id.g2, Create.Entity.RosterVector(3), DateTime.UtcNow, "answer 3");
            interview.AnswerMultipleOptionsLinkedQuestion(Id.gA, questionId.Id, RosterVector.Empty, DateTimeOffset.UtcNow, new [] { Create.RosterVector(1) });

            questionViewModel = CreateViewModel(interviewRepository: interviews, questionnaireStorage: questionnaires, answering:answering.Object);
            questionViewModel.Init("interviewId", questionId, Create.Other.NavigationState());

            questionViewModel.Options.Last().Checked = true;
            questionViewModel.ToggleAnswerAsync(questionViewModel.Options.Last()).WaitAndUnwrapException();
        }

        [Test] 
        public void should_select_to_the_first_option () =>
            Assert.That(questionViewModel.Options.First().Checked, Is.True);

        [Test] 
        public void should_not_select_the_last_option () =>
            Assert.That(questionViewModel.Options.Last().Checked, Is.False);

        static MultiOptionLinkedToRosterQuestionQuestionViewModel questionViewModel;
        static Identity questionId;
        static Mock<AnsweringViewModel> answering;
    }
}