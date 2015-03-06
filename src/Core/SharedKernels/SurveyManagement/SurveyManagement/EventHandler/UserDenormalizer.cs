using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.User;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Storage.Postgre.Implementation;
using WB.Core.SharedKernels.DataCollection.Events.User;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    public class UserDenormalizer : BaseDenormalizer,
                                    IEventHandler<NewUserCreated>,
                                    IEventHandler<UserChanged>,
                                    IEventHandler<UserLocked>,
                                    IEventHandler<UserUnlocked>,
                                    IEventHandler<UserLockedBySupervisor>,
                                    IEventHandler<UserUnlockedBySupervisor>,
                                    IEventHandler<UserLinkedToDevice>,
                                    IEventHandler
    {
        private readonly IReadSideRepositoryWriter<UserDocument> users;

        public UserDenormalizer(PostgreReadSideRepository<UserDocument> users)
        {
            this.users = users;
        }

        public override object[] Writers
        {
            get { return new object[] { users }; }
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
        }

        public void Handle(IPublishedEvent<UserChanged> evnt)
        {
            UserDocument item = this.users.GetById(evnt.EventSourceId);

            item.Email = evnt.Payload.Email;
            item.Roles = evnt.Payload.Roles.ToList();
            item.Password = evnt.Payload.PasswordHash;
            this.users.Store(item, item.PublicKey);
        }

        public void Handle(IPublishedEvent<UserLocked> @event)
        {
            UserDocument item = this.users.GetById(@event.EventSourceId);

            item.IsLockedByHQ = true;
            this.users.Store(item, item.PublicKey);
        }

        public void Handle(IPublishedEvent<UserUnlocked> @event)
        {
            UserDocument item = this.users.GetById(@event.EventSourceId);

            item.IsLockedByHQ = false;
            this.users.Store(item, item.PublicKey);
        }

        public void Handle(IPublishedEvent<UserLockedBySupervisor> evnt)
        {
            UserDocument item = this.users.GetById(evnt.EventSourceId);

            item.IsLockedBySupervisor = true;
            this.users.Store(item, item.PublicKey);
        }

        public void Handle(IPublishedEvent<UserUnlockedBySupervisor> evnt)
        {
            UserDocument item = this.users.GetById(evnt.EventSourceId);

            item.IsLockedBySupervisor = false;
            this.users.Store(item, item.PublicKey);
        }

        public void Handle(IPublishedEvent<UserLinkedToDevice> evnt)
        {
            UserDocument item = this.users.GetById(evnt.EventSourceId);

            item.DeviceId = evnt.Payload.DeviceId;
            item.DeviceChangingHistory.Add(
                new DeviceInfo { Date = evnt.EventTimeStamp, DeviceId = evnt.Payload.DeviceId });
            this.users.Store(item, item.PublicKey);
        }
    }
}