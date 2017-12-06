using System;
using System.Collections.Generic;
using System.Threading;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
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

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DeleteQuestionnaireServiceTests
{
    internal class DeleteQuestionnaireTests : DeleteQuestionnaireServiceTestContext
    {
        [Test]
        public void when_deletequestionnaire_during_assignment_preloading()
        {
            commandServiceMock = new Mock<ICommandService>();
            commandServiceMock.Setup(
                x =>
                    x.Execute(Moq.It.Is<HardDeleteInterview>(_ => _.InterviewId == interviewId && _.UserId == userId),
                        Moq.It.IsAny<string>())).Callback(() => { Thread.Sleep(1000); });

            plainQuestionnaireRepository = new Mock<IQuestionnaireStorage>();
            interviewsToDeleteFactoryMock = new Mock<IInterviewsToDeleteFactory>();

            var interviewQueue = new Queue<List<InterviewSummary>>();
            interviewQueue.Enqueue(new List<InterviewSummary>() {new InterviewSummary() {InterviewId = interviewId}});
            interviewQueue.Enqueue(new List<InterviewSummary>());
            interviewsToDeleteFactoryMock.Setup(x => x.Load(questionnaireId, questionnaireVersion))
                .Returns(interviewQueue.Dequeue);

            var questionnaireBrowseItemStorageMock = new Mock<IPlainStorageAccessor<QuestionnaireBrowseItem>>();
            var questionnaireBrowseItemQueue = new Queue<QuestionnaireBrowseItem>();
            questionnaireBrowseItemQueue.Enqueue(new QuestionnaireBrowseItem()
            {
                Disabled = false,
                QuestionnaireId = questionnaireId,
                Version = questionnaireVersion
            });
            questionnaireBrowseItemQueue.Enqueue(new QuestionnaireBrowseItem()
            {
                Disabled = true,
                QuestionnaireId = questionnaireId,
                Version = questionnaireVersion
            });
            questionnaireBrowseItemStorageMock.Setup(x => x.GetById(Moq.It.IsAny<string>()))
                .Returns(questionnaireBrowseItemQueue.Dequeue);

            var interviewImportService = new Mock<IInterviewImportService>();
            interviewImportService.Setup(x => x.Status).Returns(
                new AssignmentImportStatus()
                {
                    QuestionnaireId = new QuestionnaireIdentity(questionnaireId, questionnaireVersion),
                    IsInProgress = true
                });

            deleteQuestionnaireService = CreateDeleteQuestionnaireService(commandService: commandServiceMock.Object,
                interviewsToDeleteFactory: interviewsToDeleteFactoryMock.Object,
                questionnaireStorage: plainQuestionnaireRepository.Object,
                questionnaireBrowseItemStorage: questionnaireBrowseItemStorageMock.Object,
                interviewImportService: interviewImportService.Object);

            //AA
            deleteQuestionnaireService.DeleteQuestionnaire(questionnaireId, questionnaireVersion,
                    userId).Wait();
            //AAA
            commandServiceMock.Verify(
                    x => x.Execute(
                        Moq.It.Is<DisableQuestionnaire>(_ =>
                            _.QuestionnaireId == questionnaireId && _.QuestionnaireVersion == questionnaireVersion &&
                            _.ResponsibleId == userId), Moq.It.IsAny<string>()), Times.Once);

            commandServiceMock.Verify(
                    x => x.Execute(
                        Moq.It.Is<DeleteQuestionnaire>(_ =>
                            _.QuestionnaireId == questionnaireId && _.QuestionnaireVersion == questionnaireVersion &&
                            _.ResponsibleId == userId), Moq.It.IsAny<string>()), Times.Never);
        }

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
