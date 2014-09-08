using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Events.User;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.Synchronization;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.UserDenormalizerTests
{
    internal class when_UserUnlocked_received : UserDenormalizerContext
    {
        private Establish context = () =>
        {
            userId = Guid.Parse("22222222222222222222222222222222");
            commandExecutorId = Guid.Parse("33333333333333333333333333333333");

            syncStorage = new Mock<ISynchronizationDataStorage>();
            syncStorage.Setup(x => x.SaveUser(Moq.It.IsAny<UserDocument>(), Moq.It.IsAny<DateTime>()))
                .Callback((UserDocument userDoc, DateTime timestamp) => userToSave = userDoc);
            
            var user = new UserDocument() { PublicKey = userId };

            var userDocumentMockStorage = new Mock<IReadSideRepositoryWriter<UserDocument>>();
            userDocumentMockStorage.Setup(x => x.GetById(Moq.It.IsAny<string>())).Returns(user);

            denormalizer = CreateUserDenormalizer(users: userDocumentMockStorage.Object, syncStorage: syncStorage.Object);

            userUnlockedEvnt = CreateUserUnlocked(userId);
        };

        private Because of = () =>
            denormalizer.Handle(userUnlockedEvnt);

        private It should_sync_storage_stores_new_state = () =>
            syncStorage.Verify(x => x.SaveUser(Moq.It.IsAny<UserDocument>(), Moq.It.IsAny<DateTime>()), Times.Once);

        private It should_user_be_locked = () =>
            userToSave.IsLockedByHQ.ShouldEqual(false);
        
        private static UserDenormalizer denormalizer;
        private static Guid commandExecutorId;
        private static IPublishedEvent<UserUnlocked> userUnlockedEvnt;
        private static Guid userId;
        private static Mock<ISynchronizationDataStorage> syncStorage;

        private static UserDocument userToSave;

    }
}
