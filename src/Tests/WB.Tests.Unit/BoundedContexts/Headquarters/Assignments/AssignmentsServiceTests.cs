using System;
using System.Collections.Generic;
using Moq;
using NHibernate;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Assignments
{
    [TestOf(typeof(AssignmentsService))]
    public class AssignmentsServiceTests
    {
        [Test]
        public void when_assignment_has_no_protected_variables()
        {
            var interviewerId = Id.g1;

            var assignmentService = Create.Service.AssignmentService(
                Create.Entity.Assignment(quantity: null, responsibleId: interviewerId, protectedVariables: null) );

            // Act
            bool hasAssignmentWithProtectedVariables = assignmentService.HasAssignmentWithProtectedVariables(interviewerId);

            Assert.That(hasAssignmentWithProtectedVariables, Is.False);
        }

        [Test]
        public void when_assignment_has_protected_variables()
        {
            var interviewerId = Id.g1;

            var assignmentService = Create.Service.AssignmentService(
                Create.Entity.Assignment(quantity: null, responsibleId: interviewerId, protectedVariables: null),
                Create.Entity.Assignment(quantity: null, responsibleId: interviewerId, protectedVariables: new List<string>{ "var1", "var2" }) );

            // Act
            bool hasAssignmentWithProtectedVariables = assignmentService.HasAssignmentWithProtectedVariables(interviewerId);

            Assert.That(hasAssignmentWithProtectedVariables, Is.True);
        }

        [Test]
        public void when_assignment_has_no_audio_audit_scope()
        {
            var interviewerId = Id.g1;

            var assignmentService = Create.Service.AssignmentService(
                Create.Entity.Assignment(quantity: null, responsibleId: interviewerId, audioAuditScope: null));

            // Act
            bool hasAssignmentWithAudioAuditScope = assignmentService.HasAssignmentWithAudioAuditScope(interviewerId);

            Assert.That(hasAssignmentWithAudioAuditScope, Is.False);
        }

        [Test]
        public void when_assignment_has_audio_audit_scope()
        {
            var interviewerId = Id.g1;

            var assignmentService = Create.Service.AssignmentService(
                Create.Entity.Assignment(quantity: null, responsibleId: interviewerId, audioAuditScope: null),
                Create.Entity.Assignment(quantity: null, responsibleId: interviewerId, audioAuditScope: new List<string> { "section1", "roster1" }));

            // Act
            bool hasAssignmentWithAudioAuditScope = assignmentService.HasAssignmentWithAudioAuditScope(interviewerId);

            Assert.That(hasAssignmentWithAudioAuditScope, Is.True);
        }

        [Test]
        public void when_supervisor_team_assignment_has_audio_audit_scope()
        {
            var supervisorId = Id.g1;
            var interviewerId = Id.g2;

            var assignmentService = Create.Service.AssignmentService(
                Create.Entity.Assignment(quantity: null, responsibleId: interviewerId, assigneeSupervisorId: supervisorId,
                    audioAuditScope: new List<string> { "section1" }));

            // Act
            bool hasAssignmentWithAudioAuditScope = assignmentService.HasAssignmentWithAudioAuditScopeForSupervisor(supervisorId);

            Assert.That(hasAssignmentWithAudioAuditScope, Is.True);
        }

        [Test]
        public void when_getting_assignment_with_upgrade_lock_returns_null_for_missing_assignment()
        {
            var assignmentService = Create.Service.AssignmentService();

            var result = assignmentService.GetAssignmentWithUpgradeLock(99);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void when_getting_assignment_with_upgrade_lock_acquires_row_level_lock()
        {
            var assignment = Create.Entity.Assignment(id: 5, quantity: 1, webMode: true);

            var mockSession = new Mock<ISession>();
            var mockUnitOfWork = Mock.Of<IUnitOfWork>(x => x.Session == mockSession.Object);

            var accessor = new TestInMemoryWriter<Assignment, Guid>();
            accessor.Store(assignment, assignment.PublicKey);

            var service = new AssignmentsService(accessor,
                Mock.Of<IInterviewAnswerSerializer>(),
                mockUnitOfWork,
                Mock.Of<IAuthorizedUser>());

            var result = service.GetAssignmentWithUpgradeLock(5);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(5));
            mockSession.Verify(s => s.Refresh(assignment, LockMode.Upgrade), Times.Once);
        }

        [Test]
        public void when_getting_assignment_with_upgrade_lock_by_entity_acquires_row_level_lock()
        {
            var assignment = Create.Entity.Assignment(id: 5, quantity: 1, webMode: true);

            var mockSession = new Mock<ISession>();
            var mockUnitOfWork = Mock.Of<IUnitOfWork>(x => x.Session == mockSession.Object);

            var service = new AssignmentsService(
                new TestInMemoryWriter<Assignment, Guid>(),
                Mock.Of<IInterviewAnswerSerializer>(),
                mockUnitOfWork,
                Mock.Of<IAuthorizedUser>());

            var result = service.GetAssignmentWithUpgradeLock(assignment);

            Assert.That(result, Is.SameAs(assignment));
            mockSession.Verify(s => s.Refresh(assignment, LockMode.Upgrade), Times.Once);
        }

        [Test]
        public void when_getting_assignment_with_upgrade_lock_by_null_entity_returns_null()
        {
            var service = Create.Service.AssignmentService();

            var result = service.GetAssignmentWithUpgradeLock(null as Assignment);

            Assert.That(result, Is.Null);
        }
    }
}
