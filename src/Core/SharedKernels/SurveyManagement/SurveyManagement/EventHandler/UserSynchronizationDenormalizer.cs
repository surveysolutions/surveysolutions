using System;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.User;
using Ncqrs.Eventing.ServiceModel.Bus;

using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.User;
using WB.Core.SharedKernels.DataCollection.Views;
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
        private readonly ISerializer serializer;
        private readonly IReadSideRepositoryWriter<UserSyncPackageMeta> syncPackageWriter;

        public UserSynchronizationDenormalizer(
            IReadSideRepositoryWriter<UserDocument> users, 
            ISerializer serializer,
            IReadSideRepositoryWriter<UserSyncPackageMeta> syncPackageWriter)
        {
            this.users = users;
            this.serializer = serializer;
            this.syncPackageWriter = syncPackageWriter;
        }

        public override object[] Writers
        {
            get { return new object[] { this.syncPackageWriter }; }
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
                          Roles = evnt.Payload.Roles.ToHashSet(),
                          PersonName = evnt.Payload.PersonName,
                          PhoneNumber = evnt.Payload.PhoneNumber
                      };
            this.SaveUser(doc, evnt.EventTimeStamp, evnt.EventIdentifier.FormatGuid(), evnt.GlobalSequence);
        }

        public void Handle(IPublishedEvent<UserChanged> evnt)
        {
            UserDocument item = this.users.GetById(evnt.EventSourceId.FormatGuid());

            item.Email = evnt.Payload.Email;
            item.Password = evnt.Payload.PasswordHash;

            this.SaveUser(item, evnt.EventTimeStamp, evnt.EventIdentifier.FormatGuid(), evnt.GlobalSequence);
        }

        public void Handle(IPublishedEvent<UserLocked> evnt)
        {
            UserDocument item = this.users.GetById(evnt.EventSourceId);

            item.IsLockedByHQ = true;
            this.SaveUser(item, evnt.EventTimeStamp, evnt.EventIdentifier.FormatGuid(), evnt.GlobalSequence);
        }

        public void Handle(IPublishedEvent<UserUnlocked> evnt)
        {
            UserDocument item = this.users.GetById(evnt.EventSourceId);

            item.IsLockedByHQ = false;
            this.SaveUser(item, evnt.EventTimeStamp, evnt.EventIdentifier.FormatGuid(), evnt.GlobalSequence);
        }

        public void Handle(IPublishedEvent<UserLockedBySupervisor> evnt)
        {
            UserDocument item = this.users.GetById(evnt.EventSourceId);

            item.IsLockedBySupervisor = true;
            this.SaveUser(item, evnt.EventTimeStamp, evnt.EventIdentifier.FormatGuid(), evnt.GlobalSequence);
        }

        public void Handle(IPublishedEvent<UserUnlockedBySupervisor> evnt)
        {
            UserDocument item = this.users.GetById(evnt.EventSourceId);

            item.IsLockedBySupervisor = false;
            this.SaveUser(item, evnt.EventTimeStamp, evnt.EventIdentifier.FormatGuid(), evnt.GlobalSequence);
        }

        private void SaveUser(UserDocument user, DateTime timestamp, string packageId, long globalSequence)
        {
            if (!user.Roles.Contains(UserRoles.Operator))
            {
                return;
            }

            string content = this.serializer.Serialize(user, TypeSerializationSettings.ObjectsOnly);

            var syncPackageMeta = new UserSyncPackageMeta(user.PublicKey, timestamp)
            {
                Content = content,
                PackageId = packageId,
                SortIndex = globalSequence
            };

            syncPackageWriter.Store(syncPackageMeta, packageId);
        }
    }
}