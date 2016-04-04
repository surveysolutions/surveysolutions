using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.Events.User;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Tests.Unit.SharedKernels.DataCollection.UserTests
{
    internal class when_user_linking_to_device : UserTestContext
    {
        Establish context = () =>
        {
            Setup.InstanceToMockedServiceLocator(userDocumentStorage);
            var userDocument = Create.UserDocument(userId: userId, isArchived: false);
            userDocument.Roles.Add(UserRoles.Operator);
            userDocumentStorage.Store(userDocument, userId.FormatGuid());
            userAr = Create.User(userId);
            linkUserToDeviceCommand = Create.Command.LinkUserToDeviceCommand(userId, deviceId);
        };

        Because of = () => 
            userAr.LinkUserToDevice(linkUserToDeviceCommand);

        It should_link_one_device = () =>
            userDocumentStorage.GetById(userId.FormatGuid()).DeviceChangingHistory.Count.ShouldEqual(1);

        private static User userAr;
        private static LinkUserToDevice linkUserToDeviceCommand;
        private static Guid userId = Guid.Parse("11111111111111111111111111111111");
        private static string deviceId = "aaaaaaaaaaaaaaaa";
        private static IPlainStorageAccessor<UserDocument> userDocumentStorage = new TestPlainStorage<UserDocument>();
    }
}