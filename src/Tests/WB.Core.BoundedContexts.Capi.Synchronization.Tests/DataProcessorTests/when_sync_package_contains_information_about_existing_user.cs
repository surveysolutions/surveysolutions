using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Capi.Synchronization.ChangeLog;
using WB.Core.BoundedContexts.Capi.Synchronization.Implementation;
using WB.Core.BoundedContexts.Capi.Synchronization.Views.Login;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContext.Capi.Synchronization.Tests.DataProcessorTests
{
    internal class when_sync_package_contains_information_about_existing_user : DataProcessorTestContext
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
            syncItem = new SyncItem() { ItemType = SyncItemType.User, IsCompressed = false, Content = "some content", Id = Guid.NewGuid()};

            commandService = new Mock<ICommandService>();

            var loginViewFactoryMock = new Mock<IViewFactory<LoginViewInput, LoginView>>();
            loginViewFactoryMock.Setup(x => x.Load(Moq.It.IsAny<LoginViewInput>())).Returns(new LoginView(Guid.NewGuid(), "test"));

            changeLogManipulator=new Mock<IChangeLogManipulator>();
            dataProcessor = CreateDataProcessor(changeLogManipulator.Object, commandService.Object, jsonUtilsMock.Object, loginViewFactoryMock.Object);
        };

        Because of = () => dataProcessor.ProcessPulledItem(syncItem);

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
                        x.CreatePublicRecord(syncItem.Id),
                    Times.Once);

        private static DataProcessor dataProcessor;
        private static SyncItem syncItem;
        private static UserDocument userDocument;
        private static Mock<ICommandService> commandService;
        private static Mock<IChangeLogManipulator> changeLogManipulator;
    }
}
