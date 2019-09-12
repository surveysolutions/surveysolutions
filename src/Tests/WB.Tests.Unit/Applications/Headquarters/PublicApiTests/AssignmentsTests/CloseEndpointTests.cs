﻿using System.Net;
using System.Net.Http;
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
            var httpResponseMessage = this.controller.Close(14);
            Assert.That(httpResponseMessage, Has.Property(nameof(HttpResponseMessage.StatusCode)).EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public void should_return_409_for_web_enabled_assignment()
        {
            var assignment = Create.Entity.Assignment(id: 4, quantity: 5, webMode: true);
            this.SetupAssignment(assignment);

            var httpResponseMessage = this.controller.Close(assignment.Id);
            Assert.That(httpResponseMessage, Has.Property(nameof(HttpResponseMessage.StatusCode)).EqualTo(HttpStatusCode.Conflict));
        }

        [Test]
        public void should_return_409_for_archived_assignment()
        {
            var assignment = Create.Entity.Assignment(id: 4, quantity: 5);
            assignment.Archived = true;
            this.SetupAssignment(assignment);

            var httpResponseMessage = this.controller.Close(assignment.Id);
            Assert.That(httpResponseMessage, Has.Property(nameof(HttpResponseMessage.StatusCode)).EqualTo(HttpStatusCode.Conflict));
        }

        [Test]
        public void should_publish_command_update_quantity_and_return_200_for_closed_assignment()
        {
            var assignment = Create.Entity.Assignment(id: 4, quantity: 5);
            assignment.InterviewSummaries.Add(Create.Entity.InterviewSummary());
            this.SetupAssignment(assignment);
            
            var httpResponseMessage = this.controller.Close(assignment.Id);

            commandService.Verify(x => 
                x.Execute(It.Is<UpdateAssignmentQuantity>(c => c.Quantity == 1 && c.AssignmentId == assignment.PublicKey), null),
                Times.Once);
            Assert.That(httpResponseMessage, Has.Property(nameof(HttpResponseMessage.StatusCode)).EqualTo(HttpStatusCode.OK));
        }
    }
}
