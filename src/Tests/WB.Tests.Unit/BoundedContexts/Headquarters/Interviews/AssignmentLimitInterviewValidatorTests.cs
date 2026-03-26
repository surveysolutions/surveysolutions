using System;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Interview.Validators;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.Services;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Interviews;

[TestOf(typeof(AssignmentLimitInterviewValidator))]
public class AssignmentLimitInterviewValidatorTests
{ 
    [Test]
    public void should_not_allow_interview_creation_if_assignment_limit_reached()
    {
        var assignmentService = Create.Service.AssignmentService(
            Create.Entity.Assignment(id: 3, quantity: 5,
                webMode: true, interviewSummary: new InterviewSummary[]
                {
                    Create.Entity.InterviewSummary(Guid.NewGuid()),
                    Create.Entity.InterviewSummary(Guid.NewGuid()),
                    Create.Entity.InterviewSummary(Guid.NewGuid()),
                    Create.Entity.InterviewSummary(Guid.NewGuid()),
                    Create.Entity.InterviewSummary(Guid.NewGuid()),
                })
            );

        var validator = Create.Service.AssignmentLimitInterviewValidator(assignmentService);

        TestDelegate act = () => validator.Validate(null, Create.Command.CreateInterview(userId: Id.gA, assignmentId: 3, interviewMode: InterviewMode.CAWI));

        Assert.That(act, Throws.Exception.InstanceOf<InterviewException>()
            .With.Message.EqualTo(CommandValidatorsMessages.AssignmentLimitReached)
            .And.Property(nameof(InterviewException.ExceptionType))
            .EqualTo(InterviewDomainExceptionType.AssignmentLimitReached));
    }
    
    [Test]
    public void should_not_allow_answer_temporally_interview_if_assignment_limit_reached()
    {
        var assignmentService = Create.Service.AssignmentService(
            Create.Entity.Assignment(id: 3, quantity: 5,
                webMode: true, interviewSummary: new InterviewSummary[]
                {
                    Create.Entity.InterviewSummary(Guid.NewGuid()),
                    Create.Entity.InterviewSummary(Guid.NewGuid()),
                    Create.Entity.InterviewSummary(Guid.NewGuid()),
                    Create.Entity.InterviewSummary(Guid.NewGuid()),
                    Create.Entity.InterviewSummary(Guid.NewGuid()),
                })
            );
        

        var validator = Create.Service.AssignmentLimitInterviewValidator(assignmentService);

        var statefulInterview = Create.AggregateRoot.StatefulInterview(Id.g7);
        statefulInterview.Apply(Create.Event.InterviewCreated(assignmentId: 3));
        statefulInterview.ChangeInterviewMode(Id.g1, DateTimeOffset.Now, InterviewMode.CAWI);

        TestDelegate act = () => validator.Validate(statefulInterview, 
            Create.Command.AnswerTextQuestionCommand(userId: Id.gA, interviewId: Id.g7, answer: "ttt" ));

        Assert.That(act, Throws.Exception.InstanceOf<InterviewException>()
            .With.Message.EqualTo(CommandValidatorsMessages.AssignmentLimitReached)
            .And.Property(nameof(InterviewException.ExceptionType))
            .EqualTo(InterviewDomainExceptionType.AssignmentLimitReached));
    }

    [Test]
    public void when_one_slot_left_should_acquire_upgrade_lock_and_allow_creation()
    {
        // Assignment with 1 remaining slot: InterviewsNeeded == 1 (quantity=1, 0 interviews done)
        var assignment = Create.Entity.Assignment(id: 7, quantity: 1, webMode: true);

        var mockAssignmentsService = new Mock<IAssignmentsService>();
        mockAssignmentsService.Setup(s => s.GetAssignment(7)).Returns(assignment);
        mockAssignmentsService.Setup(s => s.GetAssignmentWithUpgradeLock(7)).Returns(assignment);

        var validator = Create.Service.AssignmentLimitInterviewValidator(
            mockAssignmentsService.Object);

        // Act — should not throw because there is still 1 slot left after the lock is acquired
        Assert.DoesNotThrow(() =>
            validator.Validate(null, Create.Command.CreateInterview(userId: Id.gA, assignmentId: 7, interviewMode: InterviewMode.CAWI)));

        mockAssignmentsService.Verify(s => s.GetAssignmentWithUpgradeLock(7), Times.Once,
            "Upgrade lock must be acquired when InterviewsNeeded <= 3");
    }

    [Test]
    public void when_many_slots_left_should_not_acquire_upgrade_lock()
    {
        // Assignment with 10 remaining slots: InterviewsNeeded == 10 (> 3, lock should be skipped)
        var assignment = Create.Entity.Assignment(id: 8, quantity: 10, webMode: true);

        var mockAssignmentsService = new Mock<IAssignmentsService>();
        mockAssignmentsService.Setup(s => s.GetAssignment(8)).Returns(assignment);

        var validator = Create.Service.AssignmentLimitInterviewValidator(
            mockAssignmentsService.Object);

        // Act — should not throw
        Assert.DoesNotThrow(() =>
            validator.Validate(null, Create.Command.CreateInterview(userId: Id.gA, assignmentId: 8, interviewMode: InterviewMode.CAWI)));

        mockAssignmentsService.Verify(s => s.GetAssignmentWithUpgradeLock(It.IsAny<int>()), Times.Never,
            "Upgrade lock must not be acquired when InterviewsNeeded > 3");
    }

    [Test]
    public void when_one_slot_left_and_lock_reveals_slot_taken_should_throw()
    {
        // Single assignment instance to mimic NHibernate 1st-level cache behavior.
        // Pre-lock read: 1 slot remaining (race condition scenario).
        var assignment = Create.Entity.Assignment(
            id: 9,
            quantity: 1,
            webMode: true,
            interviewSummary: Array.Empty<InterviewSummary>());

        var mockAssignmentsService = new Mock<IAssignmentsService>();

        // First read (without lock) returns the assignment with 1 slot remaining.
        mockAssignmentsService.Setup(s => s.GetAssignment(9)).Returns(assignment);

        // Second read (with upgrade lock) returns the same instance, but with the last slot taken.
        mockAssignmentsService.Setup(s => s.GetAssignmentWithUpgradeLock(9))
            .Returns(() =>
            {
                assignment.InterviewSummaries.Clear();
                assignment.InterviewSummaries.Add(Create.Entity.InterviewSummary(Guid.NewGuid()));
                return assignment;
            });
        var validator = Create.Service.AssignmentLimitInterviewValidator(
            mockAssignmentsService.Object);

        TestDelegate act = () =>
            validator.Validate(null, Create.Command.CreateInterview(userId: Id.gA, assignmentId: 9, interviewMode: InterviewMode.CAWI));

        Assert.That(act, Throws.Exception.InstanceOf<InterviewException>()
            .With.Message.EqualTo(CommandValidatorsMessages.AssignmentLimitReached)
            .And.Property(nameof(InterviewException.ExceptionType))
            .EqualTo(InterviewDomainExceptionType.AssignmentLimitReached));
    }
}
