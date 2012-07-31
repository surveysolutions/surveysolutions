using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Grouped;
using RavenQuestionnaire.Web.App_Start;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;

namespace RavenQuestionnaire.Core.Events
{
    public interface IEventSync
    {
        IEnumerable<AggregateRootEventStream> ReadEvents();
        void WriteEvents(IEnumerable<AggregateRootEventStream> stream);

        IEnumerable<AggregateRootEventStream> ReadCompleteQuestionare(IViewRepository viewRepository);
    }

    public class RavenEventSync : IEventSync
    {
        #region Implementation of IEventSync

        public IEnumerable<AggregateRootEventStream> ReadEvents()
        {
            var myEventStore = NcqrsEnvironment.Get<IEventStore>();

            if (myEventStore == null)
                throw new Exception("IEventStore is not correct.");
            return myEventStore.ReadByAggregateRoot().Select(c => new AggregateRootEventStream(c));
        }
        public IEnumerable<AggregateRootEventStream> ReadCompleteQuestionare(IViewRepository viewRepository)
        {
            
            var myEventStore = NcqrsEnvironment.Get<IEventStore>();
            if (myEventStore == null)
                throw new Exception("IEventStore is not correct.");
            
            var model =
                viewRepository.Load<CQGroupedBrowseInputModel, CQGroupedBrowseView>(new CQGroupedBrowseInputModel());
            List<Guid> completeIds = new List<Guid>();

            //var myEventStore = NcqrsEnvironment.Get<IEventStore>();
            foreach (CQGroupItem group in model.Groups)
            {
                foreach (CompleteQuestionnaireBrowseItem survey in group.Items)
                {
                    if (survey.Status.PublicId == SurveyStatus.Complete.PublicId) //export only completed Q
                        completeIds.Add(new Guid(survey.CompleteQuestionnaireId));
                }
            }
            return myEventStore.ReadByAggregateRoot().Select(c => new AggregateRootEventStream(c, completeIds));
        }
        public void WriteEvents(IEnumerable<AggregateRootEventStream> stream)
        {
            var eventStore = NcqrsEnvironment.Get<IEventStore>();
            if (eventStore == null)
                throw new Exception("IEventStore is not properly initialized.");
            //((InProcessEventBus)myEventBus).RegisterHandler();
            foreach (AggregateRootEventStream commitedEventStream in stream)
            {
                Guid commitId = Guid.NewGuid();
                var currentEventStore = eventStore.ReadFrom(commitedEventStream.SourceId,
                                                            commitedEventStream.FromVersion,
                                                            commitedEventStream.ToVersion);
                var uncommitedStream = new UncommittedEventStream(commitId);
                foreach (AggregateRootEvent committedEvent in commitedEventStream.Events)
                {
                    if (currentEventStore.Count(ce => ce.EventIdentifier == committedEvent.EventIdentifier) > 0)
                        continue;

                    uncommitedStream.Append(committedEvent.CreateUncommitedEvent());
                }
                eventStore.Store(uncommitedStream);
            }
            NCQRSInit.RebuildReadLayer();
        }

        #endregion
    }
}
