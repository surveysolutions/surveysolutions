using System;
using System.Linq;
using Main.Core.Events.User;
using Ncqrs.Eventing;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.Events.User;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace EventStoreToPlainStorageMigrator
{
    public class UserMigrationEventHandlers
    {
        internal readonly ObsoleteEventHandleDescriptor[] eventHandlers;

        private readonly UserPlainStorageRepository userPlainStorageRepository;

        internal UserMigrationEventHandlers(PlainPostgresTransactionManager plainPostgresTransactionManager)
        {
            var userDocumentStorage = new PostgresPlainStorageRepository<UserDocument>(plainPostgresTransactionManager);

            var isUserDocumentsTableEmpty = plainPostgresTransactionManager.ExecuteInPlainTransaction(() => userDocumentStorage.Query(_ => _.Count() == 0));
            if (!isUserDocumentsTableEmpty)
            {
                throw new InvalidOperationException("Table UserDocument is not empty. That means something already have created users in the plain storage. Please think twice before cleaning the table and run the tool again!!!");
            }

            userPlainStorageRepository = new UserPlainStorageRepository(userDocumentStorage);

            eventHandlers = new ObsoleteEventHandleDescriptor[]
            {
                new ObsoleteEventHandleDescriptor<NewUserCreated>(HandleNewUserCreatedIfPossible),
                new UserEventHandlerDescriptor<UserArchived>(HandleUserArchivedIfPossible,userPlainStorageRepository),
                new UserEventHandlerDescriptor<UserUnarchived>(HandleUserUnarchivedIfPossible,userPlainStorageRepository),
                new UserEventHandlerDescriptor<UserLocked>(HandleUserLockedIfPossible,userPlainStorageRepository),
                new UserEventHandlerDescriptor<UserUnlocked>(HandleUserUnlockedIfPossible,userPlainStorageRepository),
                new UserEventHandlerDescriptor<UserLockedBySupervisor>(HandleUserLockedBySupervisorIfPossible,userPlainStorageRepository),
                new UserEventHandlerDescriptor<UserUnlockedBySupervisor>(HandleUserUnlockedBySupervisorIfPossible,userPlainStorageRepository),
                new UserEventHandlerDescriptor<UserChanged>(HandleUserChangedIfPossible,userPlainStorageRepository),
                new UserEventHandlerDescriptor<UserLinkedToDevice>(HandleUserLinkedToDeviceIfPossible,userPlainStorageRepository)
            };
        }

        private void HandleNewUserCreatedIfPossible(
            NewUserCreated newUserCreated, Guid eventSourceId,
            long eventSequence)
        {
            var user = new User();
            user.SetId(eventSourceId);
            user.CreateUser(newUserCreated.Email, newUserCreated.IsLockedBySupervisor, newUserCreated.IsLocked,
                newUserCreated.Password, newUserCreated.PublicKey, newUserCreated.Roles, newUserCreated.Supervisor,
                newUserCreated.Name, newUserCreated.PersonName, newUserCreated.PhoneNumber);
            userPlainStorageRepository.Save(user);
        }

        private void HandleUserArchivedIfPossible(
            UserArchived userArchived,
            User user)
        {
            user.Archive();
        }

        private void HandleUserUnarchivedIfPossible(
            UserUnarchived userUnarchived,
            User user)
        {
            user.Unarchive();
        }

        private void HandleUserLockedIfPossible(
            UserLocked userLocked,
            User user)
        {
            user.Lock();
        }

        private void HandleUserUnlockedIfPossible(
            UserUnlocked userUnlocked,
            User user)
        {
            user.Unlock();
        }

        private void HandleUserLockedBySupervisorIfPossible(
            UserLockedBySupervisor userLockedBySupervisor,
            User user)
        {
            user.LockBySupervisor();
        }

        private void HandleUserUnlockedBySupervisorIfPossible(
            UserUnlockedBySupervisor userUnlockedBySupervisor,
            User user)
        {
            user.UnlockBySupervisor();
        }

        private void HandleUserChangedIfPossible(
            UserChanged userChanged,
            User user)
        {
            user.ChangeUser(userChanged.Email, user.IsLockedBySupervisor, user.IsLockedByHQ,
                userChanged.PasswordHash, userChanged.PersonName, userChanged.PhoneNumber, user.Id);
        }

        private void HandleUserLinkedToDeviceIfPossible(
            UserLinkedToDevice userLinkedToDevice,
            User user)
        {
            user.LinkUserToDevice(new LinkUserToDevice(user.Id, userLinkedToDevice.DeviceId));
        }

        internal class UserEventHandlerDescriptor<T> : ObsoleteEventHandleDescriptor
            where T : class, WB.Core.Infrastructure.EventBus.IEvent
        {
            private Action<T, User> action;
            private readonly UserPlainStorageRepository userPlainStorageRepository;
            public UserEventHandlerDescriptor(Action<T, User> action, UserPlainStorageRepository userPlainStorageRepository)
            {
                this.action = action;
                this.userPlainStorageRepository = userPlainStorageRepository;
            }

            public override void Handle(CommittedEvent @event,
                PlainPostgresTransactionManager plainPostgresTransactionManager)
            {
                var typedEvent = @event.Payload as T;
                if (typedEvent == null)
                    return;

                plainPostgresTransactionManager.ExecuteInPlainTransaction(() =>
                {
                    var user = InitUser(@event.EventSourceId);
                    this.action(typedEvent, user);
                    userPlainStorageRepository.Save(user);
                });
            }

            private User InitUser(Guid id)
            {
                return userPlainStorageRepository.Get(id);
            }
        }
    }
}