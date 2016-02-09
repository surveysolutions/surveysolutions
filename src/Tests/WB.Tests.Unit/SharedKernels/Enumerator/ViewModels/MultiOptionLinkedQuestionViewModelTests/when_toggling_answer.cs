using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using Nito.AsyncEx.Synchronous;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionLinkedQuestionViewModelTests
{
    internal class when_toggling_answer : MultiOptionLinkedQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            questionId = Create.Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), Empty.RosterVector);
            Guid linkedToQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var interview = Mock.Of<IStatefulInterview>(x =>
                x.FindAnswersOfReferencedQuestionForLinkedQuestion(Moq.It.IsAny<Guid>(), Moq.It.IsAny<Identity>()) == new[] 
                {
                    Create.TextAnswer("answer1", linkedToQuestionId, new []{1m}),
                    Create.TextAnswer("answer2", linkedToQuestionId, new []{2m})
                } &&
                x.Answers == new Dictionary<string, BaseInterviewAnswer>()
                );

            var questionnaire = Create.QuestionnaireModel();
            questionnaire.Questions = new Dictionary<Guid, BaseQuestionModel>();
            questionnaire.Questions.Add(questionId.Id, new LinkedMultiOptionQuestionModel
            {
                LinkedToQuestionId = linkedToQuestionId
            });
            questionnaire.Questions.Add(linkedToQuestionId, new TextQuestionModel());

            var interviews = new Mock<IStatefulInterviewRepository>();
            var questionnaires = new Mock<IPlainKeyValueStorage<QuestionnaireModel>>();

            interviews.SetReturnsDefault(interview);
            questionnaires.SetReturnsDefault(questionnaire);

            answering = new Mock<AnsweringViewModel>();

            questionViewModel = CreateViewModel(interviewRepository: interviews.Object, questionnaireStorage: questionnaires.Object, answering:answering.Object);
            questionViewModel.Init("interviewId", questionId, Create.NavigationState());
        };

        Because of = () =>
        {
            questionViewModel.Options.First().Checked = true;
            questionViewModel.ToggleAnswerAsync(questionViewModel.Options.First()).WaitAndUnwrapException();
        };

        private It should_send_command_with_selected_roster_vectors = () =>
            answering.Verify(x => x.SendAnswerQuestionCommandAsync(Moq.It.Is<AnswerMultipleOptionsLinkedQuestionCommand>(c =>
                c.QuestionId == questionId.Id && c.SelectedRosterVectors.Any(pv => pv.SequenceEqual(questionViewModel.Options.First().Value)))));

        static MultiOptionLinkedToQuestionQuestionViewModel questionViewModel;
        static Identity questionId;
        static Mock<AnsweringViewModel> answering;
    }
}

