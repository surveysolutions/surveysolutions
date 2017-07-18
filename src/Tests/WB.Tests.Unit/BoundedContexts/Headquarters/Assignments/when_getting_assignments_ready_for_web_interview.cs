using System;
using FluentAssertions;
using NUnit.Framework;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Assignments
{
    class when_getting_assignments_ready_for_web_interview : AssignmentsServiceTests
    {
        [Test]
        public void should_not_include_archived_and_completed_assignments()
        {
            Guid supervisorId = Id.g1;
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity();

            var archivedAssignment = Create.Entity.Assignment(id: 1, questionnaireIdentity: questionnaireIdentity, quantity: 5, assigneeSupervisorId: supervisorId);
            archivedAssignment.Archive();
            var completedAssignment = Create.Entity.Assignment(id: 2, questionnaireIdentity: questionnaireIdentity, quantity: 1, assigneeSupervisorId: supervisorId);
            completedAssignment.InterviewSummaries.Add(Create.Entity.InterviewSummary());
            var notCompletedAssignmentWithDeletedInterview = Create.Entity.Assignment(id: 3, questionnaireIdentity: questionnaireIdentity, quantity: 1, assigneeSupervisorId: supervisorId);
            notCompletedAssignmentWithDeletedInterview.InterviewSummaries.Add(Create.Entity.InterviewSummary(isDeleted: true));

            var service = Create.Service.AssignmentService(archivedAssignment, completedAssignment, notCompletedAssignmentWithDeletedInterview);

            // Act
            var assignmentsReadyForWebInterview = service.GetAssignmentsReadyForWebInterview(questionnaireIdentity);
            var countOfAssignments = service.GetCountOfAssignmentsReadyForWebInterview(questionnaireIdentity);

            // Assert
            assignmentsReadyForWebInterview.Should().NotContain(x => x.Id == 1, "Should not return archived assignment");
            assignmentsReadyForWebInterview.Should().NotContain(x => x.Id == 2, "Should not return completed assignment");
            assignmentsReadyForWebInterview.Should().Contain(x => x.Id == 3, "Should return not completed assignment with deleted interview");
            assignmentsReadyForWebInterview.Should().NotContain(x => x.Id == 4, "Should return not completed assignment");

            Assert.That(assignmentsReadyForWebInterview, Has.Count.EqualTo(countOfAssignments), "Number of returned assignments should be the same as returned count");
        }
    }
}