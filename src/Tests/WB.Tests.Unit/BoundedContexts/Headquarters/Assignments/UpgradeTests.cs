using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Assignments
{
    public class UpgradeTests
    {
        [Test]
        public void when_questionnaire_is_not_compatible_with_new_questionnaire_Should_not_migrate_it_and_report_an_error()
        {
            var migrateFrom = Create.Entity.QuestionnaireIdentity(Id.g1, 1);
            var migrateTo = Create.Entity.QuestionnaireIdentity(Id.g2, 2);
            var migratedAssignmentId = 45;
            var migratedAssignmentQuantity = 2;

            var assignmentsStorage = new TestPlainStorage<Assignment>();
            var assignmentToMigrate = Create.Entity.Assignment(id: migratedAssignmentId,
                quantity: migratedAssignmentQuantity,
                questionnaireIdentity: migrateFrom);

            assignmentsStorage.Store(assignmentToMigrate, migratedAssignmentId);
            Mock.Of<IInterviewImportService>(s =>
                s.VerifyAssignment(It.IsAny<List<InterviewAnswer>[]>(), It.IsAny<IQuestionnaire>()) ==
                AssignmentVerificationResult.Error("Generic error"));

            var service = Create.Service.AssignmentsUpgrader(assignments: assignmentsStorage);

            // Act
            service.Upgrade(migrateFrom, migrateTo);

            // Assert
            Assignment oldAssignment = assignmentsStorage.GetById(migratedAssignmentId);
            Assert.That(oldAssignment, Has.Property(nameof(oldAssignment.Archived)).EqualTo(false));
            Assert.That(oldAssignment, Has.Property(nameof(oldAssignment.QuestionnaireId)).EqualTo(migrateFrom));

            var newAssignment = assignmentsStorage.Query(_ => _.FirstOrDefault(x => x.Id != migratedAssignmentId));
            Assert.That(newAssignment, Is.Null);
        }

        [TestCase(4)]
        [TestCase(null)]
        public void when_assignment_exists_on_questionnaire_Should_migrate_it_to_new_questionnaire(int? quantity)
        {
            var migrateFrom = Create.Entity.QuestionnaireIdentity(Id.g1, 1);
            var migrateTo = Create.Entity.QuestionnaireIdentity(Id.g2, 2);
            var migratedAssignmentId = 45;
            var migratedAssignmentQuantity = quantity;

            var assignmentsStorage = new TestPlainStorage<Assignment>();
            var assignmentToMigrate = Create.Entity.Assignment(id: migratedAssignmentId,
                quantity: migratedAssignmentQuantity,
                questionnaireIdentity: migrateFrom);
            assignmentToMigrate.SetAnswers(new List<InterviewAnswer>
            {
                Create.Entity.InterviewAnswer(Create.Identity(), Create.Entity.TextQuestionAnswer("blabla"))
            });

            assignmentToMigrate.SetIdentifyingData(new List<IdentifyingAnswer>
            {
                Create.Entity.IdentifyingAnswer(identity: Create.Identity(), answer: "identifying")
            });

            assignmentsStorage.Store(assignmentToMigrate, migratedAssignmentId);

            var service = Create.Service.AssignmentsUpgrader(assignments: assignmentsStorage);

            // Act
            service.Upgrade(migrateFrom, migrateTo);

            // Assert
            Assignment oldAssignment = assignmentsStorage.GetById(migratedAssignmentId);
            Assert.That(oldAssignment, Has.Property(nameof(oldAssignment.Archived)).EqualTo(true), "Existing assignment on old questionnaire should be archived");
            Assert.That(oldAssignment, Has.Property(nameof(oldAssignment.QuestionnaireId)).EqualTo(migrateFrom));

            var newAssignment = assignmentsStorage.Query(_ => _.FirstOrDefault(x => x.Id != migratedAssignmentId));
            Assert.That(newAssignment, Is.Not.Null);
            Assert.That(newAssignment, Has.Property(nameof(newAssignment.Quantity)).EqualTo(migratedAssignmentQuantity));
            Assert.That(newAssignment, Has.Property(nameof(newAssignment.QuestionnaireId)).EqualTo(migrateTo));
            Assert.That(newAssignment.Answers, Is.EquivalentTo(assignmentToMigrate.Answers));
            Assert.That(newAssignment.IdentifyingData, Is.EquivalentTo(assignmentToMigrate.IdentifyingData));
        }

        [Test]
        public void should_not_migrate_completed_assignments()
        {
            var migrateFrom = Create.Entity.QuestionnaireIdentity(Id.g1, 1);
            var migrateTo = Create.Entity.QuestionnaireIdentity(Id.g2, 2);
            var assignmentId = 45;

            var assignmentsStorage = new TestPlainStorage<Assignment>();
            assignmentsStorage.Store(Create.Entity.Assignment(id: assignmentId, quantity: 0, questionnaireIdentity: migrateFrom),
                assignmentId);

            var service = Create.Service.AssignmentsUpgrader(assignments: assignmentsStorage);

            // Act
            service.Upgrade(migrateFrom, migrateTo);

            // Assert
            Assignment oldAssignment = assignmentsStorage.GetById(assignmentId);
            Assert.That(oldAssignment, Has.Property(nameof(oldAssignment.Archived)).EqualTo(false));

            var newAssignment = assignmentsStorage.Query(_ => _.FirstOrDefault(x => x.Id != assignmentId));
            Assert.That(newAssignment, Is.Null);
        }
    }
}
