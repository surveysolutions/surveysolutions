using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.User;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.User;
using WB.Core.Synchronization;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    public class UserDenormalizer : BaseDenormalizer,
                                    IEventHandler<NewUserCreated>,
                                    IEventHandler<UserChanged>,
                                    IEventHandler<UserLocked>,
                                    IEventHandler<UserUnlocked>,
                                    IEventHandler<UserLockedBySupervisor>,
                                    IEventHandler<UserUnlockedBySupervisor>,
                                    IEventHandler
    {
        private readonly IReadSideRepositoryWriter<UserDocument> users;
        private readonly ISynchronizationDataStorage syncStorage;

        public UserDenormalizer(IReadSideRepositoryWriter<UserDocument> users)
        {
            this.users = users;
        }

        public UserDenormalizer(IReadSideRepositoryWriter<UserDocument> users, ISynchronizationDataStorage syncStorage)
        {
            this.users = users;
            this.syncStorage = syncStorage;
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
            this.users.Store(doc, evnt.Payload.PublicKey);

            this.syncStorage.SaveUser(doc, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<UserChanged> evnt)
        {
            UserDocument item = this.users.GetById(evnt.EventSourceId);

            item.Email = evnt.Payload.Email;
            item.Roles = evnt.Payload.Roles.ToList();
            item.Password = evnt.Payload.PasswordHash;
            this.users.Store(item, item.PublicKey);
            this.syncStorage.SaveUser(item, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<UserLocked> @event)
        {
            UserDocument item = this.users.GetById(@event.EventSourceId);

            item.IsLockedByHQ = true;
            this.users.Store(item, item.PublicKey);
            this.syncStorage.SaveUser(item, @event.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<UserUnlocked> @event)
        {
            UserDocument item = this.users.GetById(@event.EventSourceId);

            item.IsLockedByHQ = false;
            this.users.Store(item, item.PublicKey);
            this.syncStorage.SaveUser(item, @event.EventTimeStamp);
        }

        public override Type[] BuildsViews
        {
            get { return new Type[] {typeof (UserDocument), typeof (SynchronizationDelta)}; }
        }

        public void Handle(IPublishedEvent<UserLockedBySupervisor> evnt)
        {
            UserDocument item = this.users.GetById(evnt.EventSourceId);

            item.IsLockedBySupervisor = true;
            this.users.Store(item, item.PublicKey);
            this.syncStorage.SaveUser(item, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<UserUnlockedBySupervisor> evnt)
        {
            UserDocument item = this.users.GetById(evnt.EventSourceId);

            item.IsLockedBySupervisor = false;
            this.users.Store(item, item.PublicKey);
            this.syncStorage.SaveUser(item, evnt.EventTimeStamp);
        }
    }
}