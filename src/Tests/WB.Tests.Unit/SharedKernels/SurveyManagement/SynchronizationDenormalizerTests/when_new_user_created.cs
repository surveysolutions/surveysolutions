using System;

using Machine.Specifications;

using Moq;

using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.Synchronization.SyncStorage;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SynchronizationDenormalizerTests
{
    internal class when_new_user_created : UserSynchronizationDenormalizerTestsContext
    {
        Establish context = () =>
        {
            usersRepository = Mock.Of<IReadSideRepositoryWriter<UserDocument>>();

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
            denormalizer.Handle(Create.Event.NewUserCreated(userId, name, password, email, isLockedBySupervisor, isLocked));

        It should_create_new_user_package = () =>
            userSyncPackageWriter.Verify(
                x => x.Store(
                    Moq.It.Is<UserSyncPackage>(
                        u => u.PackageId == packageId && u.SortIndex == sortIndex && u.UserId == userId),
                    packageId),
                Times.Once);

        It should_serialize_not_empty_user = () => 
            user.ShouldNotBeNull();

        It should_serialize_user_with_Name_specified = () =>
            user.UserName.ShouldEqual(name);

        It should_serialize_user_with_Password_specified = () =>
            user.Password.ShouldEqual(password);

        It should_serialize_user_with_Email_specified = () =>
            user.Email.ShouldEqual(email);

        It should_serialize_user_with_IsLockedBySupervisor_specified = () =>
            user.IsLockedBySupervisor.ShouldEqual(isLockedBySupervisor);

        It should_serialize_user_with_IsLockedByHQ_specified = () =>
            user.IsLockedByHQ.ShouldEqual(isLocked);

        private static UserSynchronizationDenormalizer denormalizer;
        private static Mock<IOrderableSyncPackageWriter<UserSyncPackage>> userSyncPackageWriter;
        private static Guid userId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        private static int sortIndex = 5;
        private static string packageId = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa$5";
        private static UserDocument user;
        private static IReadSideRepositoryWriter<UserDocument> usersRepository;
        private static string name = "vasya";
        private static string password = "hello_vasya";
        private static string email = "vasya@the.best";
        private static bool isLockedBySupervisor = true;
        private static bool isLocked = true;
        private static Mock<IJsonUtils> jsonUtilsMock;
    }
}