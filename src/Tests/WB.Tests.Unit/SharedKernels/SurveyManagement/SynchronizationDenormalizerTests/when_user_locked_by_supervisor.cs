using System;

using Machine.Specifications;

using Moq;

using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.Synchronization.SyncStorage;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SynchronizationDenormalizerTests
{
    internal class when_user_locked_by_supervisor : UserSynchronizationDenormalizerTestsContext
    {
        Establish context = () =>
        {
            var storedUser = CreateUserDocument(userId);

            usersRepository = Mock.Of<IReadSideRepositoryWriter<UserDocument>>(x => x.GetById(userId.FormatGuid()) == storedUser);
            userSyncPackageWriter = new Mock<IOrderableSyncPackageWriter<UserSyncPackage>>();
            userSyncPackageWriter.Setup(x => x.GetNextOrder()).Returns(sortIndex);

            jsonUtilsMock = new Mock<IJsonUtils>();

            jsonUtilsMock.Setup(x => x.Serialize(Moq.It.IsAny<object>(), TypeSerializationSettings.AllTypes))
                .Callback((object u, TypeSerializationSettings serializationSettings) => user = u as UserDocument);

            denormalizer = CreateDenormalizer(
                userPackageStorageWriter: userSyncPackageWriter.Object,
                users: usersRepository,
                jsonUtils: jsonUtilsMock.Object);
        };

        Because of = () =>
            denormalizer.Handle(Create.Event.UserLockedBySupervisor(userId));

        It should_create_new_user_package = () =>
            userSyncPackageWriter.Verify(
                x => x.Store(
                    Moq.It.Is<UserSyncPackage>(
                        u => u.PackageId == packageId && u.SortIndex == sortIndex && u.UserId == userId),
                    packageId),
                Times.Once);

        It should_serialize_not_empty_user = () =>
            user.ShouldNotBeNull();

        It should_serialize_user_with_IsLockedBySupervisor_set_in_true = () =>
            user.IsLockedBySupervisor.ShouldBeTrue();

        private static UserSynchronizationDenormalizer denormalizer;
        private static Mock<IOrderableSyncPackageWriter<UserSyncPackage>> userSyncPackageWriter;
        private static Guid userId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        private static int sortIndex = 5;
        private static string packageId = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa$5";
        private static Mock<IJsonUtils> jsonUtilsMock;
        private static UserDocument user;
        private static IReadSideRepositoryWriter<UserDocument> usersRepository;
    }
}