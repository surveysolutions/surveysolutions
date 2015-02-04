using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.User;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.User;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    internal class UserSynchronizationDenormalizer : BaseSynchronizationDenormalizer,
        IEventHandler<NewUserCreated>,
        IEventHandler<UserChanged>,
        IEventHandler<UserLocked>,
        IEventHandler<UserUnlocked>,
        IEventHandler<UserLockedBySupervisor>,
        IEventHandler<UserUnlockedBySupervisor>
    {
        private readonly IReadSideRepositoryWriter<UserDocument> users;

        public UserSynchronizationDenormalizer(IReadSideRepositoryWriter<UserDocument> users, 
            IArchiveUtils archiver,
            IJsonUtils jsonUtils,
            IReadSideRepositoryWriter<SynchronizationDelta> syncStorage,
            IQueryableReadSideRepositoryReader<SynchronizationDelta> syncStorageReader)
            : base(archiver, jsonUtils, syncStorage, syncStorageReader)
        {
            this.users = users;
        }

        public override object[] Writers
        {
            get { return new object[] { syncStorage }; }
        }

        public override object[] Readers
        {
            get { return new object[] { this.users }; }

        }

        public void Handle(IPublishedEvent<NewUserCreated> evnt)
        {
            var doc = new UserDocument
                      {
                          UserName = evnt.Payload.Name,
                          Password = evnt.Payload.Password,
                          PublicKey = evnt.Payload.PublicKey,
                          CreationDate = DateTime.UtcNow,
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
            UserDocument item = this.users.GetById(evnt.EventSourceId);

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

        public void SaveUser(UserDocument doc, DateTime timestamp)
        {
            if (doc.Roles.Contains(UserRoles.Operator))
            {
                var syncItem = this.CreateSyncItem(doc.PublicKey, SyncItemType.User, GetItemAsContent(doc), string.Empty);

                StoreChunk(syncItem, doc.PublicKey, timestamp);
            }
        }
    }
}