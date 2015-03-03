using System;

using Machine.Specifications;
using Moq;

using NUnit.Framework;

using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.Synchronization.Documents;
using WB.Core.Synchronization.Implementation.SyncManager;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Core.Synchronization
{
    internal class when_linking_user_to_device_and_device_is_registered : SyncManagerTestContext
    {
        Establish context = () =>
        {
            tabletDocument = CreateTabletDocument(deviceId, androidId);
            devices = Mock.Of<IReadSideRepositoryReader<TabletDocument>>(x => x.GetById(deviceId.FormatGuid()) == tabletDocument);

            commandServiceMock = new Mock<ICommandService>();

            commandServiceMock
                .Setup(x => x.Execute(Moq.It.IsAny<LinkUserToDevice>(), Moq.It.IsAny<string>()))
                .Callback((ICommand command, string origin) => linkUserToDevice = command as LinkUserToDevice);

            syncManager = CreateSyncManager(commandService: commandServiceMock.Object, devices: devices);
        };

        Because of = () =>
            syncManager.LinkUserToDevice(userId, androidId, appVersion, oldDeviceId);

        It should_send_RegisterTabletCommand = () =>
            linkUserToDevice.ShouldNotBeNull();

        It should_send_RegisterTabletCommand_DeviceId_specified = () =>
            linkUserToDevice.DeviceId.ShouldEqual(androidId);

        It should_send_RegisterTabletCommand_UserId_specified = () =>
            linkUserToDevice.Id.ShouldEqual(userId);

        private static LinkUserToDevice linkUserToDevice;
        private static TabletDocument tabletDocument;
        private static SyncManager syncManager;
        private static Mock<ICommandService> commandServiceMock;
        private static IReadSideRepositoryReader<TabletDocument> devices;
        private static Guid userId = Guid.Parse("11111111111111111111111111111111");
        private static string androidId = "Android";
        private static string oldDeviceId = "OldAndroid";
        private static Guid deviceId = androidId.ToGuid();
        private static string appVersion = "6002";
    }
}