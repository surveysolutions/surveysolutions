using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories
{
    internal class UserPlainStorageRepository: IPlainAggregateRootRepository<User>
    {
        private readonly IPlainStorageAccessor<UserDocument> userDocumentStorage;

        public UserPlainStorageRepository(IPlainStorageAccessor<UserDocument> userDocumentStorage)
        {
            this.userDocumentStorage = userDocumentStorage;
        }

        public User Get(Guid aggregateId)
        {
            var userDocument = this.userDocumentStorage.GetById(aggregateId.FormatGuid());

            return CreateUser(userDocument);
        }

        public void Save(User user)
        {
            var userDocument = this.userDocumentStorage.GetById(user.Id.FormatGuid()) ?? new UserDocument();

            userDocument.UserId = user.Id.FormatGuid();
            userDocument.UserName = user.UserName;
            userDocument.Password = user.Password;
            userDocument.PublicKey = user.Id;
            userDocument.CreationDate = user.CreationDate;
            userDocument.Email = user.Email;
            userDocument.IsLockedBySupervisor = user.IsLockedBySupervisor;
            userDocument.IsLockedByHQ = user.IsLockedByHQ;
            userDocument.Supervisor = user.Supervisor;
            userDocument.PersonName = user.PersonName;
            userDocument.PhoneNumber = user.PhoneNumber;
            userDocument.DeviceId = user.DeviceId;
            userDocument.IsArchived = user.IsArchived;
            userDocument.LastChangeDate = user.LastChangeDate;

            UpdateSet(userDocument.Roles, user.Roles);
            UpdateSet(userDocument.DeviceChangingHistory, user.DeviceChangingHistory);
            this.userDocumentStorage.Store(userDocument, user.Id.FormatGuid());
        }

        private void UpdateSet<T>(ISet<T> originalSet, IEnumerable<T> updatedSet)
        {
            var itemsToAdd = updatedSet.Except(originalSet);
            foreach (T itemToAdd in itemsToAdd)
            {
                originalSet.Add(itemToAdd);
            }

            var itemsToRemove = originalSet.Except(originalSet);
            foreach (T itemToRemove in itemsToRemove)
            {
                originalSet.Remove(itemToRemove);
            }
        }

        private User CreateUser(UserDocument userDocument)
        {
            var result = new User();

            result.SetId(userDocument.PublicKey);

            result.CreationDate = userDocument.CreationDate;
            result.DeviceChangingHistory = userDocument.DeviceChangingHistory.ToList();
            result.Email = userDocument.Email;
            result.DeviceId = userDocument.DeviceId;
            result.IsArchived = userDocument.IsArchived;
            result.IsLockedByHQ = userDocument.IsLockedByHQ;
            result.IsLockedBySupervisor = userDocument.IsLockedBySupervisor;
            result.LastChangeDate = userDocument.LastChangeDate;
            result.Password = userDocument.Password;
            result.PersonName = userDocument.PersonName;
            result.PhoneNumber = userDocument.PhoneNumber;
            result.Roles = userDocument.Roles.ToArray();
            result.Supervisor = userDocument.Supervisor;

            result.UserName = userDocument.UserName;
            return result;
        }
    }
}