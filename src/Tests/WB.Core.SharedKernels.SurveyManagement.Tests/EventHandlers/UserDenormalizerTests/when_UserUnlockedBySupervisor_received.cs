using System;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.User;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.Synchronization;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.UserDenormalizerTests
{
    internal class when_UserUnlockedBySupervisor_received : UserDenormalizerContext
    {
        private Establish context = () =>
        {
            userId = Guid.Parse("22222222222222222222222222222222");
            commandExecutorId = Guid.Parse("33333333333333333333333333333333");
            
            var user = new UserDocument() { PublicKey = userId };

            var userDocumentMockStorage = new Mock<IReadSideRepositoryWriter<UserDocument>>();
            userDocumentMockStorage.Setup(x => x.GetById(Moq.It.IsAny<string>())).Returns(user);
            userDocumentMockStorage.Setup(x => x.Store(Moq.It.IsAny<UserDocument>(), Moq.It.IsAny<string>())).Callback((UserDocument userDoc, string id) => userToSave = userDoc);
            denormalizer = CreateUserDenormalizer(users: userDocumentMockStorage.Object);
            
            userUnlockedBySupervisorEvnt = CreateUserUnlockedBySupervisor(commandExecutorId, userId);
        };

        private Because of = () =>
            denormalizer.Handle(userUnlockedBySupervisorEvnt);

        private It should_user_be_locked_by_supervisor = () =>
            userToSave.IsLockedBySupervisor.ShouldEqual(false);
        
        private static UserDenormalizer denormalizer;
        private static Guid commandExecutorId;
        private static IPublishedEvent<UserUnlockedBySupervisor> userUnlockedBySupervisorEvnt;
        private static Guid userId;

        private static UserDocument userToSave;

    }
}
