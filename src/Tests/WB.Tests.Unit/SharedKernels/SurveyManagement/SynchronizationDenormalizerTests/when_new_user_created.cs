using System;

using Machine.Specifications;

using Moq;

using WB.Core.GenericSubdomains.Portable.Services;
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

            jsonUtilsMock = new Mock<IJsonUtils>();

            jsonUtilsMock.Setup(x => x.Serialize(Moq.It.IsAny<object>(), Moq.It.IsAny<TypeSerializationSettings>()))
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
                    Moq.It.IsAny<UserSyncPackageContent>(),
                    Moq.It.Is<UserSyncPackageMeta>(u => u.UserId == userId),
                    partialPackageId,
                    CounterId),
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
        private static Mock<IOrderableSyncPackageWriter<UserSyncPackageMeta, UserSyncPackageContent>> userSyncPackageWriter= new Mock<IOrderableSyncPackageWriter<UserSyncPackageMeta, UserSyncPackageContent>>();
        private static Guid userId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        private static string partialPackageId = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
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