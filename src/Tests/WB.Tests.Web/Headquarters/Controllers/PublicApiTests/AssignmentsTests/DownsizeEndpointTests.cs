using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Commands.Assignment;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.PublicApi.Models;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests.AssignmentsTests
{
    public class DownsizeEndpointTests : BaseAssignmentsControllerTest
    {
        [Test]
        public void should_return_404_when_assignment_not_found()
        {
            var result = this.controller.Downsize(14).Result;
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public void should_return_409_for_web_enabled_assignment()
        {
            var assignment = Create.Entity.Assignment(id: 4, quantity: 5, webMode: true);
            this.SetupAssignment(assignment);

            var result = this.controller.Downsize(assignment.Id).Result;
            Assert.That(result, Is.TypeOf<ConflictResult>());
        }

        [Test]
        public void should_return_409_for_archived_assignment()
        {
            var assignment = Create.Entity.Assignment(id: 4, quantity: 5);
            assignment.Archived = true;
            this.SetupAssignment(assignment);

            var result = this.controller.Downsize(assignment.Id).Result;
            Assert.That(result, Is.TypeOf<ConflictResult>());
        }

        [Test]
        public void should_publish_command_update_quantity_and_return_updated_assignment()
        {
            var assignment = Create.Entity.Assignment(id: 4, quantity: 5);
            assignment.InterviewSummaries.Add(Create.Entity.InterviewSummary());
            this.SetupAssignment(assignment);
            
            var result = this.controller.Downsize(assignment.Id).Result;

            commandService.Verify(x => 
                x.Execute(It.Is<UpdateAssignmentQuantity>(c => c.Quantity == 1 && c.PublicKey == assignment.PublicKey), null),
                Times.Once);
            Assert.That(result, Is.InstanceOf<AssignmentDetails>());
        }

        [Test]
        public void should_log_assignment_size_changed_in_audit_log()
        {
            var assignment = Create.Entity.Assignment(id: 4, quantity: 5);
            assignment.InterviewSummaries.Add(Create.Entity.InterviewSummary());
            this.SetupAssignment(assignment);

            this.controller.Downsize(assignment.Id);

            this.auditLog.Verify(x => x.AssignmentSizeChanged(assignment.Id, 1), Times.Once);
        }
    }
}
