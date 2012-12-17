﻿// -----------------------------------------------------------------------
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

    public class UserEventStreamReader : AbstractSnapshotableEventStreamReader, IUserEventSync
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

        public UserEventStreamReader(IDenormalizer denormalizer)
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
            var retval = ExtractUsers();
            if (role.HasValue)
            {
                return retval.Where(aggregateRootEvent =>
                        {
                            return
                                (aggregateRootEvent.Payload is UserBaseEvent) &&
                                (aggregateRootEvent.Payload as UserBaseEvent).IsAssignedRole(role.Value);
                        }
                    ).ToList();
            }
            return retval.OrderBy(x => x.EventTimeStamp).ToList();
        }

        /// <summary>
        /// Responsible for load and added users from database
        /// </summary>
        /// <param name="retval">
        /// The retval.
        /// </param>
        protected List<AggregateRootEvent> ExtractUsers()
        {
            var usersList = new List<AggregateRootEvent>();

            IQueryable<UserDocument> model = this.denormalizer.Query<UserDocument>();
            foreach (UserDocument item in model)
            {
                usersList.AddRange(base.GetEventStreamById(item.PublicKey, typeof(UserAR)));
            }

            return usersList;
        }

        #endregion

        #region Overrides of AbstractEventSync

        public override IEnumerable<AggregateRootEvent> ReadEvents()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
