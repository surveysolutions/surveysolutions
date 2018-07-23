using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;


namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests.AssignmentTests
{
    public class when_synchronize_assignments_with_changes_in_responsible
    {
        private List<AssignmentDocument> localAssignments;
        private List<AssignmentApiDocument> remoteAssignments;
        private IAssignmentDocumentsStorage localAssignmentsRepo;

        private void Context()
        {
            localAssignments = new List<AssignmentDocument>
            {
                Create.Entity
                    .AssignmentDocument(4, 10, 0, Create.Entity.QuestionnaireIdentity(Id.gB).ToString(), Id.g1)
                    .WithAnswer(Create.Entity.Identity(Guid.NewGuid()), "1")
                    .WithAnswer(Create.Entity.Identity(Guid.NewGuid()), "2")
                    .Build(),
                Create.Entity
                    .AssignmentDocument(5, 10, 0, Create.Entity.QuestionnaireIdentity(Id.gB).ToString(), Id.g1, Id.g2)
                    .WithAnswer(Create.Entity.Identity(Guid.NewGuid()), "1")
                    .WithAnswer(Create.Entity.Identity(Guid.NewGuid()), "2")
                    .Build()
            };

            remoteAssignments = new List<AssignmentApiDocument>
            {
                Create.Entity
                    .AssignmentApiDocument(4, 10, Create.Entity.QuestionnaireIdentity(Id.gB), Id.g2)
                    .Build(),
                Create.Entity
                    .AssignmentApiDocument(5, 10, Create.Entity.QuestionnaireIdentity(Id.gB), Id.g2)
                    .Build()
            };
        }

        PlainQuestionnaire CreatePlain(AssignmentApiDocument assignment)
        {
            var questionnaire = Create.Entity.QuestionnaireDocument(assignment.QuestionnaireId.QuestionnaireId);
            questionnaire.Title = "title";
            return Create.Entity.PlainQuestionnaire(questionnaire);
        }

        private AssignmentApiView FromView(AssignmentApiDocument document)
        {
            return new AssignmentApiView
            {
                Id = document.Id,
                Quantity = document.Quantity,
                QuestionnaireId = document.QuestionnaireId,
                ResponsibleId = document.ResponsibleId
            };
        }

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            Context();

            localAssignmentsRepo = Create.Storage.AssignmentDocumentsInmemoryStorage();
            localAssignmentsRepo.Store(localAssignments);

            var assignmentSyncService = new Mock<ISynchronizationService>();
            assignmentSyncService.Setup(s => s.GetAssignmentsAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(remoteAssignments.Select(FromView).ToList()));

            assignmentSyncService.Setup(s => s.GetAssignmentAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(new Func<int, CancellationToken, Task<AssignmentApiDocument>>((id, token) =>
                {
                    var result = remoteAssignments.FirstOrDefault(i => i.Id == id);

                    return Task.FromResult(result);
                }));

            var interviewViewRepository = Mock.Of<IPlainStorage<InterviewView>>();

            var questionarrieStorage = new Mock<IQuestionnaireStorage>();
            questionarrieStorage
                .Setup(qs => qs.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()))
                .Returns(new Func<QuestionnaireIdentity, string, IQuestionnaire>((identity, version) =>
                {
                    return CreatePlain(remoteAssignments.FirstOrDefault(a => a.QuestionnaireId == identity));
                }));

            var viewModel = Create.Service.AssignmentsSynchronizer(
                synchronizationService: assignmentSyncService.Object,
                assignmentsRepository: localAssignmentsRepo,
                questionnaireStorage: questionarrieStorage.Object,
                interviewViewRepository: interviewViewRepository
            );

            var progressInfo = new Mock<IProgress<SyncProgressInfo>>();

            await viewModel.SynchronizeAssignmentsAsync(progressInfo.Object, new SynchronizationStatistics(), CancellationToken.None);
        }

        [Test]
        public void should_update_responsible_person_for_asignment_4()
        {
            var assignmentDocument = localAssignmentsRepo.GetById(4);

            Assert.That(assignmentDocument.ResponsibleId, Is.EqualTo(Id.g2));
            Assert.That(assignmentDocument.OriginalResponsibleId, Is.EqualTo(Id.g2));
        }

        [Test]
        public void should_dont_update_responsible_person_for_asignment_5()
        {
            var assignmentDocument = localAssignmentsRepo.GetById(5);

            Assert.That(assignmentDocument.ResponsibleId, Is.EqualTo(Id.g1));
            Assert.That(assignmentDocument.OriginalResponsibleId, Is.EqualTo(Id.g2));
        }
    }
}
