using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.ServiceModel.Bus.ViewConstructorEventBus;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.SharedKernels.DataCollection.EventHandler
{
    public class QuestionnairePropagationStructureDenormalizer : IEventHandler, IEventHandler<TemplateImported>
    {
        private readonly IVersionedReadSideRepositoryWriter<QuestionnairePropagationStructure> questionnries;

        public QuestionnairePropagationStructureDenormalizer(
            IVersionedReadSideRepositoryWriter<QuestionnairePropagationStructure> questionnries)
        {
            this.questionnries = questionnries;
        }

        public string Name
        {
            get { return this.GetType().Name; }
        }

        public Type[] UsesViews
        {
            get { return new Type[0]; }
        }

        public Type[] BuildsViews
        {
            get { return new[] {typeof (QuestionnairePropagationStructure)}; }
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            this.questionnries.Store(new QuestionnairePropagationStructure(evnt.Payload.Source, evnt.EventSequence), evnt.EventSourceId);
        }
    }
}
