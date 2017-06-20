using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Assignments
{
    [TestOf(typeof(AssignmentsService))]
    public class when_getting_assignments_for_web_interview
    {
        [Test]
        public void should_return_assignments_assigned_to_interviewer()
        {
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity();
            Guid supervisorId = Id.g1;

            var assignment1 = Create.Entity.Assignment(1, questionnaireIdentity, 1, assigneeSupervisorId: supervisorId);
            var assignment2 = Create.Entity.Assignment(2, questionnaireIdentity, 1, assigneeSupervisorId: supervisorId);
            var assignment3 = Create.Entity.Assignment(3, questionnaireIdentity, 1, assigneeSupervisorId: null);

            var service = Create.Service.AssignmentService(assignment1, assignment2, assignment3);

            // Act
           var assignmentsReadyForWebInterview = service.GetAssignmentsReadyForWebInterview(questionnaireIdentity);

            // Assert

            Assert.That(assignmentsReadyForWebInterview.Count, Is.EqualTo(2));
            Assert.That(assignmentsReadyForWebInterview.Select(x => x.Id), Is.EquivalentTo(new List<int>{ 1 , 2 }));

            Assert.That(service.GetCountOfAssignmentsReadyForWebInterview(questionnaireIdentity), Is.EqualTo(2));
        }
    }
}