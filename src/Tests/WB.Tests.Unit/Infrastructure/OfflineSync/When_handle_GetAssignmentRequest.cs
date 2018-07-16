using System.Threading.Tasks;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Infrastructure.Native.Storage;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Infrastructure.OfflineSync
{
    [TestOf(typeof(SupervisorAssignmentsHandler))]
    public class When_handle_GetAssignmentRequest
    {
        [Test]
        public async Task should_pass_answers()
        {
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(Id.g10, Id.gA,
                    Create.Entity.TextQuestion(Id.g1, preFilled: true)));

            var answerSerializer = new NewtonInterviewAnswerJsonSerializer();

            var documentStorage = Create.Storage.AssignmentDocumentsInmemoryStorage();

            var textAnswer = TextAnswer.FromString("answer");

            documentStorage.Store(Create.Entity.AssignmentDocument(1, 10, questionnaireIdentity: Create.Entity.QuestionnaireIdentity().ToString())
                .WithAnswer(Create.Identity(Id.g1), "answer", identifying: true, 
                    serializedAnswer: answerSerializer.Serialize(textAnswer))
                .Build());

            var handler = new SupervisorAssignmentsHandler(documentStorage, answerSerializer);

            // act
            var assignmentDocument = await handler.GetAssignment(new GetAssignmentRequest{Id = 1});

            // assert

            var answerDocumentBuilder = new AssignmentDocumentFromDtoBuilder(new AnswerToStringConverter(), answerSerializer);

            var assignment = answerDocumentBuilder.GetAssignmentDocument(assignmentDocument.Assignment, questionnaire);
            Assert.That(assignment.Answers[0], Has.Property(nameof(AssignmentDocument.AssignmentAnswer.AnswerAsString)).EqualTo("answer"));
        }
    }
}