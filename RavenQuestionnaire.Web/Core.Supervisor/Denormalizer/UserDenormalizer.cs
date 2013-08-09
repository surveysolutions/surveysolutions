using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.User;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Synchronization;

namespace Core.Supervisor.Denormalizer
{
    /// <summary>
    /// The user denormalizer.
    /// </summary>
    public class UserDenormalizer : IEventHandler<NewUserCreated>, 
                                    IEventHandler<UserChanged>, 
                                    IEventHandler<UserLocked>,
                                    IEventHandler<UserUnlocked>
    {
        #region Constants and Fields

        private readonly IReadSideRepositoryWriter<UserDocument> users;
        private readonly ISynchronizationDataStorage syncStorage;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UserDenormalizer"/> class.
        /// </summary>
        /// <param name="users">
        /// The users.
        /// </param>
        public UserDenormalizer(IReadSideRepositoryWriter<UserDocument> users)
        {
            this.users = users;
        }

        #endregion

        #region Public Methods and Operators

        public UserDenormalizer(IReadSideRepositoryWriter<UserDocument> users, ISynchronizationDataStorage syncStorage)
        {
            this.users = users;
            this.syncStorage = syncStorage;
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
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
            this.users.Store(doc
               , 
                evnt.Payload.PublicKey);

            syncStorage.SaveUser(doc);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<UserChanged> evnt)
        {
            UserDocument item = this.users.GetById(evnt.EventSourceId);

            item.Email = evnt.Payload.Email;
            item.Roles = evnt.Payload.Roles.ToList();
            users.Store(item, item.PublicKey);
        }

        public void Handle(IPublishedEvent<UserLocked> @event)
        {
            UserDocument item = this.users.GetById(@event.EventSourceId);

            item.IsLocked = true;
            users.Store(item,item.PublicKey);
        }

        public void Handle(IPublishedEvent<UserUnlocked> @event)
        {
            UserDocument item = this.users.GetById(@event.EventSourceId);

            item.IsLocked = false;
            users.Store(item, item.PublicKey);
        }

        #endregion
    }
}