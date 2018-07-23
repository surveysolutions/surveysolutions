using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests.AssignmentTests
{
    public class when_synchronize_assignments
    {
        private List<AssignmentDocument> localAssignments;
        private List<AssignmentApiDocument> remoteAssignments;
        private List<InterviewView> interviews;
        private IAssignmentDocumentsStorage localAssignmentsRepo;
        private Mock<IProgress<SyncProgressInfo>> progressInfo;

        private void Context()
        {
            localAssignments = new List<AssignmentDocument>
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

            remoteAssignments = new List<AssignmentApiDocument>
            {
                Create.Entity
                    .AssignmentApiDocument(1, 20, Create.Entity.QuestionnaireIdentity(Id.gA))
                    .WithAnswer(Create.Entity.Identity(Guid.NewGuid()), "1")
                    .WithAnswer(Create.Entity.Identity(Guid.NewGuid()), "2")
                    .WithAnswer(Create.Entity.Identity(Guid.NewGuid()), "3")
                    .WithAnswer(Create.Entity.Identity(Guid.NewGuid()), "4")
                    .WithAnswer(Create.Entity.Identity(Guid.NewGuid()), "gpsQuestion",latitude: 10.0, longtitude: 20.0)
                    .WithAnswer(Create.Entity.Identity(Guid.NewGuid()), "gpsnonIdent")
                    .Build(),
                Create.Entity
                    .AssignmentApiDocument(3, 25, Create.Entity.QuestionnaireIdentity(Id.gC))
                    .WithAnswer(Create.Entity.Identity(Guid.NewGuid()), "1")
                    .WithAnswer(Create.Entity.Identity(Guid.NewGuid()), "2")
                    .WithAnswer(Create.Entity.Identity(Guid.NewGuid()), "3")
                    .WithAnswer(Create.Entity.Identity(Id.gA), "gpsQuestion_1", latitude: 10.0, longtitude: 20.0)
                    .WithAnswer(Create.Entity.Identity(Id.gB), "gpsQuestion2_3")
                    .Build()
            };

            interviews = new List<InterviewView>
            {
                Create.Entity.InterviewView(status: InterviewStatus.InterviewerAssigned, assignmentId: 1, canBeDeleted: false),
                Create.Entity.InterviewView(status: InterviewStatus.InterviewerAssigned, assignmentId: 1, canBeDeleted: true),
                Create.Entity.InterviewView(status: InterviewStatus.Completed, assignmentId: 1, canBeDeleted: true),
                Create.Entity.InterviewView(status: InterviewStatus.RejectedBySupervisor, assignmentId: 1, canBeDeleted: false)
            };
        }

        PlainQuestionnaire CreatePlain(AssignmentApiDocument assignment)
        {
            var questionnaire = Create.Entity.QuestionnaireDocument(assignment.QuestionnaireId.QuestionnaireId, children: new IComposite[]
            {
                Create.Entity.TextQuestion(assignment.Answers[0].Identity.Id, text: "text 1"),
                Create.Entity.TextQuestion(assignment.Answers[1].Identity.Id, text: "title 2"),
                Create.Entity.TextQuestion(assignment.Answers[2].Identity.Id, text: "title 3", preFilled: true),
                Create.Entity.GpsCoordinateQuestion(assignment.Answers[3].Identity.Id, isPrefilled: true),
                Create.Entity.GpsCoordinateQuestion(assignment.Answers[4].Identity.Id)
            });

            questionnaire.Title = "title";
            return Create.Entity.PlainQuestionnaire(questionnaire);
        }

        private AssignmentApiView FromView(AssignmentApiDocument document)
        {
            return new AssignmentApiView
            {
                Id = document.Id,
                Quantity = document.Quantity,
                QuestionnaireId = document.QuestionnaireId
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

            var interviewViewRepository = new SqliteInmemoryStorage<InterviewView>();
            interviewViewRepository.Store(interviews);

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

            progressInfo = new Mock<IProgress<SyncProgressInfo>>();

            await viewModel.SynchronizeAssignmentsAsync(progressInfo.Object, new SynchronizationStatistics(), CancellationToken.None);
        }

        [Test]
        public void should_add_new_assignment()
        {
            var newRemoteAssign = localAssignmentsRepo.LoadAll();
            newRemoteAssign.Should().Contain(ad => ad.Id == 3);
        }

        [Test]
        public void should_fill_identifying_answers_without_gps_and_set_quantity()
        {
            var assignment = localAssignmentsRepo.LoadAll().First(ass => ass.Id == 3);

            assignment.IdentifyingAnswers.Should().HaveCount(1);
            assignment.IdentifyingAnswers.Should().NotContain(ia => ia.Identity.Id == Id.gA);

            Assert.That(assignment.Quantity, Is.EqualTo(remoteAssignments[1].Quantity));
            Assert.That(assignment.CreatedInterviewsCount, Is.EqualTo(0));
        }

        [Test]
        public void should_remove_removed_assignment()
        {
            var newRemoteAssign = localAssignmentsRepo.LoadAll().FirstOrDefault(ad => ad.Id == 2);
            newRemoteAssign.Should().BeNull();
        }

        [Test]
        public void should_update_existing_assignment_quantity()
        {
            var existingAssignment = localAssignmentsRepo.LoadAll().FirstOrDefault(ad => ad.Id == 1);
            Assert.That(existingAssignment.Quantity, Is.EqualTo(remoteAssignments[0].Quantity));
            Assert.That(existingAssignment.CreatedInterviewsCount, Is.EqualTo(1 /*InterviewerAssigned that can be deleted*/ 
                                                                               + 1 /* Completed that can be deleted too*/));
        }

        [Test]
        public void should_make_local_assignments_equal_to_remote()
        {
            var assignments = localAssignmentsRepo.LoadAll();

            Assert.That(assignments, Has.Count.EqualTo(remoteAssignments.Count));

            var remoteLookup = remoteAssignments.ToDictionary(x => x.Id);
            foreach (var local in assignments)
            {
                var remote = remoteLookup[local.Id];

                Assert.That(remote.Quantity, Is.EqualTo(local.Quantity));
                Assert.That(remote.QuestionnaireId.ToString(), Is.EqualTo(local.QuestionnaireId));
            }
        }

        [Test]
        public void should_count_new_assignments()
        {
            progressInfo.Verify(s => s.Report(It.Is<SyncProgressInfo>(p => p.Statistics.NewAssignmentsCount == 1)));
        }

        [Test]
        public void should_count_removed_assignments()
        {
            progressInfo.Verify(s => s.Report(It.Is<SyncProgressInfo>(p => p.Statistics.RemovedAssignmentsCount == 1)));
        }
    }
}
