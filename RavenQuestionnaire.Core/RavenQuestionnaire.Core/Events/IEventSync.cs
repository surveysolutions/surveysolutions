using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs;
using Ncqrs.Eventing;
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
        public static IEnumerable<IEnumerable<AggregateRootEvent>> ReadEventsByChunks(this IEventSync source, int chunksize =2048)
        {
            return source.ReadEvents().Chunk(chunksize);
        }
    }

    public abstract class AbstractEventSync : IEventSync
    {
       /* private readonly IViewRepository viewRepository;
        public RavenEventSync(IViewRepository viewRepository)
        {
            this.viewRepository = viewRepository;
        }*/

        #region Implementation of IEventSync

        public abstract IEnumerable<AggregateRootEvent> ReadEvents();/*
        {
            var myEventStore = NcqrsEnvironment.Get<IEventStore>();

            if (myEventStore == null)
                throw new Exception("IEventStore is not correct.");
            var allEvents = myEventStore.ReadFrom(DateTime.MinValue);
            List<Guid> aggregateRootIds = allEvents.GroupBy(x=>x.EventSourceId).Select(x=>x.Key).ToList();
            List<AggregateRootEventStream> retval=new List<AggregateRootEventStream>(aggregateRootIds.Count);
            foreach (Guid aggregateRootId in aggregateRootIds)
            {
                Guid id = aggregateRootId;
                retval.Add(new AggregateRootEventStream(new CommittedEventStream(aggregateRootId, allEvents.Where(e => e.EventSourceId == id))));
            }
            return retval;
        }
        */
       /* public AggregateRootEventStream ReadEventStream(Guid eventSurceId)
        {
            var myEventStore = NcqrsEnvironment.Get<IEventStore>();
            if (myEventStore == null)
                throw new Exception("IEventStore is not correct.");
            return new AggregateRootEventStream(myEventStore.ReadFrom(eventSurceId,
                                                                      int.MinValue, int.MaxValue));
        }*/

      /*  public IEnumerable<AggregateRootEventStream> ReadCompleteQuestionare()
        {
            var myEventStore = NcqrsEnvironment.Get<IEventStore>();
            if (myEventStore == null)
                throw new Exception("IEventStore is not correct.");
            var model =
                viewRepository.Load<CompleteQuestionnaireBrowseInputModel, CompleteQuestionnaireBrowseView>(
                    new CompleteQuestionnaireBrowseInputModel());

            List<AggregateRootEventStream> retval = new List<AggregateRootEventStream>();
            foreach (var item in model.Items)
            {
                if (item.Status.Name != SurveyStatus.Complete.Name)
                    continue;
                retval.Add(
                    new AggregateRootEventStream(myEventStore.ReadFrom(Guid.Parse(item.CompleteQuestionnaireId),
                                                                       int.MinValue, int.MaxValue)));
            }
            // return retval;
            return retval;
        }*/

        public void WriteEvents(IEnumerable<AggregateRootEvent> stream)
        {
            var eventStore = NcqrsEnvironment.Get<IEventStore>();
            if (eventStore == null)
                throw new Exception("IEventStore is not properly initialized.");
            /*foreach (var eventItem in stream)
            {*/
            Guid commitId = Guid.NewGuid();
            var currentEventStore = eventStore.ReadFrom(DateTime.MinValue).ToList();
            var uncommitedStream = new UncommittedEventStream(commitId);
            foreach (AggregateRootEvent committedEvent in stream)
            {
                if (currentEventStore.Count(ce => ce.EventIdentifier == committedEvent.EventIdentifier) > 0)
                    continue;

                uncommitedStream.Append(committedEvent.CreateUncommitedEvent());
            }
            if (!uncommitedStream.Any())
                return;
            eventStore.Store(uncommitedStream);
            /*   }*/
            NCQRSInit.RebuildReadLayer();
        }

        #endregion
    }
}
