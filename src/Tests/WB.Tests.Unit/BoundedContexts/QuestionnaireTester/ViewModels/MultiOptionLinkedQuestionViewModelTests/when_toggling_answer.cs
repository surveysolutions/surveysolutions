using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using Nito.AsyncEx.Synchronous;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.MultiOptionLinkedQuestionViewModelTests
{
    public class when_toggling_answer : MultiOptionLinkedQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            questionId = Create.Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), Empty.RosterVector);
            Guid linkedToQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var interview = Mock.Of<IStatefulInterview>(x =>
                x.FindAnswersByQuestionId(Moq.It.IsAny<Guid>()) == new[] 
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

            viewModel = CreateViewModel(interviewRepository: interviews.Object, questionnaireStorage: questionnaires.Object, answering:answering.Object);
            viewModel.Init("interviewId", questionId, Create.NavigationState());
        };

        Because of = () =>
        {
            viewModel.Options.First().Checked = true;
            viewModel.ToggleAnswerAsync(viewModel.Options.First()).WaitAndUnwrapException();
        };

        private It should_send_command_with_selected_roster_vectors = () =>
            answering.Verify(x => x.SendAnswerQuestionCommand(Moq.It.Is<AnswerMultipleOptionsLinkedQuestionCommand>(c =>
                c.QuestionId == questionId.Id && c.SelectedPropagationVectors.Any(pv => pv.SequenceEqual(viewModel.Options.First().Value)))));

        static MultiOptionLinkedQuestionViewModel viewModel;
        static Identity questionId;
        static Mock<AnsweringViewModel> answering;
    }
}

