using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using NSubstitute;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DeleteQuestionnaireTemplate;
using WB.Core.SharedKernels.SurveyManagement.Services.DeleteQuestionnaireTemplate;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DeleteQuestionnaireServiceTests
{
    internal class when_delete_questionnaire_the_same_simultaneously : DeleteQuestionnaireServiceTestContext
    {
        Establish context = () =>
        {
            commandServiceMock = new Mock<ICommandService>();
            commandServiceMock.Setup(
                x =>
                    x.Execute(Moq.It.Is<HardDeleteInterview>(_ => _.InterviewId == interviewId && _.UserId == userId),
                        Moq.It.IsAny<string>())).Callback(() => { Thread.Sleep(1000); });

            plainQuestionnaireRepository = new Mock<IPlainQuestionnaireRepository>();
            interviewsToDeleteFactoryMock = new Mock<IInterviewsToDeleteFactory>();

            var interviewQueue = new Queue<List<InterviewSummary>>();
            interviewQueue.Enqueue(new List<InterviewSummary>() { new InterviewSummary() { InterviewId = interviewId } });
            interviewQueue.Enqueue(new List<InterviewSummary>());
            interviewsToDeleteFactoryMock.Setup(x => x.Load(questionnaireId, questionnaireVersion))
                .Returns(interviewQueue.Dequeue);

            var questionnaireBrowseItemStorageMock = new Mock<IReadSideRepositoryReader<QuestionnaireBrowseItem>>();
            var questionnaireBrowseItemQueue = new Queue<QuestionnaireBrowseItem>();
            questionnaireBrowseItemQueue.Enqueue(new QuestionnaireBrowseItem() { Disabled = false, QuestionnaireId = questionnaireId, Version = questionnaireVersion });
            questionnaireBrowseItemQueue.Enqueue(new QuestionnaireBrowseItem() { Disabled = true, QuestionnaireId = questionnaireId, Version = questionnaireVersion });
            questionnaireBrowseItemStorageMock.Setup(x => x.GetById(Moq.It.IsAny<string>()))
                .Returns(questionnaireBrowseItemQueue.Dequeue);

            deleteQuestionnaireService = CreateDeleteQuestionnaireService(commandService: commandServiceMock.Object,
                interviewsToDeleteFactory: interviewsToDeleteFactoryMock.Object,
                plainQuestionnaireRepository: plainQuestionnaireRepository.Object,
                questionnaireBrowseItemStorage: questionnaireBrowseItemStorageMock.Object);
        };

         Because of = () =>
        {
            RunDeletes().Wait();
        };

        It should_once_execute_DisableQuestionnaire_Command = () =>
            commandServiceMock.Verify(x => x.Execute(Moq.It.Is<DisableQuestionnaire>(_ => _.QuestionnaireId == questionnaireId && _.QuestionnaireVersion == questionnaireVersion && _.ResponsibleId == userId), Moq.It.IsAny<string>()), Times.Once);

        It should_once_execute_DeleteQuestionnaire_Command = () =>
           commandServiceMock.Verify(x => x.Execute(Moq.It.Is<DeleteQuestionnaire>(_ => _.QuestionnaireId == questionnaireId && _.QuestionnaireVersion == questionnaireVersion && _.ResponsibleId == userId), Moq.It.IsAny<string>()), Times.Once);

        It should_once_execute_HardDeleteInterview_Command = () =>
            commandServiceMock.Verify(x => x.Execute(Moq.It.Is<HardDeleteInterview>(_ => _.InterviewId == interviewId && _.UserId == userId), Moq.It.IsAny<string>()), Times.Once);

        It should_once_call_DeleteQuestionnaireDocument = () =>
            plainQuestionnaireRepository.Verify(x => x.DeleteQuestionnaireDocument(questionnaireId, questionnaireVersion), Times.Once);

        private static async Task RunDeletes()
        {
            var delete1 = deleteQuestionnaireService.DeleteQuestionnaire(questionnaireId, questionnaireVersion,
              userId);
            var delete2 = deleteQuestionnaireService.DeleteQuestionnaire(questionnaireId, questionnaireVersion,
               userId);
            await Task.WhenAll(delete1, delete2);
        }

        private static DeleteQuestionnaireService deleteQuestionnaireService;
        private static Guid questionnaireId = Guid.NewGuid();
        private static long questionnaireVersion = 5;
        private static Guid userId = Guid.Parse("22222222222222222222222222222222");
        private static Guid interviewId = Guid.Parse("33333333333333333333333333333333");
        private static Mock<ICommandService> commandServiceMock;
        private static Mock<IPlainQuestionnaireRepository> plainQuestionnaireRepository;
        private static Mock<IInterviewsToDeleteFactory> interviewsToDeleteFactoryMock;
    }
}
