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

            return ConvertToUser(userDocument);
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

        private static void UpdateSet<T>(ISet<T> originalSet, IEnumerable<T> updatedSet)
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

        private static User ConvertToUser(UserDocument userDocument)
        {
            var result = new User
            {
                CreationDate = userDocument.CreationDate,
                DeviceChangingHistory = userDocument.DeviceChangingHistory.ToList(),
                Email = userDocument.Email,
                DeviceId = userDocument.DeviceId,
                IsArchived = userDocument.IsArchived,
                IsLockedByHQ = userDocument.IsLockedByHQ,
                IsLockedBySupervisor = userDocument.IsLockedBySupervisor,
                LastChangeDate = userDocument.LastChangeDate,
                Password = userDocument.Password,
                PersonName = userDocument.PersonName,
                PhoneNumber = userDocument.PhoneNumber,
                Roles = userDocument.Roles.ToArray(),
                Supervisor = userDocument.Supervisor,
                UserName = userDocument.UserName
            };

            result.SetId(userDocument.PublicKey);

            return result;
        }
    }
}