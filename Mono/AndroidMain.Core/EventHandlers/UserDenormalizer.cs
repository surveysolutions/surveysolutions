// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserDenormalizer.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The user denormalizer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Main.Core.Denormalizers;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.User;
using Ncqrs.Eventing.ServiceModel.Bus;

namespace Main.Core.EventHandlers
{
    /// <summary>
    /// The user denormalizer.
    /// </summary>
    public class UserDenormalizer : IEventHandler<NewUserCreated>, 
                                    IEventHandler<UserChanged>, 
                                    IEventHandler<UserStatusChanged>
    {
        #region Fields

        /// <summary>
        /// The users.
        /// </summary>
        private readonly IDenormalizerStorage<UserDocument> users;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UserDenormalizer"/> class.
        /// </summary>
        /// <param name="users">
        /// The users.
        /// </param>
        public UserDenormalizer(IDenormalizerStorage<UserDocument> users)
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
            UserDocument item = this.users.GetByGuid(evnt.EventSourceId);

            item.Email = evnt.Payload.Email;
            item.IsLocked = evnt.Payload.IsLocked;
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<UserStatusChanged> evnt)
        {
            UserDocument item = this.users.GetByGuid(evnt.EventSourceId);

            item.IsLocked = evnt.Payload.IsLocked;
        }

        #endregion
    }
}