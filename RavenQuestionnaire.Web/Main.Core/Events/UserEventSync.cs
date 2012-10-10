// -----------------------------------------------------------------------
// <copyright file="UserEventSync.cs" company="WorldBank">
// 2012
// </copyright>
// -----------------------------------------------------------------------

using Main.DenormalizerStorage;

namespace Main.Core.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Main.Core.Documents;
    using Main.Core.Domain;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Events.User;

    using Ncqrs;
    using Ncqrs.Eventing.Storage;

    public interface IUserEventSync
    {
        IEnumerable<AggregateRootEvent> GetUsers(UserRoles? role);
    }

    public class UserEventSync : AbstractSnapshotableEventSync, IUserEventSync
    {
        #region Fields

        /// <summary>
        /// myEventStore object
        /// </summary>
        private readonly IEventStore myEventStore;

        /// <summary>
        /// ViewRepository  object
        /// </summary>
        private readonly IDenormalizer denormalizer;

        #endregion

        #region Constructor

        public UserEventSync(IDenormalizer denormalizer)
        {
            this.denormalizer = denormalizer;
            this.myEventStore = NcqrsEnvironment.Get<IEventStore>();
            if (this.myEventStore == null)
            {
                throw new Exception("IEventStore is not correct.");
            }
        }

        #endregion

        #region Methods

        public IEnumerable<AggregateRootEvent> GetUsers(UserRoles? role)
        {
            var retval = new List<AggregateRootEvent>();
            this.AddUsers(retval);
            if (role != null)
            {
                return retval.Where(aggregateRootEvent => (aggregateRootEvent.Payload as NewUserCreated).Roles.Where(t => t == role).Count() > 0).ToList();
            }
            return retval.OrderBy(x => x.EventTimeStamp).ToList();
        }

        /// <summary>
        /// Responsible for load and added users from database
        /// </summary>
        /// <param name="retval">
        /// The retval.
        /// </param>
        protected void AddUsers(List<AggregateRootEvent> retval)
        {
            IQueryable<UserDocument> model = this.denormalizer.Query<UserDocument>();
            foreach (UserDocument item in model)
            {
                retval.AddRange(base.GetEventStreamById(item.PublicKey, typeof(UserAR)));
            }
        }

        #endregion
    }
}
