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
    internal class when_user_unlocked_by_supervisor : UserSynchronizationDenormalizerTestsContext
    {
        Establish context = () =>
        {
            var storedUser = CreateUserDocument(userId);

            usersRepository = Mock.Of<IReadSideRepositoryWriter<UserDocument>>(x => x.GetById(userId.FormatGuid()) == storedUser);

            jsonUtilsMock = new Mock<ISerializer>();

            jsonUtilsMock.Setup(x => x.Serialize(Moq.It.IsAny<object>(), Moq.It.IsAny<TypeSerializationSettings>()))
                .Callback((object u, TypeSerializationSettings serializationSettings) => user = u as UserDocument);

            denormalizer = CreateDenormalizer(
                userPackageStorageWriter: userSyncPackageWriter.Object,
                users: usersRepository,
                serializer: jsonUtilsMock.Object);
        };

        Because of = () =>
            denormalizer.Handle(Create.Event.UserUnlockedBySupervisor(userId, eventId: Guid.Parse(partialPackageId)));

        It should_create_new_user_package = () =>
            userSyncPackageWriter.Verify(
                x => x.Store(
                    Moq.It.Is<UserSyncPackageMeta>(u => u.UserId == userId),
                    partialPackageId),
                Times.Once);

        It should_serialize_not_empty_user = () =>
            user.ShouldNotBeNull();

        It should_serialize_user_with_IsLockedBySupervisor_set_in_fasle = () =>
            user.IsLockedBySupervisor.ShouldBeFalse();

        private static UserSynchronizationDenormalizer denormalizer;
        private static Mock<IReadSideRepositoryWriter<UserSyncPackageMeta>> userSyncPackageWriter = new Mock<IReadSideRepositoryWriter<UserSyncPackageMeta>>();
        private static Guid userId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        private static string partialPackageId = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        private static Mock<ISerializer> jsonUtilsMock;
        private static UserDocument user;
        private static IReadSideRepositoryWriter<UserDocument> usersRepository;
    }
}