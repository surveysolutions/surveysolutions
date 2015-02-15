using System;
using Machine.Specifications;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.Events.User;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.DataCollection.UserTests
{
    internal class when_user_linking_to_device : UserTestContext
    {
        Establish context = () =>
        {
            userAr = CreateUser();
            linkUserToDeviceCommand = Create.Command.LinkUserToDeviceCommand(userId, deviceId);
            eventContext = Create.EventContext();
        };

        Because of = () => 
            userAr.LinkUserToDevice(linkUserToDeviceCommand);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_UserLinkedToDevice_event = () =>
            eventContext.ShouldContainEvent<UserLinkedToDevice>();

        It should_raise_UserLinkedToDevice_event_with_DeviceId_specified = () =>
            eventContext.GetSingleEvent<UserLinkedToDevice>().DeviceId.ShouldEqual(deviceId);

        private static User userAr;
        private static LinkUserToDevice linkUserToDeviceCommand;
        private static EventContext eventContext;
        private static Guid userId = Guid.Parse("11111111111111111111111111111111");
        private static string deviceId = "aaaaaaaaaaaaaaaa";

    }
}