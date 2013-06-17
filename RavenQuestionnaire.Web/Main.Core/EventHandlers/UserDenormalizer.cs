// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserDenormalizer.cs" company="The World Bank">
//   2012
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;

namespace Main.Core.EventHandlers
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Events.User;
    using Main.DenormalizerStorage;

    using Ncqrs.Eventing.ServiceModel.Bus;

    using System.Linq;

    /// <summary>
    /// The user denormalizer.
    /// </summary>
    public class UserDenormalizer : IEventHandler<NewUserCreated>, 
                                    IEventHandler<UserChanged>, 
                                    IEventHandler<UserLocked>,
                                    IEventHandler<UserUnlocked>
    {
        #region Constants and Fields

        /// <summary>
        /// The users.
        /// </summary>
        private readonly IReadSideRepositoryWriter<UserDocument> users;

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

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<NewUserCreated> evnt)
        {
            this.users.Store(
                new UserDocument
                    {
                        UserName = evnt.Payload.Name, 
                        Password = evnt.Payload.Password, 
                        PublicKey = evnt.Payload.PublicKey, 
                        CreationDate = DateTime.UtcNow, 
                        Email = evnt.Payload.Email, 
                        IsLocked = evnt.Payload.IsLocked, 
                        Supervisor = evnt.Payload.Supervisor, 
                        Roles = new List<UserRoles>(evnt.Payload.Roles)
                    }, 
                evnt.Payload.PublicKey);
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
        }

        public void Handle(IPublishedEvent<UserLocked> @event)
        {
            UserDocument item = this.users.GetById(@event.EventSourceId);

            item.IsLocked = true;
        }

        public void Handle(IPublishedEvent<UserUnlocked> @event)
        {
            UserDocument item = this.users.GetById(@event.EventSourceId);

            item.IsLocked = false;
        }

        #endregion
    }
}