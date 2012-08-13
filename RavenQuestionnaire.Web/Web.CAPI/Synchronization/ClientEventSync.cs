using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public override IEnumerable<AggregateRootEventStream> ReadEvents()
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
        }

        #endregion
    }
}
