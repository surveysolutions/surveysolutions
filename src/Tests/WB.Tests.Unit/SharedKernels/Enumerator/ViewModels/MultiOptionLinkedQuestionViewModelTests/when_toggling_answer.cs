using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using Nito.AsyncEx.Synchronous;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;

using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionLinkedQuestionViewModelTests
{
    [Ignore("KP-8159")]
    internal class when_toggling_answer : MultiOptionLinkedQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            questionId = Create.Entity.Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), Empty.RosterVector);
            Guid linkedToQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            IStatefulInterview interview = null;
            //Mock.Of<IStatefulInterview>(x =>
            //    x.FindAnswersOfReferencedQuestionForLinkedQuestion(Moq.It.IsAny<Guid>(), Moq.It.IsAny<Identity>()) == new[] 
            //    {
            //        Create.Entity.InterviewTreeTextQuestion("answer1", linkedToQuestionId, new []{1m}),
            //        Create.Entity.InterviewTreeTextQuestion("answer2", linkedToQuestionId, new []{2m})
            //    } &&
            //    x.Answers == new Dictionary<string, BaseInterviewAnswer>()
            //    );

            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.GetQuestionReferencedByLinkedQuestion(questionId.Id) == linkedToQuestionId
                && _.ShouldQuestionRecordAnswersOrder(questionId.Id) == false);

            var interviews = new Mock<IStatefulInterviewRepository>();
            var questionnaires = new Mock<IQuestionnaireStorage>();

            interviews.SetReturnsDefault(interview);
            questionnaires.SetReturnsDefault(questionnaire);

            answering = new Mock<AnsweringViewModel>();

            questionViewModel = CreateViewModel(interviewRepository: interviews.Object, questionnaireStorage: questionnaires.Object, answering:answering.Object);
            questionViewModel.Init("interviewId", questionId, Create.Other.NavigationState());
        };

        Because of = () =>
        {
            questionViewModel.Options.First().Checked = true;
            questionViewModel.ToggleAnswerAsync(questionViewModel.Options.First()).WaitAndUnwrapException();
        };

        private It should_send_command_with_selected_roster_vectors = () =>
            answering.Verify(x => x.SendAnswerQuestionCommandAsync(Moq.It.Is<AnswerMultipleOptionsLinkedQuestionCommand>(c =>
                c.QuestionId == questionId.Id && c.SelectedRosterVectors.Any(pv => pv.SequenceEqual(questionViewModel.Options.First().Value)))));

        static MultiOptionLinkedToRosterQuestionQuestionViewModel questionViewModel;
        static Identity questionId;
        static Mock<AnsweringViewModel> answering;
    }
}

