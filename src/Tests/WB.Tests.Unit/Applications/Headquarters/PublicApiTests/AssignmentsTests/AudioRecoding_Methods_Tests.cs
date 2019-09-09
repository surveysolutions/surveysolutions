﻿using System.Net;
using System.Net.Http;
using System.Web.Http;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.DenormalizerStorage;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Assignment;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.PublicApi.Models;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests.AssignmentsTests
{
    internal class AudioRecoding_Methods_Tests : ApiTestContext
    {
        [Test]
        public void should_return_assignment_audio_recording_settings()
        {
            var assignment = Create.Entity.Assignment(id: 15);

            var assignments = new InMemoryReadSideRepositoryAccessor<Assignment>();
            assignments.Store(assignment, assignment.PublicKey);

            var assignmentsService = Create.Service.AssignmentsService(assignments);

            var controller = Create.Controller.AssignmentsPublicApiController(assignmentsService: assignmentsService);

            // Act
            var response = controller.AudioRecoding(15);

            // Assert
            Assert.That(response.Enabled, Is.False);
        }

        [Test]
        public void should_throw_404_when_assignment_archived()
        {
            var assignment = Create.Entity.Assignment(id: 15, isArchived: true);

            var assignments = new InMemoryReadSideRepositoryAccessor<Assignment>();
            assignments.Store(assignment, assignment.PublicKey);

            var assignmentsService = Create.Service.AssignmentsService(assignments);

            var controller = Create.Controller.AssignmentsPublicApiController(assignmentsService: assignmentsService);

            // Act
            TestDelegate act = () => controller.AudioRecoding(15);

            // Assert
            Assert.That(act, Throws.Exception.InstanceOf<HttpResponseException>()
                                             .With.Property(nameof(HttpResponseException.Response))
                                             .With.Property(nameof(HttpResponseMessage.StatusCode)).EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public void should_set_audio_recording_enabled()
        {
            var assignment = Create.Entity.Assignment(id: 15);

            var assignments = new InMemoryReadSideRepositoryAccessor<Assignment>();
            assignments.Store(assignment, assignment.PublicKey);
            
            var assignmentsService = Create.Service.AssignmentsService(assignments);
            var commandService = Mock.Of<ICommandService>();

            var controller = Create.Controller.AssignmentsPublicApiController(assignmentsService: assignmentsService, commandService: commandService);

            // Act
            var response = controller.AudioRecodingPatch(15, new UpdateRecordingRequest{Enabled = true});

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Mock.Get(commandService).Verify(c => c.Execute(It.Is<UpdateAssignmentAudioRecording>(a => a.AssignmentId == assignment.PublicKey), null), Times.Once);
        }
    }
}
