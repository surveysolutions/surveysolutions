using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Commands.Assignment;
using WB.Tests.Abc;


namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests.AssignmentsTests
{
    public class CloseEndpointTests : BaseAssignmentsControllerTest
    {
        [Test]
        public void should_return_404_when_assignment_not_found()
        {
            var httpResponseMessage = this.controller.ClosePost(14);
            Assert.That(httpResponseMessage, Has.Property(nameof(HttpResponseMessage.StatusCode)).EqualTo(StatusCodes.Status404NotFound));
        }

        [Test]
        public void should_return_409_for_web_enabled_assignment()
        {
            var assignment = Create.Entity.Assignment(id: 4, quantity: 5, webMode: true);
            this.SetupAssignment(assignment);

            var httpResponseMessage = this.controller.ClosePost(assignment.Id);
            Assert.That(httpResponseMessage, Has.Property(nameof(HttpResponseMessage.StatusCode)).EqualTo(StatusCodes.Status409Conflict));
        }

        [Test]
        public void should_return_409_for_archived_assignment()
        {
            var assignment = Create.Entity.Assignment(id: 4, quantity: 5);
            assignment.Archived = true;
            this.SetupAssignment(assignment);

            var httpResponseMessage = this.controller.ClosePost(assignment.Id);
            Assert.That(httpResponseMessage, Has.Property(nameof(HttpResponseMessage.StatusCode)).EqualTo(StatusCodes.Status409Conflict));
        }

        [Test]
        public void should_publish_command_update_quantity_and_return_200_for_closed_assignment()
        {
            var assignment = Create.Entity.Assignment(id: 4, quantity: 5);
            assignment.InterviewSummaries.Add(Create.Entity.InterviewSummary());
            this.SetupAssignment(assignment);
            
            var httpResponseMessage = this.controller.ClosePost(assignment.Id);

            commandService.Verify(x => 
                x.Execute(It.Is<UpdateAssignmentQuantity>(c => c.Quantity == 1 && c.PublicKey == assignment.PublicKey), null),
                Times.Once);
            Assert.That(httpResponseMessage, Has.Property(nameof(HttpResponseMessage.StatusCode)).EqualTo(StatusCodes.Status200OK));
        }
    }
}
