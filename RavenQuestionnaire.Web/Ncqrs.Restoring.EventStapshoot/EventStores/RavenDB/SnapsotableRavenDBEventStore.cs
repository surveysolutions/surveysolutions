// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SnapsotableRavenDBEventStore.cs" company="">
//   
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Ncqrs.Restoring.EventStapshoot.EventStores.RavenDB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Ncqrs.Eventing;
    using Ncqrs.Eventing.Storage.RavenDB;

    using Raven.Client;
    using Raven.Client.Document;
    using Raven.Client.Indexes;
    using Raven.Client.Linq;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class SnapsotableRavenDBEventStoreExtension
    {
        #region Public Methods and Operators

        /// <summary>
        /// The create index.
        /// </summary>
        /// <param name="documentStore">
        /// The document store.
        /// </param>
        public static void CreateIndex(this DocumentStore documentStore)
        {
            IndexCreation.CreateIndexes(typeof(UniqueEventsIndex).Assembly, documentStore);
        }

        /// <summary>
        /// The get all events.
        /// </summary>
        /// <param name="documentStore">
        /// The document store.
        /// </param>
        /// <returns>
        /// </returns>
        public static IEnumerable<CommittedEvent> GetAllEvents(this DocumentStore documentStore)
        {
            var retval = new List<StoredEvent>();
            List<UniqueEventsResults> aggregateRoots;
            using (IDocumentSession session = documentStore.OpenSession())
            {
                aggregateRoots =
                    session.Query<StoredEvent, UniqueEventsIndex>().AsProjection<UniqueEventsResults>().ToList();
            }

            foreach (UniqueEventsResults uniqueEventsResultse in aggregateRoots)
            {
                retval.AddRange(ReadByAggregateRootId(documentStore, uniqueEventsResultse.EventSourceId));
            }

            return retval.Select(RavenDBEventStore.ToComittedEvent);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The read by aggregate root id.
        /// </summary>
        /// <param name="documentStore">
        /// The document store.
        /// </param>
        /// <param name="aggreagateRootId">
        /// The aggreagate root id.
        /// </param>
        /// <returns>
        /// </returns>
        private static IQueryable<StoredEvent> ReadByAggregateRootId(DocumentStore documentStore, Guid aggreagateRootId)
        {
            IQueryable<StoredEvent> result = Enumerable.Empty<StoredEvent>().AsQueryable();
            long lastSnapshot = 0;
            using (IDocumentSession session = documentStore.OpenSession())
            {
                IDocumentQuery<StoredEvent> snapshoots =
                    session.Advanced.LuceneQuery<StoredEvent>().WaitForNonStaleResults().Where(
                        string.Format("Data.$type:*SnapshootLoaded* AND EventSourceId:{0}", aggreagateRootId));

                if (snapshoots.Any())
                {
                    lastSnapshot = snapshoots.Select(e => e.EventSequence).Max();
                }
            }

            int maxPageSize = 1024;
            int page = 0;
            while (true)
            {
                using (IDocumentSession session = documentStore.OpenSession())
                {
                    IQueryable<StoredEvent> chunk =
                        session.Query<StoredEvent>().Customize(x => x.WaitForNonStaleResults()).Skip(page * maxPageSize)
                            .Take(maxPageSize).Where(
                                e => e.EventSourceId == aggreagateRootId && e.EventSequence >= lastSnapshot);
                    if (!chunk.Any())
                    {
                        break;
                    }

                    result = result.Concat(chunk);
                    page++;
                }
            }

            return result;
        }

        #endregion
    }
}