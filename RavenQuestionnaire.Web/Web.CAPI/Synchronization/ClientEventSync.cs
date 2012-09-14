using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs;
using Ncqrs.Eventing.Storage;
using RavenQuestionnaire.Core;
using Main.Core.Entities.SubEntities;
using Main.Core.Events;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;

namespace Web.CAPI.Synchronization
{
    using Main.Core.Events;

    public class ClientEventSync : AbstractEventSync
    {
        private readonly IViewRepository viewRepository;
        private readonly IEventStore myEventStore;
        public ClientEventSync(IViewRepository viewRepository)
        {
            this.viewRepository = viewRepository;
            this.myEventStore = NcqrsEnvironment.Get<IEventStore>();
        }
        #region Overrides of AbstractEventSync

        public override IEnumerable<AggregateRootEvent> ReadEvents()
        {
            var myEventStore = NcqrsEnvironment.Get<IEventStore>();
            if (myEventStore == null)
                throw new Exception("IEventStore is not correct.");
            var model =
                viewRepository.Load<CompleteQuestionnaireBrowseInputModel, CompleteQuestionnaireBrowseView>(
                    new CompleteQuestionnaireBrowseInputModel());
            List<AggregateRootEvent> retval = new List<AggregateRootEvent>();
            if (model == null)
                return retval;
            foreach (var item in model.Items)
            {
                if (!SurveyStatus.IsStatusAllowCapiSync(item.Status))
                    continue;
                retval.AddRange(GetEventStreamById(item.CompleteQuestionnaireId));
            }
            // return retval;
            return retval.OrderBy(x => x.EventTimeStamp);
        }

        protected List<AggregateRootEvent> GetEventStreamById(Guid aggregateRootId)
        {
            var events = this.myEventStore.ReadFrom(aggregateRootId,
                                                  int.MinValue, int.MaxValue);
            return events.Select(e => new AggregateRootEvent(e)).ToList();
        }

        #endregion
    }
}
