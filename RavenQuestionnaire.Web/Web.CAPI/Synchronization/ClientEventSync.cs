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
            if (model == null)
                return retval;
            foreach (var item in model.Items)
            {
                if (!SurveyStatus.IsStatusAllowCapiSync(item.Status))
                    continue;
                GetEventStreamById(retval, item.CompleteQuestionnaireId);
            }
            // return retval;
            return retval.OrderBy(x => x.EventTimeStamp);
        }

        #endregion
    }
}
