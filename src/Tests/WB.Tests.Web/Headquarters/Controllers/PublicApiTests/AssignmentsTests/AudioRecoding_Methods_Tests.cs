using System;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.DenormalizerStorage;
using WB.Core.SharedKernels.DataCollection.Commands.Assignment;
using WB.Tests.Web;

using WB.UI.Headquarters.API.PublicApi.Models;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests.AssignmentsTests
{
    internal class AudioRecoding_Methods_Tests : ApiTestContext
    {
        [Test]
        public void should_return_assignment_audio_recording_settings()
        {
            var assignment = Abc.Create.Entity.Assignment(id: 15);

            var assignments = new InMemoryReadSideRepositoryAccessor<Assignment, Guid>();
            assignments.Store(assignment, assignment.PublicKey);

            var assignmentsService = Abc.Create.Service.AssignmentsService(assignments);

            var controller = Create.Controller.AssignmentsPublicApiController(assignmentsService: assignmentsService);

            // Act
            var response = controller.AudioRecoding(15);

            // Assert
            Assert.That(response.Value.Enabled, Is.False);
        }

        [Test]
        public void should_throw_404_when_assignment_archived()
        {
            var assignment = Abc.Create.Entity.Assignment(id: 15, isArchived: true);

            var assignments = new InMemoryReadSideRepositoryAccessor<Assignment, Guid>();
            assignments.Store(assignment, assignment.PublicKey);

            var assignmentsService = Abc.Create.Service.AssignmentsService(assignments);

            var controller = Create.Controller.AssignmentsPublicApiController(assignmentsService: assignmentsService);

            // Act
            var result = controller.AudioRecoding(15);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public void should_set_audio_recording_enabled()
        {
            var assignment = Abc.Create.Entity.Assignment(id: 15);

            var assignments = new InMemoryReadSideRepositoryAccessor<Assignment, Guid>();
            assignments.Store(assignment, assignment.PublicKey);
            
            var assignmentsService = Abc.Create.Service.AssignmentsService(assignments);
            var commandService = Mock.Of<ICommandService>();

            var controller = Create.Controller.AssignmentsPublicApiController(assignmentsService: assignmentsService, commandService: commandService);

            // Act
            var response = controller.AudioRecodingPatch(15, new UpdateRecordingRequest{Enabled = true});

            // Assert
            Assert.That(response, Is.InstanceOf<NoContentResult>());
            Mock.Get(commandService).Verify(c => c.Execute(It.Is<UpdateAssignmentAudioRecording>(a => a.PublicKey == assignment.PublicKey), null), Times.Once);
        }
    }
}
