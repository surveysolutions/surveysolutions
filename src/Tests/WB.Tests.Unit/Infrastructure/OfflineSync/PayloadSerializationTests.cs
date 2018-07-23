using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Kernel;
using Ncqrs.Eventing.ServiceModel.Bus;
using NUnit.Framework;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services.Implementation;
using WB.Infrastructure.Native.Storage;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Infrastructure.OfflineSync
{
    [TestFixture]
    public class PayloadSerializationTests
    {
        [Test]
        public async Task can_serialize_and_deserialize_all_communication_messages()
        {
            var communicationMessageType = typeof(ICommunicationMessage);
            var messageTypes = Assembly.GetAssembly(communicationMessageType)
                .GetTypes().Where(t => communicationMessageType.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                .ToArray();

            var fixture = Create.Other.AutoFixture();

            fixture.Register<IPublishableEvent>(() =>
                Create.Other.CommittedEvent(payload: Create.Event.TextQuestionAnswered()));
            fixture.Register<WB.Core.Infrastructure.EventBus.IEvent>(() => Create.Event.TextQuestionAnswered());
            var serializer = new PayloadSerializer(new JsonAllTypesSerializer());

            var deserializeMethod = serializer.GetType().GetMethod(nameof(PayloadSerializer.FromPayloadAsync));
            if (deserializeMethod == null) throw new Exception();

            foreach (var messageType in messageTypes)
            {
                var message = new SpecimenContext(fixture).Resolve(messageType);
                var payload = await serializer.ToPayloadAsync(message);

                var action = deserializeMethod.MakeGenericMethod(messageType);

                Assert.DoesNotThrowAsync(async () =>
                    {
                        await (Task) action.Invoke(serializer, new object[] {payload});
                    },
                    $"Try to deserialize {messageType.Name} failed");
            }
        }
    }
}
