using System;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization;
using WB.Core.Synchronization.Commands;
using WB.Core.Synchronization.Documents;
using WB.Core.Synchronization.Implementation.SyncManager;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Core.Synchronization
{
    internal class when_sync_is_initialized_and_device_is_not_registered : SyncManagerTestContext
    {
        Establish context = () =>
        {
            devices = Mock.Of<IReadSideRepositoryReader<TabletDocument>>();

            clientIdentifier = CreateClientIdentifier(userId: userId, androidId: androidId, appVersion: appVersion);
            commandServiceMock = new Mock<ICommandService>();

            commandServiceMock
                .Setup(x => x.Execute(Moq.It.IsAny<RegisterTabletCommand>(), Moq.It.IsAny<string>()))
                .Callback((ICommand command, string origin) => registerTabletCommand = command as RegisterTabletCommand);

            syncManager = CreateSyncManager(commandService: commandServiceMock.Object, devices: devices);
        };

        Because of = () =>
            handshakePackage = syncManager.InitSync(clientIdentifier);

        It should_send_RegisterTabletCommand = () =>
            registerTabletCommand.ShouldNotBeNull();

        It should_send_RegisterTabletCommand_DeviceId_specified = () =>
           registerTabletCommand.DeviceId.ShouldEqual(deviceId);

        It should_send_RegisterTabletCommand_AndroidId_specified = () =>
           registerTabletCommand.AndroidId.ShouldEqual(androidId);

        It should_send_RegisterTabletCommand_UserId_specified = () =>
           registerTabletCommand.UserId.ShouldEqual(userId);

        It should_send_RegisterTabletCommand_AppVersion_specified = () =>
           registerTabletCommand.AppVersion.ShouldEqual(appVersion);

        It should_return_package_with_ClientInstanceKey_specified = () => 
            handshakePackage.ClientInstanceKey.ShouldEqual(clientIdentifier.ClientInstanceKey);

        It should_return_package_with_UserId_specified = () =>
            handshakePackage.UserId.ShouldEqual(clientIdentifier.UserId);

        private static RegisterTabletCommand registerTabletCommand;
        private static SyncManager syncManager;
        private static ClientIdentifier clientIdentifier;
        private static HandshakePackage handshakePackage;
        private static Mock<ICommandService> commandServiceMock;
        private static IReadSideRepositoryReader<TabletDocument> devices;
        private static Guid userId = Guid.Parse("11111111111111111111111111111111");        
        private static string androidId = "Android";
        private static Guid deviceId = androidId.ToGuid();
        private static string appVersion = "6002";
    }
}