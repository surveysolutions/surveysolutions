using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Capi.ChangeLog;
using WB.Core.BoundedContexts.Capi.Implementation.Services;
using WB.Core.BoundedContexts.Capi.Views.Login;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Utils.Services;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveySolutions.Services;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Capi.CapiDataSynchronizationServiceTests
{
    internal class when_sync_package_contains_information_about_existing_user : CapiDataSynchronizationServiceTestContext
    {
        Establish context = () =>
        {
            userDocument = new UserDocument()
            {
                PublicKey = Guid.NewGuid(),
                Email = "test@ddd.cc",
                Roles = new List<UserRoles> { UserRoles.Operator },
                UserName = "name",
                Password = "password",
                IsLockedByHQ = false,
                IsLockedBySupervisor = false,
                Supervisor = new UserLight(Guid.NewGuid(), "super")
            };

            var jsonUtilsMock = new Mock<IJsonUtils>();
            jsonUtilsMock.Setup(x => x.Deserrialize<UserDocument>(Moq.It.IsAny<string>())).Returns(userDocument);
            syncItem = new SyncItem() { ItemType = SyncItemType.User, IsCompressed = false, Content = "some content", RootId = Guid.NewGuid()};

            commandService = new Mock<ICommandService>();

            var loginViewFactoryMock = new Mock<IViewFactory<LoginViewInput, LoginView>>();
            loginViewFactoryMock.Setup(x => x.Load(Moq.It.IsAny<LoginViewInput>())).Returns(new LoginView(Guid.NewGuid(), "test"));

            changeLogManipulator=new Mock<IChangeLogManipulator>();
            capiDataSynchronizationService = CreateCapiDataSynchronizationService(changeLogManipulator.Object, commandService.Object, jsonUtilsMock.Object, loginViewFactoryMock.Object);
        };

        Because of = () => capiDataSynchronizationService.SavePulledItem(syncItem);

        It should_call_ChangeUserCommand_once =
            () =>
                commandService.Verify(
                    x =>
                        x.Execute(
                            Moq.It.Is<ChangeUserCommand>(
                                param =>
                                    param.PublicKey == userDocument.PublicKey && param.Email == userDocument.Email &&
                                        param.IsLockedByHQ == userDocument.IsLockedByHQ && 
                                        param.IsLockedBySupervisor == userDocument.IsLockedBySupervisor &&
                                        param.Roles.SequenceEqual(userDocument.Roles) &&
                                        param.PasswordHash == userDocument.Password), null),
                    Times.Once);

        It should_create_public_record_in_change_log_for_sync_item_once =
            () =>
                changeLogManipulator.Verify(
                    x =>
                        x.CreatePublicRecord(syncItem.RootId),
                    Times.Once);

        private static CapiDataSynchronizationService capiDataSynchronizationService;
        private static SyncItem syncItem;
        private static UserDocument userDocument;
        private static Mock<ICommandService> commandService;
        private static Mock<IChangeLogManipulator> changeLogManipulator;
    }
}
