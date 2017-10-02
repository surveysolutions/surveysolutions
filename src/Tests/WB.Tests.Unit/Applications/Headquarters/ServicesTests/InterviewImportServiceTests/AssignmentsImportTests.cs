using System;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Applications.Headquarters.ServicesTests.InterviewImportServiceTests
{
    [TestFixture]
    public class AssignmentsImportTests
    {
        [SetUp]
        public void SetUp()
        {
            commandServiceMock = new Mock<ICommandService>();
            interviewImportDataParsingServiceMock = new Mock<IInterviewImportDataParsingService>();
            questionnaireStorageMock = new Mock<IQuestionnaireStorage>();
            interviewKeyGeneratorMock = new Mock<IInterviewUniqueKeyGenerator>();
            var plainTransactionManagerProviderMock = new Mock<IPlainTransactionManagerProvider>();
            plainTransactionManagerProviderMock
                .Setup(x => x.GetPlainTransactionManager())
                .Returns(Mock.Of<IPlainTransactionManager>());

            var transactionManagerProviderMock = new Mock<ITransactionManagerProvider>();
            transactionManagerProviderMock.Setup(x => x.GetTransactionManager())
                .Returns(Mock.Of<ITransactionManager>());
            assignmentPlainStorageAccessorMock = new Mock<IPlainStorageAccessor<Assignment>>();
            userViewFactoryMock = new Mock<IUserViewFactory>();
            preloadedDataRepositoryMock = new Mock<IPreloadedDataRepository>();
            preloadedDataVerifierMock = new Mock<IPreloadedDataVerifier>();
            
            questionnaireBrowseItemStorageMock = new Mock<IPlainStorageAccessor<QuestionnaireBrowseItem>>();
            questionnaireBrowseItemStorageMock.Setup(x => x.GetById(Moq.It.IsAny<string>()))
                .Returns(()=>new QuestionnaireBrowseItem());

            importService = new InterviewImportService(
                commandServiceMock.Object,
                Mock.Of<ILogger>(),
                Create.Entity.SampleImportSettings(),
                interviewImportDataParsingServiceMock.Object,
                questionnaireStorageMock.Object,
                interviewKeyGeneratorMock.Object,
                plainTransactionManagerProviderMock.Object,
                transactionManagerProviderMock.Object,
                assignmentPlainStorageAccessorMock.Object,
                userViewFactoryMock.Object,
                Create.Service.InterviewTreeBuilder(),
                preloadedDataRepositoryMock.Object,
                preloadedDataVerifierMock.Object,
                questionnaireBrowseItemStorageMock.Object);
        }

        [Test]
        public void when_importing_assignment_and_assignmets_are_not_allowed()
        {
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity(Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF"), 1);
            var importProcessId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA").FormatGuid();
            var responsibleId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var supervisorId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            var numericId = Guid.Parse("44444444444444444444444444444444");

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(numericId)
            ));

            questionnaireStorageMock.Setup(x => x.GetQuestionnaire(questionnaireIdentity, null)).Returns(questionnaire);

            InterviewAnswer[] answers =
            {
                Create.Entity.InterviewAnswer(Create.Identity(numericId, RosterVector.Empty), Create.Entity.NumericIntegerAnswer(1)),
            };
          
            var assignments = new []
            {
                Create.Entity.AssignmentImportData(responsibleId, supervisorId, answers)
            };

            interviewImportDataParsingServiceMock
                .Setup(x => x.GetAssignmentsData(importProcessId, questionnaireIdentity, AssignmentImportType.Panel))
                .Returns(assignments);

            CreateInterview createInterviewCommand = null;

            commandServiceMock
                .Setup(x => x.Execute(It.IsAny<ICommand>(), null))
                .Callback<ICommand, string>((command, o) =>
                {
                    createInterviewCommand = command as CreateInterview;
                });

            importService.ImportAssignments(questionnaireIdentity, importProcessId, null, Guid.NewGuid(),  AssignmentImportType.Panel, false);

            Assert.That(importService.Status.IsInProgress, Is.False);
            Assert.That(importService.Status.TotalCount, Is.EqualTo(1));
            Assert.That(importService.Status.ProcessedCount, Is.EqualTo(1));
            assignmentPlainStorageAccessorMock.Verify(x => x.Store(It.IsAny<Assignment>(), null), Times.Once);
            Assert.That(createInterviewCommand.AssignmentId, Is.Not.Null);
            Assert.That(createInterviewCommand.InterviewerId, Is.EqualTo(responsibleId));
            Assert.That(createInterviewCommand.SupervisorId, Is.EqualTo(supervisorId));
            Assert.That(createInterviewCommand.QuestionnaireId, Is.EqualTo(questionnaireIdentity));
            Assert.That(createInterviewCommand.Answers.Count, Is.EqualTo(1));
        }

        [Test]
        public void when_importing_assignment_and_assignmets_are_allowed()
        {
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity(Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF"), 1);
            var importProcessId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA").FormatGuid();
            var responsibleId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var supervisorId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            var numericId = Guid.Parse("44444444444444444444444444444444");

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(numericId)
            ));

            questionnaireStorageMock.Setup(x => x.GetQuestionnaire(questionnaireIdentity, null)).Returns(questionnaire);

            InterviewAnswer[] answers =
            {
                Create.Entity.InterviewAnswer(Create.Identity(numericId, RosterVector.Empty), Create.Entity.NumericIntegerAnswer(1)),
            };

            var assignments = new[]
            {
                Create.Entity.AssignmentImportData(responsibleId, supervisorId, answers)
            };

            interviewImportDataParsingServiceMock
                .Setup(x => x.GetAssignmentsData(importProcessId, questionnaireIdentity, AssignmentImportType.Panel))
                .Returns(assignments);

            importService.ImportAssignments(questionnaireIdentity, importProcessId, null, Guid.NewGuid(), AssignmentImportType.Panel, true);

            Assert.That(importService.Status.IsInProgress, Is.False);
            Assert.That(importService.Status.TotalCount, Is.EqualTo(1));
            Assert.That(importService.Status.ProcessedCount, Is.EqualTo(1));
            assignmentPlainStorageAccessorMock.Verify(x => x.Store(It.IsAny<Assignment>(), null), Times.Once);
            commandServiceMock.Verify(x => x.Execute(It.IsAny<ICommand>(), null), Times.Never);
        }

        private InterviewImportService importService;

        private Mock<ICommandService> commandServiceMock;
        private Mock<IInterviewImportDataParsingService> interviewImportDataParsingServiceMock;
        private Mock<IQuestionnaireStorage> questionnaireStorageMock;
        private Mock<IInterviewUniqueKeyGenerator> interviewKeyGeneratorMock;
        private Mock<IPlainStorageAccessor<Assignment>> assignmentPlainStorageAccessorMock;
        private Mock<IUserViewFactory> userViewFactoryMock;
        private Mock<IPreloadedDataRepository> preloadedDataRepositoryMock;
        private Mock<IPreloadedDataVerifier> preloadedDataVerifierMock;
        private Mock<IPlainStorageAccessor<QuestionnaireBrowseItem>> questionnaireBrowseItemStorageMock;

    }
}
