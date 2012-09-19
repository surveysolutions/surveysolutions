// -----------------------------------------------------------------------
// <copyright file="SnapsotableRavenDBEventStore.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.Linq.Expressions;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage.RavenDB;
using Raven.Client.Document;
using Raven.Client.Indexes;
using Raven.Client.Linq;

namespace Ncqrs.Restoring.EventStapshoot.EventStores.RavenDB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class SnapsotableRavenDBEventStoreExtension
    {
        public static void CreateIndex(this DocumentStore documentStore)
        {
            IndexCreation.CreateIndexes(typeof(UniqueEventsIndex).Assembly, documentStore);
        }
        public static IEnumerable<CommittedEvent> GetAllEvents(this DocumentStore documentStore)
        {
            List<StoredEvent> retval = new List<StoredEvent>();
            List<UniqueEventsResults> aggregateRoots;
            using (var session = documentStore.OpenSession())
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
        static IQueryable<StoredEvent> ReadByAggregateRootId(DocumentStore documentStore,Guid aggreagateRootId)
        {
            IQueryable<StoredEvent> result = Enumerable.Empty<StoredEvent>().AsQueryable(); 
            long lastSnapshot = 0;
            using (var session = documentStore.OpenSession())
            {
                var snapshoots =
                    session.Advanced.LuceneQuery<StoredEvent>().WaitForNonStaleResults().Where(
                        string.Format("Data.$type:*SnapshootLoaded* AND EventSourceId:{0}", aggreagateRootId));

                if (snapshoots.Any())
                    lastSnapshot = snapshoots.Select(e => e.EventSequence).Max();
            }
            int maxPageSize = 1024;
            int page = 0;
            while (true)
            {
                using (var session = documentStore.OpenSession())
                {
                    var chunk =
                        session.Query<StoredEvent>().Customize(x => x.WaitForNonStaleResults()).
                            Skip(page*maxPageSize).
                            Take(maxPageSize).Where(
                                e => e.EventSourceId == aggreagateRootId && e.EventSequence >= lastSnapshot);
                    if (!chunk.Any())
                        break;

                    result = result.Concat(chunk);
                    page++;
                }
            }
            return result;
        }

    }
}
