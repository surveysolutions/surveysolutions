// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserEventStreamReader.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Domain;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Events.User;
    using Main.DenormalizerStorage;

    using Ncqrs;
    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The user event stream reader.
    /// </summary>
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

        public UserEventStreamReader(IDenormalizer denormalizer):base(denormalizer)
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
        protected List<AggregateRootEvent> ExtractUsers()
        {
            var usersList = new List<AggregateRootEvent>();

            IQueryable<UserDocument> model = this.denormalizer.Query<UserDocument>();
            foreach (UserDocument item in model)
            {
                usersList.AddRange(base.GetEventStreamById<UserAR>(item.PublicKey));
            }

            return usersList;
        }

        #endregion

        #region Overrides of AbstractEventSync

        public override IEnumerable<AggregateRootEvent> ReadEvents()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<SyncItemsMeta> GetAllARIds()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<AggregateRootEvent> GetARById(Guid ARId, string ARType, Guid? startFrom)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
