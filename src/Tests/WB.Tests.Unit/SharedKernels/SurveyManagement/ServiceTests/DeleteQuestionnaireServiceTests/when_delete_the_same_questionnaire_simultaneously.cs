using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.DeleteQuestionnaireTemplate;
using WB.Core.BoundedContexts.Headquarters.Services.DeleteQuestionnaireTemplate;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DeleteQuestionnaireServiceTests
{
    internal class when_delete_the_same_questionnaire_simultaneously : DeleteQuestionnaireServiceTestContext
    {
        [OneTimeSetUp]
        public async Task Establish()
        {
            commandServiceMock = new Mock<ICommandService>();
            commandServiceMock.Setup(
                x =>
                    x.Execute(It.Is<HardDeleteInterview>(_ => _.InterviewId == interviewId && _.UserId == userId),
                        It.IsAny<string>())).Callback(() => { Thread.Sleep(1000); });

            plainQuestionnaireRepository = new Mock<IQuestionnaireStorage>();
            plainQuestionnaireRepository.Setup(s =>
                    s.GetQuestionnaireDocument(new QuestionnaireIdentity(questionnaireId, questionnaireVersion)))
                .Returns(new QuestionnaireDocument());

            Setup.InstanceToMockedServiceLocator(plainQuestionnaireRepository.Object);
            interviewsToDeleteFactoryMock = new Mock<IInterviewsToDeleteFactory>();

            var interviewQueue = new Queue<List<InterviewSummary>>();
            interviewQueue.Enqueue(new List<InterviewSummary> { new InterviewSummary { InterviewId = interviewId } });
            interviewQueue.Enqueue(new List<InterviewSummary>());
            interviewsToDeleteFactoryMock.Setup(x => x.Load(questionnaireId, questionnaireVersion))
                .Returns(interviewQueue.Dequeue);

            var questionnaireBrowseItemStorageMock = new Mock<IPlainStorageAccessor<QuestionnaireBrowseItem>>();
            var questionnaireBrowseItemQueue = new Queue<QuestionnaireBrowseItem>();
            questionnaireBrowseItemQueue.Enqueue(new QuestionnaireBrowseItem { Disabled = false, QuestionnaireId = questionnaireId, Version = questionnaireVersion });
            questionnaireBrowseItemQueue.Enqueue(new QuestionnaireBrowseItem { Disabled = true, QuestionnaireId = questionnaireId, Version = questionnaireVersion });
            questionnaireBrowseItemStorageMock.Setup(x => x.GetById(It.IsAny<string>()))
                .Returns(questionnaireBrowseItemQueue.Dequeue);

            deleteQuestionnaireService = CreateDeleteQuestionnaireService(commandService: commandServiceMock.Object,
                interviewsToDeleteFactory: interviewsToDeleteFactoryMock.Object,
                questionnaireStorage: plainQuestionnaireRepository.Object,
                questionnaireBrowseItemStorage: questionnaireBrowseItemStorageMock.Object);

            await Because();
        }

        public async Task Because()
        {
            var delete1 = deleteQuestionnaireService.DeleteQuestionnaire(questionnaireId, questionnaireVersion, userId);
            var delete2 = deleteQuestionnaireService.DeleteQuestionnaire(questionnaireId, questionnaireVersion, userId);
            await Task.WhenAll(delete1, delete2);
        }

        [Test]
        public void should_once_execute_DisableQuestionnaire_Command() =>
            commandServiceMock.Verify(x => x.Execute(It.Is<DisableQuestionnaire>(_ 
                => _.QuestionnaireId == questionnaireId && _.QuestionnaireVersion == questionnaireVersion && _.ResponsibleId == userId), It.IsAny<string>()), Times.Once);

        [Test]
        public void should_once_execute_DeleteQuestionnaire_Command() =>
           commandServiceMock.Verify(x => x.Execute(It.Is<DeleteQuestionnaire>(_
               => _.QuestionnaireId == questionnaireId && _.QuestionnaireVersion == questionnaireVersion && _.ResponsibleId == userId), 
                It.IsAny<string>()), Times.Once);

        [Test]
        public void should_once_execute_HardDeleteInterview_Command() =>
            commandServiceMock.Verify(x => x.Execute(It.Is<HardDeleteInterview>(_ => _.InterviewId == interviewId && _.UserId == userId), It.IsAny<string>()), Times.Once);

        private static DeleteQuestionnaireService deleteQuestionnaireService;
        private static Guid questionnaireId = Guid.NewGuid();
        private static long questionnaireVersion = 5;
        private static Guid userId = Guid.Parse("22222222222222222222222222222222");
        private static Guid interviewId = Guid.Parse("33333333333333333333333333333333");
        private static Mock<ICommandService> commandServiceMock;
        private static Mock<IQuestionnaireStorage> plainQuestionnaireRepository;
        private static Mock<IInterviewsToDeleteFactory> interviewsToDeleteFactoryMock;
    }
}
