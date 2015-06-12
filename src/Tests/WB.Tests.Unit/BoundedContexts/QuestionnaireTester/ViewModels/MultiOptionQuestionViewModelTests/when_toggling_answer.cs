using System;
using System.Linq;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using It = System.Reflection.PortableExecutable.Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.MultiOptionQuestionViewModelTests
{
    public class when_toggling_answer : MultiOptionQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            questionGuid = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionId = Create.Identity(questionGuid, Empty.RosterVector);

            var questionnaire = BuildDefaultQuestionnaire(questionId);
            ((MultiOptionQuestionModel)questionnaire.Questions.First().Value).MaxAllowedAnswers = null;

            var multiOptionAnswer = Create.MultiOptionAnswer(questionGuid, Empty.RosterVector);
            multiOptionAnswer.SetAnswers(new[] { 1m });

            var interview = Mock.Of<IStatefulInterview>(x => x.GetMultiOptionAnswer(questionId) == multiOptionAnswer);

            var questionnaireStorage = new Mock<IPlainKeyValueStorage<QuestionnaireModel>>();
            var interviewRepository = new Mock<IStatefulInterviewRepository>();

            questionnaireStorage.SetReturnsDefault(questionnaire);
            interviewRepository.SetReturnsDefault(interview);

            answeringMock = new Mock<AnsweringViewModel>();
            answeringMock.Setup(x => x.SendAnswerQuestionCommand(Moq.It.IsAny<AnswerQuestionCommand>()))
                    .Returns(Task.FromResult(true));

            viewModel = CreateViewModel(questionnaireStorage: questionnaireStorage.Object,
                interviewRepository: interviewRepository.Object,
                answeringViewModel: answeringMock.Object);

            viewModel.Init("blah", questionId, new NavigationState());
            viewModel.Options.Second().Checked = true;
        };

        Because of = async () => await viewModel.ToggleAnswer(viewModel.Options.Second());

        It should_send_command_to_service = () => answeringMock.Verify(x => x.SendAnswerQuestionCommand(Moq.It.Is<AnswerMultipleOptionsQuestionCommand>(c => 
            c.SelectedValues.SequenceEqual(new []{1m,2m}))));

        static MultiOptionQuestionViewModel viewModel;
        static Identity questionId;
        static Guid questionGuid;
        static Mock<AnsweringViewModel> answeringMock;
    }
}

