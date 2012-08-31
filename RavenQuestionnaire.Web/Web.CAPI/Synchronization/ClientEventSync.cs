using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs;
using Ncqrs.Eventing.Storage;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Events;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;

namespace Web.CAPI.Synchronization
{
    public class ClientEventSync : AbstractEventSync
    {
        private readonly IViewRepository viewRepository;
        public ClientEventSync(IViewRepository viewRepository)
        {
            this.viewRepository = viewRepository;
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
            foreach (var item in model.Items)
            {
                if (item.Status.Name != SurveyStatus.Complete.Name)
                    continue;
                var events = myEventStore.ReadFrom(item.CompleteQuestionnaireId,
                                                   int.MinValue, int.MaxValue);
                retval.AddRange(events.Select(e => new AggregateRootEvent(e)));
                /* retval.Add(
                    new AggregateRootEventStream(myEventStore.ReadFrom(Guid.Parse(item.CompleteQuestionnaireId),
                                                                       int.MinValue, int.MaxValue));*/
            }
            // return retval;
            return retval.OrderBy(x => x.EventTimeStamp);
        }

        #endregion
    }
}
