using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Applications.Headquarters.ServicesTests.InterviewImportServiceTests
{
    [TestFixture]
    class AssignmentVerificationTests
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
                preloadedDataVerifierMock.Object);
        }

        [Test]
        public void when_verifying_assignments()
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

            importService.VerifyAssignments(questionnaireIdentity, importProcessId, "hello.tab");

            Assert.That(importService.Status.TotalCount, Is.EqualTo(1));
            Assert.That(importService.Status.ProcessedCount, Is.EqualTo(1));
            Assert.That(importService.Status.VerificationState.Errors.Count, Is.EqualTo(0));
        }

        [Test]
        public void when_verifying_assignment_with_text_question_answered()
        {
            var questionId = Guid.Parse("11111111111111111111111111111111");
            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextQuestion(questionId)
            ));

            var answers = new List<InterviewAnswer>
            {
                Create.Entity.InterviewAnswer(Create.Identity(questionId), Create.Entity.TextQuestionAnswer("Hello"))
            };

            var result = importService.VerifyAssignment(answers.GroupedByLevels(), questionnaire);

            Assert.That(result.Status, Is.True);
        }

        [Test]
        public void when_verifying_assignment_with_numeric_answer_that_is_roster_size_with_exceeded_limit()
        {
            var questionId = Guid.Parse("11111111111111111111111111111111");
            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(questionId),
                Create.Entity.NumericRoster(rosterSizeQuestionId: questionId)
            ));

            var answers = new List<InterviewAnswer>
            {
                Create.Entity.InterviewAnswer(Create.Identity(questionId), Create.Entity.NumericIntegerAnswer(Constants.MaxLongRosterRowCount + 1))
            };

            var result = importService.VerifyAssignment(answers.GroupedByLevels(), questionnaire);

            Assert.That(result.Status, Is.False);
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
    }
}
