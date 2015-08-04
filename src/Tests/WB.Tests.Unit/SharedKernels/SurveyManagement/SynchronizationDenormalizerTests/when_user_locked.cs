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
    internal class when_user_locked : UserSynchronizationDenormalizerTestsContext
    {
        Establish context = () =>
        {
            var storedUser = CreateUserDocument(userId);

            usersRepository = Mock.Of<IReadSideRepositoryWriter<UserDocument>>(x => x.GetById(userId.FormatGuid()) == storedUser);

            jsonUtilsMock = new Mock<IJsonUtils>();

            jsonUtilsMock.Setup(x => x.Serialize(Moq.It.IsAny<object>(), Moq.It.IsAny<TypeSerializationSettings>()))
                .Callback((object u, TypeSerializationSettings serializationSettings) => user = u as UserDocument);

            denormalizer = CreateDenormalizer(
                userPackageStorageWriter: userSyncPackageWriter.Object,
                users: usersRepository,
                jsonUtils: jsonUtilsMock.Object);
        };

        Because of = () =>
            denormalizer.Handle(Create.Event.UserLocked(userId));

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

        It should_serialize_user_with_IsLockedByHQ_set_in_true = () =>
            user.IsLockedByHQ.ShouldBeTrue();

        private static UserSynchronizationDenormalizer denormalizer;
        private static Mock<IOrderableSyncPackageWriter<UserSyncPackageMeta, UserSyncPackageContent>> userSyncPackageWriter = new Mock<IOrderableSyncPackageWriter<UserSyncPackageMeta, UserSyncPackageContent>>();
        private static Guid userId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        private static string partialPackageId = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        private static Mock<IJsonUtils> jsonUtilsMock;
        private static UserDocument user;
        private static IReadSideRepositoryWriter<UserDocument> usersRepository;
    }
}