using System.Net;
using System.Net.Http;
using System.Web.Http;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;
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

            IPlainStorageAccessor<Assignment> assignments = new InMemoryPlainStorageAccessor<Assignment>();
            assignments.Store(assignment, assignment.Id);
            
            var controller = Create.Controller.AssignmentsPublicApiController(assignmentsStorage: assignments);

            // Act
            var response = controller.AudioRecoding(15);

            // Assert
            Assert.That(response.Enabled, Is.False);
        }

        [Test]
        public void should_throw_404_when_assignment_archived()
        {
            var assignment = Create.Entity.Assignment(id: 15);

            IPlainStorageAccessor<Assignment> assignments = new InMemoryPlainStorageAccessor<Assignment>();
            assignments.Store(assignment, assignment.Id);
            assignment.Archive();
            
            var controller = Create.Controller.AssignmentsPublicApiController(assignmentsStorage: assignments);

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

            IPlainStorageAccessor<Assignment> assignments = new InMemoryPlainStorageAccessor<Assignment>();
            assignments.Store(assignment, assignment.Id);
            
            var controller = Create.Controller.AssignmentsPublicApiController(assignmentsStorage: assignments);

            // Act
            var response = controller.AudioRecodingPatch(15, new UpdateRecordingRequest{Enabled = true});

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(assignment.IsAudioRecordingEnabled, Is.True);
        }
    }
}
