using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Web.App_Start;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Grouped;

namespace RavenQuestionnaire.Core.Events
{
    public interface IEventSync
    {
        /// <summary>
        /// return list of ALL events grouped by aggregate root, please use very carefully
        /// </summary>
        /// <returns></returns>
        IEnumerable<AggregateRootEvent> ReadEvents();
        void WriteEvents(IEnumerable<AggregateRootEvent> stream);
  //      AggregateRootEventStream ReadEventStream(Guid eventSurceId);
       // IEnumerable<AggregateRootEventStream> ReadCompleteQuestionare();
    }

    public static class EventSyncExtensions
    {
        public static IEnumerable<IEnumerable<AggregateRootEvent>> ReadEventsByChunks(this IEventSync source, int chunksize = 2048)
        {
            var events = source.ReadEvents();
            
            return events.Chunk(chunksize,
                                (e, previous) =>
                                    {
                                        return e.CommitId == previous.CommitId;
                                    });
        }
    }

    public abstract class AbstractEventSync : IEventSync
    {
        private IEventStore eventStore;
        public AbstractEventSync()
        {
            eventStore = NcqrsEnvironment.Get<IEventStore>();
            if (eventStore == null)
                throw new Exception("IEventStore is not properly initialized.");
        }

        #region Implementation of IEventSync

        public abstract IEnumerable<AggregateRootEvent> ReadEvents();
        public void WriteEvents(IEnumerable<AggregateRootEvent> stream)
        {
            var uncommitedStreams = BuildEventStreams(stream);

            if (!uncommitedStreams.Any())
                return;

            var myEventBus = NcqrsEnvironment.Get<IEventBus>();
            if (myEventBus == null)
                throw new Exception("IEventBus is not properly initialized.");

            foreach (UncommittedEventStream uncommittedEventStream in uncommitedStreams)
            {
                if (!uncommittedEventStream.Any()) continue;
                
                eventStore.Store(uncommittedEventStream);
                myEventBus.Publish(uncommittedEventStream);
            }
        }

        #endregion

        protected IEnumerable<UncommittedEventStream> BuildEventStreams(IEnumerable<AggregateRootEvent> stream)
        {
            return
                stream.GroupBy(x => x.EventSourceId).Select(
                    g => g.CreateUncommittedEventStream(eventStore.ReadFrom(g.Key, long.MinValue, long.MaxValue)));
        }
    }
}
