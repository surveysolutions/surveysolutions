using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.User;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Team.EventHandlers
{
    public class UserDenormalizer : BaseDenormalizer,
                                    IEventHandler<NewUserCreated>,
                                    IEventHandler<UserChanged>,
                                    IEventHandler<UserLocked>,
                                    IEventHandler<UserUnlocked>, 
                                    IEventHandler
    {
        private readonly IReadSideRepositoryWriter<UserDocument> users;

        public UserDenormalizer(IReadSideRepositoryWriter<UserDocument> users)
        {
            this.users = users;
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
                    IsLocked = evnt.Payload.IsLocked,
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

            item.IsLocked = true;
            this.users.Store(item, item.PublicKey);
        }

        public void Handle(IPublishedEvent<UserUnlocked> @event)
        {
            UserDocument item = this.users.GetById(@event.EventSourceId);

            item.IsLocked = false;
            this.users.Store(item, item.PublicKey);
        }

        public override Type[] BuildsViews
        {
            get { return new Type[] {typeof (UserDocument)}; }
        }
    }
}