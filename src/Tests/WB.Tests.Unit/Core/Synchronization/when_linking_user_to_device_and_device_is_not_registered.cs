using System;

using Machine.Specifications;

using Moq;

using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Synchronization.Commands;
using WB.Core.Synchronization.Documents;
using WB.Core.Synchronization.Implementation.SyncManager;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Core.Synchronization
{
    internal class when_linking_user_to_device_and_device_is_not_registered : SyncManagerTestContext
    {
        Establish context = () =>
        {
            devices = Mock.Of<IReadSideRepositoryReader<TabletDocument>>();

            commandServiceMock = new Mock<ICommandService>();

            commandServiceMock
                .Setup(x => x.Execute(Moq.It.IsAny<RegisterTabletCommand>(), Moq.It.IsAny<string>()))
                .Callback((ICommand command, string origin) => registerTabletCommand = command as RegisterTabletCommand);

            syncManager = CreateSyncManager(commandService: commandServiceMock.Object, devices: devices);
        };

        Because of = () =>
            syncManager.LinkUserToDevice(userId, androidId, appVersion, oldDeviceId);

        It should_send_RegisterTabletCommand = () =>
            registerTabletCommand.ShouldNotBeNull();

        It should_send_RegisterTabletCommand_DeviceId_specified = () =>
            registerTabletCommand.DeviceId.ShouldEqual(deviceId);

        It should_send_RegisterTabletCommand_UserId_specified = () =>
            registerTabletCommand.UserId.ShouldEqual(userId);

        It should_send_RegisterTabletCommand_AppVersion_specified = () =>
            registerTabletCommand.AppVersion.ShouldEqual(appVersion);

        It should_send_RegisterTabletCommand_AndroidId_specified = () =>
            registerTabletCommand.AndroidId.ShouldEqual(androidId);

        private static RegisterTabletCommand registerTabletCommand;
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