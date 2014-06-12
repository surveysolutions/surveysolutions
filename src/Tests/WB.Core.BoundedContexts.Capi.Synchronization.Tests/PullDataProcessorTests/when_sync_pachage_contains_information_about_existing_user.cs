using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Capi.Synchronization.Synchronization.Pull;
using WB.Core.BoundedContexts.Capi.Synchronization.Views.Login;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContext.Capi.Synchronization.Tests.PullDataProcessorTests
{
    internal class when_sync_pachage_contains_information_about_existing_user : PullDataProcessorTestContext
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
            syncItem = new SyncItem() { ItemType = SyncItemType.User, IsCompressed = false, Content = "some content" };

            commandService = new Mock<ICommandService>();

            var loginViewFactoryMock = new Mock<IViewFactory<LoginViewInput, LoginView>>();
            loginViewFactoryMock.Setup(x => x.Load(Moq.It.IsAny<LoginViewInput>())).Returns(new LoginView(Guid.NewGuid(), "test"));
            pullDataProcessor = CreatePullDataProcessor(commandService.Object, jsonUtilsMock.Object, loginViewFactoryMock.Object);
        };

        Because of = () => pullDataProcessor.Process(syncItem);

        private It should_call_ChangeUserCommand_once =
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

        private static PullDataProcessor pullDataProcessor;
        private static SyncItem syncItem;
        private static UserDocument userDocument;
        private static Mock<ICommandService> commandService;
    }
}
