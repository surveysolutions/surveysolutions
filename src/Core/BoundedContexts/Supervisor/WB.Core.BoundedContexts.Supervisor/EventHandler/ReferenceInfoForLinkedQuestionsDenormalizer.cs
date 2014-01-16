using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;
using WB.Core.Infrastructure.FunctionalDenormalization;
using WB.Core.SharedKernels.DataCollection.ReadSide;

namespace WB.Core.BoundedContexts.Supervisor.EventHandler
{
    public class ReferenceInfoForLinkedQuestionsDenormalizer : IEventHandler, IEventHandler<TemplateImported>
    {
        private readonly IVersionedReadSideRepositoryWriter<ReferenceInfoForLinkedQuestions> questionnaires;

        public ReferenceInfoForLinkedQuestionsDenormalizer(
            IVersionedReadSideRepositoryWriter<ReferenceInfoForLinkedQuestions> questionnaires)
        {
            this.questionnaires = questionnaires;
        }

        public string Name
        {
            get { return GetType().Name; }
        }

        public Type[] UsesViews
        {
            get { return new Type[0]; }
        }

        public Type[] BuildsViews
        {
            get { return new[] {typeof (ReferenceInfoForLinkedQuestions)}; }
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            evnt.Payload.Source.ConnectChildrenWithParent();

            var referenceInfoForLinkedQuestions = new ReferenceInfoForLinkedQuestions(evnt.Payload.Source, evnt.EventSequence);

            questionnaires.Store(referenceInfoForLinkedQuestions, evnt.EventSourceId);
        }
    }
}
