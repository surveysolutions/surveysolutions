// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserDenormalizer.cs" company="The World Bank">
//   2012
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using Main.DenormalizerStorage;
using Ncqrs.Eventing.ServiceModel.Bus;

namespace Designer.Web.Providers.CQRS
{
    /// <summary>
    /// The user denormalizer.
    /// </summary>
    public class AccountDenormalizer : IEventHandler<AccountCreated>, 
                                    IEventHandler<AccountChanged>, 
                                    IEventHandler<AccountDeleted>,
                                    IEventHandler<AccountRegistered>
    {
        #region Constants and Fields

        /// <summary>
        /// The users.
        /// </summary>
        private readonly IDenormalizerStorage<AccountDocument> _accounts;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountDenormalizer"/> class.
        /// </summary>
        /// <param name="accounts">
        /// The users.
        /// </param>
        public AccountDenormalizer(IDenormalizerStorage<AccountDocument> accounts)
        {
            _accounts = accounts;
        }

        #endregion

        //#region Public Methods and Operators

        ///// <summary>
        ///// The handle.
        ///// </summary>
        ///// <param name="evnt">
        ///// The evnt.
        ///// </param>
        //public void Handle(IPublishedEvent<NewUserCreated> evnt)
        //{
        //    this._accounts.Store(
        //        new UserDocument
        //            {
        //                UserName = evnt.Payload.Name, 
        //                Password = evnt.Payload.Password, 
        //                PublicKey = evnt.Payload.PublicKey, 
        //                CreationDate = DateTime.UtcNow, 
        //                Email = evnt.Payload.Email, 
        //                IsLocked = evnt.Payload.IsLocked, 
        //                Supervisor = evnt.Payload.Supervisor, 
        //                Roles = new List<UserRoles>(evnt.Payload.Roles)
        //            }, 
        //        evnt.Payload.PublicKey);
        //}

        ///// <summary>
        ///// The handle.
        ///// </summary>
        ///// <param name="evnt">
        ///// The evnt.
        ///// </param>
        //public void Handle(IPublishedEvent<UserChanged> evnt)
        //{
        //    UserDocument item = this._accounts.GetByGuid(evnt.EventSourceId);

        //    item.Email = evnt.Payload.Email;
        //    item.Roles = evnt.Payload.Roles.ToList();
        //}

        //public void Handle(IPublishedEvent<UserLocked> @event)
        //{
        //    UserDocument item = this._accounts.GetByGuid(@event.EventSourceId);

        //    item.IsLocked = true;
        //}

        //public void Handle(IPublishedEvent<UserUnlocked> @event)
        //{
        //    UserDocument item = this._accounts.GetByGuid(@event.EventSourceId);

        //    item.IsLocked = false;
        //}

        //#endregion

        public void Handle(IPublishedEvent<AccountCreated> @event)
        {
            //    this._accounts.Store(
            //        new UserDocument
            //            {
            //                UserName = evnt.Payload.Name, 
            //                Password = evnt.Payload.Password, 
            //                PublicKey = evnt.Payload.PublicKey, 
            //                CreationDate = DateTime.UtcNow, 
            //                Email = evnt.Payload.Email, 
            //                IsLocked = evnt.Payload.IsLocked, 
            //                Supervisor = evnt.Payload.Supervisor, 
            //                Roles = new List<UserRoles>(evnt.Payload.Roles)
            //            }, 
            //        evnt.Payload.PublicKey);
        }

        public void Handle(IPublishedEvent<AccountChanged> @event)
        {
            AccountDocument item = _accounts.GetByGuid(@event.EventSourceId);

            
        }

        public void Handle(IPublishedEvent<AccountDeleted> @event)
        {
            _accounts.Remove(@event.Payload.PublicKey);
        }

        public void Handle(IPublishedEvent<AccountRegistered> @event)
        {
            throw new NotImplementedException();
        }
    }
}