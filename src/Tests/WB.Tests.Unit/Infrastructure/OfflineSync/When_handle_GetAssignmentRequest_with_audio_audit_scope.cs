using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Infrastructure.OfflineSync
{
    [TestOf(typeof(SupervisorAssignmentsHandler))]
    public class When_handle_GetAssignmentRequest_with_audio_audit_scope
    {
        [Test]
        public async Task should_preserve_audio_audit_scope_during_tablet_to_tablet_sync()
        {
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(Id.g10, Id.gA,
                    Create.Entity.TextQuestion(Id.g1, preFilled: true)));

            var answerSerializer = new NewtonInterviewAnswerJsonSerializer();
            var documentStorage = Create.Storage.AssignmentDocumentsInmemoryStorage();

            documentStorage.Store(Create.Entity
                .AssignmentDocument(1, 10, questionnaireIdentity: Create.Entity.QuestionnaireIdentity().ToString())
                .WithAudioAuditScope("household", "members_roster")
                .Build());

            var handler = new SupervisorAssignmentsHandler(documentStorage);

            var assignmentDocument = await handler.GetAssignment(new GetAssignmentRequest { Id = 1 });

            assignmentDocument.Assignment.AudioAuditScope.Should().BeEquivalentTo(new[] { "household", "members_roster" });

            var builder = new AssignmentDocumentFromDtoBuilder(new AnswerToStringConverter(), answerSerializer);
            var rebuilt = builder.GetAssignmentDocument(assignmentDocument.Assignment, questionnaire);

            rebuilt.AudioAuditScope.Should().BeEquivalentTo(new[] { "household", "members_roster" });
        }
    }
}
