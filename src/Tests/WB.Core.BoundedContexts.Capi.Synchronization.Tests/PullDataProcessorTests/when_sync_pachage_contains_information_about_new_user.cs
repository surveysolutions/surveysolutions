using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Sync;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Capi.Synchronization.Synchronization.Pull;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContext.Capi.Synchronization.Tests.PullDataProcessorTests
{
    internal class when_sync_pachage_contains_information_about_new_user : PullDataProcessorTestContext
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
            syncItem = new SyncItem() { ItemType = SyncItemType.User, IsCompressed = false, Content = "some content"};

            commandService=new Mock<ICommandService>();
            pullDataProcessor = CreatePullDataProcessor(commandService.Object,jsonUtilsMock.Object);
        };

        Because of = () => pullDataProcessor.Process(syncItem);

        private It should_call_CreateUserCommand_once =
            () =>
                commandService.Verify(
                    x =>
                        x.Execute(
                            Moq.It.Is<CreateUserCommand>(
                                param =>
                                    param.PublicKey == userDocument.PublicKey && param.Email == userDocument.Email &&
                                        param.UserName == userDocument.UserName && param.IsLockedByHQ == userDocument.IsLockedByHQ
                                        && param.IsLockedBySupervisor == userDocument.IsLockedBySupervisor &&
                                        param.Roles.SequenceEqual(userDocument.Roles)&&
                                        param.Password==userDocument.Password), null),
                    Times.Once);
        
        private static PullDataProcessor pullDataProcessor;
        private static SyncItem syncItem;
        private static UserDocument userDocument;
        private static Mock<ICommandService> commandService;
    }
}
