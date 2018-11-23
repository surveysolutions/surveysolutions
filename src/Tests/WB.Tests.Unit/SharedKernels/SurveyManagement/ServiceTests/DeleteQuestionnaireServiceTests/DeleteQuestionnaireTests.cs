using System;
using System.Collections.Generic;
using System.Threading;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.DeleteQuestionnaireTemplate;
using WB.Core.BoundedContexts.Headquarters.Services.DeleteQuestionnaireTemplate;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
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
        public void when_deletequestionnaire_during_varification_of_preloaded_assignments()
        {
            DeleteQuestionnaireService deleteQuestionnaireService;
            Guid questionnaireId = Guid.NewGuid();
            long questionnaireVersion = 5;
            Guid userId = Guid.Parse("22222222222222222222222222222222");
            Guid interviewId = Guid.Parse("33333333333333333333333333333333");
            Mock<ICommandService> commandServiceMock;
            Mock<IQuestionnaireStorage> plainQuestionnaireRepository;
            Mock<IInterviewsToDeleteFactory> interviewsToDeleteFactoryMock;

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

            var interviewImportService = new Mock<IAssignmentsImportService>();
            interviewImportService.Setup(x => x.GetImportStatus()).Returns(
                new AssignmentsImportStatus()
                {
                    QuestionnaireIdentity = new QuestionnaireIdentity(questionnaireId, questionnaireVersion),
                    ProcessStatus = AssignmentsImportProcessStatus.Verification
                });

            deleteQuestionnaireService = CreateDeleteQuestionnaireService(commandService: commandServiceMock.Object,
                interviewsToDeleteFactory: interviewsToDeleteFactoryMock.Object,
                questionnaireStorage: plainQuestionnaireRepository.Object,
                questionnaireBrowseItemStorage: questionnaireBrowseItemStorageMock.Object,
                interviewImportService: interviewImportService.Object);

            //AA
            deleteQuestionnaireService.DisableQuestionnaire(questionnaireId, questionnaireVersion,
                    userId);
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

        [Test]
        public void when_deletequestionnaire_during_import_of_preloaded_assignments()
        {
            Guid questionnaireId = Guid.NewGuid();
            long questionnaireVersion = 5;
            Guid userId = Guid.Parse("22222222222222222222222222222222");
            Guid interviewId = Guid.Parse("33333333333333333333333333333333");

            var commandServiceMock = new Mock<ICommandService>();
            commandServiceMock.Setup(
                x =>
                    x.Execute(Moq.It.Is<HardDeleteInterview>(_ => _.InterviewId == interviewId && _.UserId == userId),
                        Moq.It.IsAny<string>())).Callback(() => { Thread.Sleep(1000); });

            var plainQuestionnaireRepository = new Mock<IQuestionnaireStorage>();
            var interviewsToDeleteFactoryMock = new Mock<IInterviewsToDeleteFactory>();

            var interviewQueue = new Queue<List<InterviewSummary>>();
            interviewQueue.Enqueue(new List<InterviewSummary>() { new InterviewSummary() { InterviewId = interviewId } });
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

            var interviewImportService = new Mock<IAssignmentsImportService>();
            interviewImportService.Setup(x => x.GetImportStatus()).Returns(
                new AssignmentsImportStatus()
                {
                    QuestionnaireIdentity = new QuestionnaireIdentity(questionnaireId, questionnaireVersion),
                    ProcessStatus = AssignmentsImportProcessStatus.Import
                });

            var deleteQuestionnaireService = CreateDeleteQuestionnaireService(commandService: commandServiceMock.Object,
                interviewsToDeleteFactory: interviewsToDeleteFactoryMock.Object,
                questionnaireStorage: plainQuestionnaireRepository.Object,
                questionnaireBrowseItemStorage: questionnaireBrowseItemStorageMock.Object,
                interviewImportService: interviewImportService.Object);

            //AA
            deleteQuestionnaireService.DisableQuestionnaire(questionnaireId, questionnaireVersion,
                    userId);
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
    }
}
