using System;
using System.Linq;
using Main.Core.Entities.Composite;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionLinkedQuestionViewModelTests
{
    internal class when_toggling_answer : MultiOptionLinkedQuestionViewModelTestsContext
    {
        [OneTimeSetUp] 
        public void context () {
            questionId = Create.Entity.Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), Empty.RosterVector);
            Guid linkedToQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            Guid userId = Guid.Parse("77777777777777777777777777777777");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(new IComposite[]
            {
                Create.Entity.MultipleOptionsQuestion(questionId.Id, linkedToQuestionId: linkedToQuestionId),
                Create.Entity.FixedRoster(fixedTitles: new[] {new FixedRosterTitle(1, "fixed")}, children: new[]
                {
                    Create.Entity.TextQuestion(linkedToQuestionId)
                })
            });

            var interview = Setup.StatefulInterview(questionnaire);


            var interviews = Create.Fake.StatefulInterviewRepositoryWith(interview);
            var questionnaires = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);

            answering = new Mock<AnsweringViewModel>();
            interview.AnswerTextQuestion(userId, linkedToQuestionId, Create.Entity.RosterVector(1), DateTime.UtcNow, "some answer");

            questionViewModel = CreateViewModel(interviewRepository: interviews, questionnaireStorage: questionnaires, answering:answering.Object);
            questionViewModel.Init("interviewId", questionId, Create.Other.NavigationState());

            questionViewModel.Options.First().Checked = true;
            questionViewModel.ToggleAnswerAsync(questionViewModel.Options.First()).WaitAndUnwrapException();
        }

        [Test] 
        public void should_send_command_with_selected_roster_vectors () =>
            answering.Verify(x => x.SendAnswerQuestionCommandAsync(Moq.It.Is<AnswerMultipleOptionsLinkedQuestionCommand>(c =>
                c.QuestionId == questionId.Id && c.SelectedRosterVectors.Any(pv => pv.Identical(questionViewModel.Options.First().Value)))));

        static MultiOptionLinkedToRosterQuestionQuestionViewModel questionViewModel;
        static Identity questionId;
        static Mock<AnsweringViewModel> answering;
    }
}

