using System;
using System.Collections.Generic;
using Ncqrs.Eventing.ServiceModel.Bus;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Events.User;

namespace RavenQuestionnaire.Core.EventHandlers
{
    public class UserDenormalizer : IEventHandler<NewUserCreated>, 
        IEventHandler<UserChanged>,
        IEventHandler<UserStatusChanged>
    {
        private IDenormalizerStorage<UserDocument> users;
        public UserDenormalizer(IDenormalizerStorage<UserDocument> users)
        {
            this.users = users;
        }


        #region Implementation of IEventHandler<in NewUserCreated>

        public void Handle(IPublishedEvent<NewUserCreated> evnt)
        {
            users.Store(
                new UserDocument()
                    {
                        UserName = evnt.Payload.Name,
                        Password = evnt.Payload.Password,
                        PublicKey = evnt.Payload.PublicKey,
                        CreationDate =  DateTime.UtcNow,
                        Email = evnt.Payload.Email,
                        IsLocked = evnt.Payload.IsLocked,
                        Supervisor = evnt.Payload.Supervisor,
                        Roles = new List<UserRoles>(evnt.Payload.Roles)
                    }, evnt.Payload.PublicKey);
        }

        #endregion

        #region Implementation of IEventHandler<in UserChanged>

        public void Handle(IPublishedEvent<UserChanged> evnt)
        {
            var item = this.users.GetByGuid(evnt.Payload.PublicKey);

            item.Email = evnt.Payload.Email;
            item.IsLocked = evnt.Payload.IsLocked;
            
        }

        #endregion

        public void Handle(IPublishedEvent<UserStatusChanged> evnt)
        {
            var item = this.users.GetByGuid(evnt.EventSourceId);

            item.IsLocked = evnt.Payload.IsLocked;
        }
    }
}
