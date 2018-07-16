using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services.Implementation;
using WB.Infrastructure.Native.Storage;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.OfflineSyncTests
{
    [TestOf(typeof(PostInterviewRequest))]
    public class PostInterviewRequestTests
    {
        [Test]
        public async Task should_be_able_to_serialize_and_deserialize_events()
        {
            var serializer = new PayloadSerializer(new JsonAllTypesSerializer());
            InterviewCreated interviewCreated = Create.Event.InterviewCreated();

            PostInterviewRequest request = new PostInterviewRequest(Id.gA, new List<CommittedEvent>
            {
                Create.Other.CommittedEvent(payload: interviewCreated, eventSourceId: Id.gA)
            });
            // Act
            byte[] payload = await serializer.ToPayloadAsync(request);
            Assert.That(payload, Is.Not.Empty);

            var deserialized = await serializer.FromPayloadAsync<PostInterviewRequest>(payload);

            Assert.That(deserialized.InterviewId, Is.EqualTo(Id.gA));
            Assert.That(deserialized.Events, Has.Count.EqualTo(1));
            Assert.That(deserialized.Events.First().Payload, Is.TypeOf<InterviewCreated>());
        }
    }
}
