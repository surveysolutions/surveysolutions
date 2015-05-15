using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Capi.ChangeLog;
using WB.Core.BoundedContexts.Capi.Implementation.Services;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveySolutions.Services;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Capi.CapiDataSynchronizationServiceTests
{
    internal class when_sync_package_contains_information_about_new_user : CapiDataSynchronizationServiceTestContext
    {
        Establish context = () =>
        {
            userDocument = new UserDocument()
            {
                PublicKey = Guid.NewGuid(),
                Email = "test@ddd.cc",
                Roles = new HashSet<UserRoles> { UserRoles.Operator },
                UserName = "name",
                Password = "password",
                IsLockedByHQ = false,
                IsLockedBySupervisor = false,
                Supervisor = new UserLight(Guid.NewGuid(), "super")
            };

            var jsonUtilsMock = new Mock<IJsonUtils>();
            jsonUtilsMock.Setup(x => x.Deserialize<UserDocument>(Moq.It.IsAny<string>())).Returns(userDocument);
            syncItem = new UserSyncPackageDto()
                       {
                           Content = "some content"
                       };

            commandService=new Mock<ICommandService>();

            changeLogManipulator = new Mock<IChangeLogManipulator>();
            capiDataSynchronizationService = CreateCapiDataSynchronizationService(changeLogManipulator.Object, commandService.Object, jsonUtilsMock.Object);
        };

        Because of = () => capiDataSynchronizationService.ProcessDownloadedPackage(syncItem);

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

        private static CapiDataSynchronizationService capiDataSynchronizationService;
        private static UserSyncPackageDto syncItem;
        private static UserDocument userDocument;
        private static Mock<ICommandService> commandService;
        private static Mock<IChangeLogManipulator> changeLogManipulator;
    }
}
