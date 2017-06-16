using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Entities.Composite;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Synchronization;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests.AssignmentTests
{
    public class when_synchronize_assignments
    {
        private List<AssignmentDocument> LocalAssignments;

        private List<AssignmentApiView> RemoteAssignments;
        private IAssignmentDocumentsStorage localAssignmentsRepo;
        private Mock<IProgress<SyncProgressInfo>> progressInfo;

        private void Context()
        {
            this.LocalAssignments = new List<AssignmentDocument>
            {
                Create.Entity
                    .AssignmentDocument(1, 10, 0, Create.Entity.QuestionnaireIdentity(Id.gA).ToString())
                    .WithAnswer(Create.Entity.Identity(Guid.NewGuid()), "1")
                    .WithAnswer(Create.Entity.Identity(Guid.NewGuid()), "2")
                    .Build(),
                Create.Entity
                    .AssignmentDocument(2, 10, 0, Create.Entity.QuestionnaireIdentity(Id.gB).ToString())
                    .WithAnswer(Create.Entity.Identity(Guid.NewGuid()), "1")
                    .WithAnswer(Create.Entity.Identity(Guid.NewGuid()), "2")
                    .Build()
            };

            this.RemoteAssignments = new List<AssignmentApiView>
            {
                Create.Entity
                    .AssignmentApiView(1, 20, 0, Create.Entity.QuestionnaireIdentity(Id.gA))
                    .WithAnswer(Create.Entity.Identity(Guid.NewGuid()), "1")
                    .WithAnswer(Create.Entity.Identity(Guid.NewGuid()), "2")
                    .Build(),
                Create.Entity
                    .AssignmentApiView(3, 20, 0, Create.Entity.QuestionnaireIdentity(Id.gC))
                    .WithAnswer(Create.Entity.Identity(Guid.NewGuid()), "1")
                    .WithAnswer(Create.Entity.Identity(Guid.NewGuid()), "2")
                    .Build()
            };
        }

        PlainQuestionnaire CreatePlain(Guid id, AssignmentApiView assignemtn)
        {
            var questionnaire = Create.Entity.QuestionnaireDocument(id, children: new IComposite[]
            {
                Create.Entity.TextQuestion(assignemtn.Answers[0].Identity.Id, text: "text 1"),
                Create.Entity.TextQuestion(assignemtn.Answers[1].Identity.Id, text: "title 2"),
            });

            questionnaire.Title = "title";
            return Create.Entity.PlainQuestionnaire(questionnaire);
        }

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            this.Context();

            this.localAssignmentsRepo = Create.Storage.AssignmentDocumentsInmemoryStorage();
            this.localAssignmentsRepo.Store(this.LocalAssignments);

            var questionaries = new[]
            {
                Create.Entity.QuestionnaireIdentity(Id.gA),
                Create.Entity.QuestionnaireIdentity(Id.gB),
                Create.Entity.QuestionnaireIdentity(Id.gC)
            };

            var synchronizationService = Mock.Of<ISynchronizationService>(
                x => x.GetCensusQuestionnairesAsync(Moq.It.IsAny<CancellationToken>()) == Task.FromResult(new List<QuestionnaireIdentity>())
                     && x.GetServerQuestionnairesAsync(Moq.It.IsAny<CancellationToken>()) == Task.FromResult(new List<QuestionnaireIdentity>(questionaries))
                     && x.GetInterviewsAsync(Moq.It.IsAny<CancellationToken>()) == Task.FromResult(new List<InterviewApiView>())
                     && x.GetAttachmentContentsAsync(It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<Action<decimal, long, long>>(), Moq.It.IsAny<CancellationToken>()) == Task.FromResult(new List<string>())
                     && x.GetQuestionnaireAsync(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<Action<decimal, long, long>>(), Moq.It.IsAny<CancellationToken>()) == Task.FromResult(new QuestionnaireApiView())
                     && x.GetQuestionnaireTranslationAsync(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<CancellationToken>()) == Task.FromResult(new List<TranslationDto>())
                     && x.GetAssignmentsAsync(Moq.It.IsAny<CancellationToken>()) == Task.FromResult(this.RemoteAssignments)
            );

            var interviewViewRepository = new SqliteInmemoryStorage<InterviewView>();
            interviewViewRepository.Store(new List<InterviewView>());
            var questionnaireStorage = Mock.Of<IQuestionnaireStorage>(q => 
                q.GetQuestionnaire(questionaries[2], null) == CreatePlain(Id.gC, this.RemoteAssignments[1])
                && q.GetQuestionnaire(questionaries[0], null) == CreatePlain(Id.gA, this.RemoteAssignments[0]));

            var viewModel = Create.Service.AssignmentsSynchronizer(
                synchronizationService: synchronizationService,
                assignmentsRepository: this.localAssignmentsRepo,
                questionnaireStorage: questionnaireStorage
            );

            this.progressInfo = new Mock<IProgress<SyncProgressInfo>>();

            await viewModel.SynchronizeAssignmentsAsync(progressInfo.Object, new SychronizationStatistics(), CancellationToken.None);
        }

        [Test]
        public void should_add_new_assignment()
        {
            var newRemoteAssign = this.localAssignmentsRepo.GetById(3);
            Assert.NotNull(newRemoteAssign);
        }

        [Test]
        public void should_remove_removed_assignment()
        {
            var newRemoteAssign = this.localAssignmentsRepo.GetById(2);
            Assert.Null(newRemoteAssign);
        }

        [Test]
        public void should_update_existing_assignment_quantity()
        {
            var existingAssignment = this.localAssignmentsRepo.GetById(1);
            Assert.That(existingAssignment.Quantity, Is.EqualTo(this.RemoteAssignments[0].Quantity));
        }

        [Test]
        public void should_make_local_assignments_equal_to_remote()
        {
            var assignments = this.localAssignmentsRepo.LoadAll();

            Assert.That(assignments, Has.Count.EqualTo(this.RemoteAssignments.Count));

            var remoteLookup = this.RemoteAssignments.ToDictionary(x => x.Id);
            foreach (var local in assignments)
            {
                var remote = remoteLookup[local.Id];

                Assert.That(remote.Quantity, Is.EqualTo(local.Quantity));
                Assert.That(remote.InterviewsCount, Is.EqualTo(local.InterviewsCount));
                Assert.That(remote.QuestionnaireId.ToString(), Is.EqualTo(local.QuestionnaireId));
            }
        }

        [Test]
        public void should_count_new_assignments()
        {
            this.progressInfo.Verify(s => s.Report(It.Is<SyncProgressInfo>(p => p.Statistics.NewAssignmentsCount == 1)));
        }

        [Test]
        public void should_count_removed_assignments()
        {
            this.progressInfo.Verify(s => s.Report(It.Is<SyncProgressInfo>(p => p.Statistics.RemovedAssignmentsCount == 1)));
        }
    }
}