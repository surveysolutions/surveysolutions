using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Main.Core.Entities.Composite;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Upgrade;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.DenormalizerStorage;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Assignment;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Assignments
{
    [TestOf(typeof(AssignmentsUpgrader))]
    public class AssignmentsUpgraderTests
    {
        [Test]
        public void should_not_upgrade_archived_assignments()
        {
            var migrateFrom = Create.Entity.QuestionnaireIdentity(Id.g1, 1);
            var migrateTo = Create.Entity.QuestionnaireIdentity(Id.g2, 2);
            var migratedAssignmentId = 45;
            var migratedAssignmentQuantity = 2;

            var assignmentsStorage = new InMemoryReadSideRepositoryAccessor<Assignment, Guid>();
            var assignmentToMigrate = Create.Entity.Assignment(id: migratedAssignmentId,
                publicKey: Id.g7,
                quantity: migratedAssignmentQuantity,
                questionnaireIdentity: migrateFrom);
            assignmentToMigrate.Archived = true;

            assignmentsStorage.Store(assignmentToMigrate, Id.g7);
            var importService = Mock.Of<IPreloadedDataVerifier>(s =>
                s.VerifyWithInterviewTree(It.IsAny<List<InterviewAnswer>>(), It.IsAny<Guid?>(), It.IsAny<IQuestionnaire>()) ==
                new InterviewImportError("Some code", "Generic error"));

            var assignmentsService = Create.Service.AssignmentsService(assignmentsStorage);
            var service = Create.Service.AssignmentsUpgrader(assignments: assignmentsService,
                importService: importService);

            // Act
            service.Upgrade(new AssignmentsUpgradeProcess(new Guid(), Guid.NewGuid(), migrateFrom, migrateTo));

            // Assert
            Assignment oldAssignment = assignmentsStorage.GetById(Id.g7);
            Assert.That(oldAssignment, Has.Property(nameof(oldAssignment.Archived)).EqualTo(true));
            Assert.That(oldAssignment, Has.Property(nameof(oldAssignment.QuestionnaireId)).EqualTo(migrateFrom));

            var newAssignment = assignmentsStorage.Query(_ => _.FirstOrDefault(x => x.Id != migratedAssignmentId));
            Assert.That(newAssignment, Is.Null);
        }

        [Test]
        public void when_questionnaire_is_not_compatible_with_new_questionnaire_Should_not_migrate_it_and_report_an_error()
        {
            var migrateFrom = Create.Entity.QuestionnaireIdentity(Id.g1, 1);
            var migrateTo = Create.Entity.QuestionnaireIdentity(Id.g2, 2);
            var migratedAssignmentId = 45;
            var migratedAssignmentQuantity = 2;

            var assignmentsStorage = new InMemoryReadSideRepositoryAccessor<Assignment, Guid>();
            var assignmentToMigrate = Create.Entity.Assignment(id: migratedAssignmentId,
                publicKey: Id.g7,
                quantity: migratedAssignmentQuantity,
                questionnaireIdentity: migrateFrom);

            assignmentsStorage.Store(assignmentToMigrate, assignmentToMigrate.PublicKey);
            var importService = Mock.Of<IPreloadedDataVerifier>(s =>
                s.VerifyWithInterviewTree(It.IsAny<List<InterviewAnswer>>(), It.IsAny<Guid?>(), It.IsAny<IQuestionnaire>()) ==
                new InterviewImportError("Some code", "Generic error"));

            var assignmentsService = Create.Service.AssignmentsService(assignmentsStorage);
            var service = Create.Service.AssignmentsUpgrader(assignments: assignmentsService,
                importService: importService);

            // Act
            service.Upgrade(new AssignmentsUpgradeProcess(new Guid(), Guid.NewGuid(), migrateFrom, migrateTo));

            // Assert
            Assignment oldAssignment = assignmentsStorage.GetById(Id.g7);
            Assert.That(oldAssignment, Has.Property(nameof(oldAssignment.Archived)).EqualTo(false));
            Assert.That(oldAssignment, Has.Property(nameof(oldAssignment.QuestionnaireId)).EqualTo(migrateFrom));

            var newAssignment = assignmentsStorage.Query(_ => _.FirstOrDefault(x => x.Id != migratedAssignmentId));
            Assert.That(newAssignment, Is.Null);
        }

        [Test]
        public void when_some_interviews_are_already_collected_Should_create_assignment_with_reduced_quantity()
        {
            var migrateFrom = Create.Entity.QuestionnaireIdentity(Id.g1, 1);
            var migrateTo = Create.Entity.QuestionnaireIdentity(Id.g2, 2);
            var migratedAssignmentId = 45;
            var migratedAssignmentQuantity = 3;

            var assignmentsStorage = new InMemoryReadSideRepositoryAccessor<Assignment, Guid>();
            var assignmentToMigrate = Create.Entity.Assignment(id: migratedAssignmentId,
                publicKey: Id.g7,
                quantity: migratedAssignmentQuantity,
                questionnaireIdentity: migrateFrom);
            assignmentToMigrate.InterviewSummaries.Add(Create.Entity.InterviewSummary());

            assignmentsStorage.Store(assignmentToMigrate, Id.g7);

            var questionnaires = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(migrateTo.QuestionnaireId,
                questionnaireVersion: migrateTo.Version
                );
            var commandService = new Mock<ICommandService>();
            commandService.Setup(cs => cs.Execute(It.IsAny<CreateAssignment>(), null))
                .Callback<ICommand, string>((commandArgs, origin) =>
                {
                    var c = (CreateAssignment) commandArgs;
                    var assignment = Create.Entity.Assignment(publicKey: c.PublicKey, id: c.Id, quantity: c.Quantity, questionnaireIdentity: c.QuestionnaireId);
                    assignmentsStorage.Store(assignment, c.PublicKey);
                });
            commandService.Setup(cs => cs.Execute(It.IsAny<UpgradeAssignmentCommand>(), null))
                .Callback<ICommand, string>((commandArgs, origin) =>
                {
                    var c = (UpgradeAssignmentCommand) commandArgs;
                    assignmentsStorage.GetById(c.PublicKey).Archived = true;
                });
            var assignmentsService = Create.Service.AssignmentsService(assignmentsStorage);
            var service = Create.Service.AssignmentsUpgrader(assignments: assignmentsService, questionnaireStorage: questionnaires,
                commandService: commandService.Object);

            // Act
            service.Upgrade(new AssignmentsUpgradeProcess(new Guid(), Guid.NewGuid(), migrateFrom, migrateTo));

            // Assert
            Assignment oldAssignment = assignmentsStorage.GetById(Id.g7);

            Assert.That(oldAssignment, Has.Property(nameof(oldAssignment.Archived)).EqualTo(true), "Existing assignment on old questionnaire should be archived");
            Assert.That(oldAssignment, Has.Property(nameof(oldAssignment.QuestionnaireId)).EqualTo(migrateFrom));

            var newAssignment = assignmentsStorage.Query(_ => _.FirstOrDefault(x => x.Id != migratedAssignmentId));
            Assert.That(newAssignment, Is.Not.Null);
            Assert.That(newAssignment, Has.Property(nameof(newAssignment.Quantity)).EqualTo(migratedAssignmentQuantity - 1));
            Assert.That(newAssignment, Has.Property(nameof(newAssignment.QuestionnaireId)).EqualTo(migrateTo));
        }

        [TestCase(4)]
        [TestCase(null)]
        public void when_assignment_exists_on_questionnaire_Should_migrate_it_to_new_questionnaire(int? quantity)
        {
            var migrateFrom = Create.Entity.QuestionnaireIdentity(Id.g1, 1);
            var migrateTo = Create.Entity.QuestionnaireIdentity(Id.g2, 2);
            var migratedAssignmentId = 45;
            var migratedAssignmentQuantity = quantity;

            var assignmentsStorage = new InMemoryReadSideRepositoryAccessor<Assignment, Guid>();
            var assignmentToMigrate = Create.Entity.Assignment(id: migratedAssignmentId,
                quantity: migratedAssignmentQuantity,
                questionnaireIdentity: migrateFrom);
            var interviewAnswerId = Create.Identity();
            var protectedVariableName = "pro";
            assignmentToMigrate.Answers = new List<InterviewAnswer>
            {
                Create.Entity.InterviewAnswer(interviewAnswerId, Create.Entity.TextQuestionAnswer("blabla"))
            };

            var prefilledInterviewAnswerId = Create.Identity();
            assignmentToMigrate.IdentifyingData = new List<IdentifyingAnswer>
            {
                Create.Entity.IdentifyingAnswer(identity: prefilledInterviewAnswerId, answer: "identifying")
            };

            assignmentToMigrate.ProtectedVariables = new List<string>()
            {
                protectedVariableName
            };
            assignmentToMigrate.AudioRecording = true;

            assignmentsStorage.Store(assignmentToMigrate, assignmentToMigrate.PublicKey);

            var questionnaires = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(migrateTo.QuestionnaireId,
                questionnaireVersion: migrateTo.Version,
                questionnaire: Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocument(
                    children: new IComposite[]
                    {
                        Create.Entity.TextQuestion(questionId: interviewAnswerId.Id),
                        Create.Entity.TextQuestion(questionId: prefilledInterviewAnswerId.Id, preFilled: true)
                    }
                )));
            var commandService = new Mock<ICommandService>();
            commandService.Setup(cs => cs.Execute(It.IsAny<CreateAssignment>(), null))
                .Callback<ICommand, string>((commandArgs, origin) =>
                {
                    var c = (CreateAssignment)commandArgs;
                    var assignment = Create.Entity.Assignment(publicKey: c.PublicKey, 
                        id: c.Id, 
                        quantity: c.Quantity, 
                        questionnaireIdentity: c.QuestionnaireId,
                        protectedVariables: c.ProtectedVariables,
                        answers: c.Answers,
                        isAudioRecordingEnabled: c.AudioRecording,
                        identifyingAnswers: assignmentToMigrate.IdentifyingData.ToList()
                        );
                    assignmentsStorage.Store(assignment, c.PublicKey);
                });
            commandService.Setup(cs => cs.Execute(It.IsAny<UpgradeAssignmentCommand>(), null))
                .Callback<ICommand, string>((commandArgs, origin) =>
                {
                    var c = (UpgradeAssignmentCommand)commandArgs;
                    assignmentsStorage.GetById(c.PublicKey).Archived = true;
                });


            var assignmentsService = Create.Service.AssignmentsService(assignmentsStorage);
            var service = Create.Service.AssignmentsUpgrader(assignments: assignmentsService, questionnaireStorage: questionnaires,
                commandService: commandService.Object);

            // Act
            service.Upgrade(new AssignmentsUpgradeProcess(new Guid(), Guid.NewGuid(), migrateFrom, migrateTo));

            // Assert
            Assignment oldAssignment = assignmentsStorage.GetById(assignmentToMigrate.PublicKey);
            Assert.That(oldAssignment, Has.Property(nameof(oldAssignment.Archived)).EqualTo(true), "Existing assignment on old questionnaire should be archived");
            Assert.That(oldAssignment, Has.Property(nameof(oldAssignment.QuestionnaireId)).EqualTo(migrateFrom));

            var newAssignment = assignmentsStorage.Query(_ => _.FirstOrDefault(x => x.Id != migratedAssignmentId));
            Assert.That(newAssignment, Is.Not.Null);
            Assert.That(newAssignment, Has.Property(nameof(newAssignment.Quantity)).EqualTo(migratedAssignmentQuantity));
            Assert.That(newAssignment, Has.Property(nameof(newAssignment.QuestionnaireId)).EqualTo(migrateTo));
            Assert.That(newAssignment.Answers, Is.EquivalentTo(assignmentToMigrate.Answers));
            Assert.That(newAssignment.IdentifyingData, Has.Count.EqualTo(1));
            Assert.That(newAssignment.ProtectedVariables, Has.Count.EqualTo(1));
            Assert.That(newAssignment.AudioRecording, Is.True);
        }

        [Test]
        public void should_not_migrate_completed_assignments()
        {
            var migrateFrom = Create.Entity.QuestionnaireIdentity(Id.g1, 1);
            var migrateTo = Create.Entity.QuestionnaireIdentity(Id.g2, 2);
            var assignmentId = 45;

            var assignmentsStorage = new InMemoryReadSideRepositoryAccessor<Assignment, Guid>();
            assignmentsStorage.Store(Create.Entity.Assignment(id: assignmentId, publicKey: Id.g7, quantity: 0, questionnaireIdentity: migrateFrom),
                Id.g7);

            var assignmentsService = Create.Service.AssignmentsService(assignmentsStorage);
            var service = Create.Service.AssignmentsUpgrader(assignments: assignmentsService);

            // Act
            service.Upgrade(new AssignmentsUpgradeProcess(new Guid(), Guid.NewGuid(), migrateFrom, migrateTo));

            // Assert
            Assignment oldAssignment = assignmentsStorage.GetById(Id.g7);
            Assert.That(oldAssignment, Has.Property(nameof(oldAssignment.Archived)).EqualTo(false));

            var newAssignment = assignmentsStorage.Query(_ => _.FirstOrDefault(x => x.Id != assignmentId));
            Assert.That(newAssignment, Is.Null);
        }

        [Test]
        public void should_mark_cancelled_process_as_cancelled()
        {
            var migrateFrom = Create.Entity.QuestionnaireIdentity(Id.g1, 1);
            var migrateTo = Create.Entity.QuestionnaireIdentity(Id.g2, 2);

            var assignmentId = 45;
            var assignmentsStorage = new InMemoryReadSideRepositoryAccessor<Assignment, Guid>();
            assignmentsStorage.Store(Create.Entity.Assignment(id: assignmentId, publicKey: Id.g7, quantity: 0, questionnaireIdentity: migrateFrom),
                Id.g7);

            var upgradeServiceMock = new Mock<IAssignmentsUpgradeService>();

            var assignmentsService = Create.Service.AssignmentsService(assignmentsStorage);
            var service = Create.Service.AssignmentsUpgrader(assignments: assignmentsService, upgradeService: upgradeServiceMock.Object);
            var processId = Id.g1;
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            cancellationTokenSource.Cancel();

            // Act
            service.Upgrade(new AssignmentsUpgradeProcess(processId, Guid.NewGuid(), migrateFrom, migrateTo), cancellationToken);

            // Assert
            upgradeServiceMock.Verify(x => x.ReportProgress(processId, It.Is<AssignmentUpgradeProgressDetails>(
                p => p.Status == AssignmentUpgradeStatus.Cancelled
                )));
        }
    }
}
