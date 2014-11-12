using System;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.User;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.User;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.Synchronization;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.UserDenormalizerTests
{
    internal class UserDenormalizerContext
    {
        protected static UserDenormalizer CreateUserDenormalizer(IReadSideRepositoryWriter<UserDocument> users = null, 
            ISynchronizationDataStorage syncStorage = null)
        {
            return new UserDenormalizer(
                users ?? Mock.Of<IReadSideRepositoryWriter<UserDocument>>(),
                syncStorage ?? new Mock<ISynchronizationDataStorage>().Object);
        }

        protected static IPublishedEvent<T> ToPublishedEvent<T>(T @event, Guid eventSourceId) where T : class
        {
            return Mock.Of<IPublishedEvent<T>>(publishedEvent
                => publishedEvent.Payload == @event
                && publishedEvent.EventSourceId == eventSourceId);
        }
        
        protected static IPublishedEvent<UserLockedBySupervisor> CreateUserLockedBySupervisor(Guid executorId, Guid eventSourceId)
        {
            var evnt = ToPublishedEvent(new UserLockedBySupervisor(), eventSourceId);
            return evnt;
        }

        protected static IPublishedEvent<UserLocked> CreateUserLocked(Guid eventSourceId)
        {
            return ToPublishedEvent(new UserLocked(), eventSourceId);
        }

        protected static IPublishedEvent<UserUnlockedBySupervisor> CreateUserUnlockedBySupervisor(Guid executorId, Guid eventSourceId)
        {
            return ToPublishedEvent(new UserUnlockedBySupervisor(), eventSourceId);
        }

        protected static IPublishedEvent<UserUnlocked> CreateUserUnlocked(Guid eventSourceId)
        {
            return ToPublishedEvent(new UserUnlocked(), eventSourceId);
        }

        protected static IPublishedEvent<NewUserCreated> CreateNewUserCreated(Guid eventSourceId)
        {
            return ToPublishedEvent(new NewUserCreated(), eventSourceId);
        }

        protected static IPublishedEvent<UserChanged> CreateUserChanged(Guid eventSourceId)
        {
            return ToPublishedEvent(new UserChanged()
            {
                 Email = "test",
                 Roles = new UserRoles[0]{},
            }, eventSourceId);
        }
    }
}
