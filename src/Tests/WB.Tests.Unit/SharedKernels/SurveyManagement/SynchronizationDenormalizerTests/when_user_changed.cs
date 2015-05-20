using System;

using Machine.Specifications;

using Moq;

using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.Synchronization.SyncStorage;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SynchronizationDenormalizerTests
{
    internal class when_user_changed : UserSynchronizationDenormalizerTestsContext
    {
        Establish context = () =>
        {
            var storedUser = CreateUserDocument(userId);

            usersRepository = Mock.Of<IReadSideRepositoryWriter<UserDocument>>(x => x.GetById(userId.FormatGuid()) == storedUser);

            jsonUtilsMock = new Mock<IJsonUtils>();

            jsonUtilsMock.Setup(x => x.Serialize(Moq.It.IsAny<object>(), TypeSerializationSettings.AllTypes))
                .Callback((object u, TypeSerializationSettings serializationSettings) => user = u as UserDocument);

            denormalizer = CreateDenormalizer(
                userPackageStorageWriter: userSyncPackageWriter.Object,
                users: usersRepository,
                jsonUtils: jsonUtilsMock.Object);
        };

        Because of = () =>
            denormalizer.Handle(Create.Event.UserChanged(userId, password, email));

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

        It should_serialize_user_with_Password_specified = () =>
            user.Password.ShouldEqual(password);

        It should_serialize_user_with_Email_specified = () =>
            user.Email.ShouldEqual(email);

        private static UserSynchronizationDenormalizer denormalizer;
        private static Mock<IOrderableSyncPackageWriter<UserSyncPackageMeta, UserSyncPackageContent>> userSyncPackageWriter = new Mock<IOrderableSyncPackageWriter<UserSyncPackageMeta, UserSyncPackageContent>>();
        private static Guid userId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        private static string partialPackageId = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        private static Mock<IJsonUtils> jsonUtilsMock;
        private static UserDocument user;
        private static string password = "hello_vasya";
        private static string email = "vasya@the.best";
        private static IReadSideRepositoryWriter<UserDocument> usersRepository;
    }
}