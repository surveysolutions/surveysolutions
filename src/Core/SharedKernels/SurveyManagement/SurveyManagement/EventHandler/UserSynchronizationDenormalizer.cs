using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.User;
using Ncqrs.Eventing.ServiceModel.Bus;

using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.User;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    internal class UserSynchronizationDenormalizer : BaseDenormalizer,
        IEventHandler<NewUserCreated>,
        IEventHandler<UserChanged>,
        IEventHandler<UserLocked>,
        IEventHandler<UserUnlocked>,
        IEventHandler<UserLockedBySupervisor>,
        IEventHandler<UserUnlockedBySupervisor>
    {
        private readonly IReadSideRepositoryWriter<UserDocument> users;
        private readonly IJsonUtils jsonUtils;
        private readonly IOrderableSyncPackageWriter<UserSyncPackage> userPackageStorageWriter;

        private const string CounterId = "UserSyncPackageСounter";

        public UserSynchronizationDenormalizer(
            IReadSideRepositoryWriter<UserDocument> users, 
            IJsonUtils jsonUtils,
            IOrderableSyncPackageWriter<UserSyncPackage> userPackageStorageWriter)
        {
            this.users = users;
            this.jsonUtils = jsonUtils;
            this.userPackageStorageWriter = userPackageStorageWriter;
        }

        public override object[] Writers
        {
            get { return new object[] { this.userPackageStorageWriter }; }
        }

        public override object[] Readers
        {
            get { return new object[] { this.users }; }

        }

        public void Handle(IPublishedEvent<NewUserCreated> evnt)
        {
            var doc = new UserDocument
                      {
                          PublicKey = evnt.EventSourceId,
                          CreationDate = evnt.EventTimeStamp,

                          UserName = evnt.Payload.Name,
                          Password = evnt.Payload.Password,
                          Email = evnt.Payload.Email,
                          IsLockedBySupervisor = evnt.Payload.IsLockedBySupervisor,
                          IsLockedByHQ = evnt.Payload.IsLocked,
                          Supervisor = evnt.Payload.Supervisor,
                          Roles = new List<UserRoles>(evnt.Payload.Roles)
                      };
            this.SaveUser(doc, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<UserChanged> evnt)
        {
            UserDocument item = this.users.GetById(evnt.EventSourceId.FormatGuid());

            item.Email = evnt.Payload.Email;
            item.Roles = evnt.Payload.Roles.ToList();
            item.Password = evnt.Payload.PasswordHash;

            this.SaveUser(item, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<UserLocked> evnt)
        {
            UserDocument item = this.users.GetById(evnt.EventSourceId);

            item.IsLockedByHQ = true;
            this.SaveUser(item, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<UserUnlocked> evnt)
        {
            UserDocument item = this.users.GetById(evnt.EventSourceId);

            item.IsLockedByHQ = false;
            this.SaveUser(item, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<UserLockedBySupervisor> evnt)
        {
            UserDocument item = this.users.GetById(evnt.EventSourceId);

            item.IsLockedBySupervisor = true;
            this.SaveUser(item, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<UserUnlockedBySupervisor> evnt)
        {
            UserDocument item = this.users.GetById(evnt.EventSourceId);

            item.IsLockedBySupervisor = false;
            this.SaveUser(item, evnt.EventTimeStamp);
        }

        private void SaveUser(UserDocument user, DateTime timestamp)
        {
            if (!user.Roles.Contains(UserRoles.Operator))
            {
                return;
            }

            userPackageStorageWriter.StoreNextPackage(
                CounterId,
                nextSortIndex =>
                {
                    var synchronizationDelta = new UserSyncPackage(
                        userId: user.PublicKey,
                        content: this.GetItemAsContent(user),
                        timestamp: timestamp,
                        sortIndex: nextSortIndex);
                    return synchronizationDelta;
                });
        }

        protected string GetItemAsContent(UserDocument item)
        {
            return this.jsonUtils.Serialize(item, TypeSerializationSettings.AllTypes);
        }
    }
}