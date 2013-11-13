using System;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.ServiceModel.Bus.ViewConstructorEventBus;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.SharedKernels.DataCollection.EventHandler
{
    public class QuestionnaireRosterStructureDenormalizer : IEventHandler, IEventHandler<TemplateImported>
    {
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireRosterStructure> questionnries;

        public QuestionnaireRosterStructureDenormalizer(
            IVersionedReadSideRepositoryWriter<QuestionnaireRosterStructure> questionnries)
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
            get { return new[] {typeof (QuestionnaireRosterStructure)}; }
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            this.questionnries.Store(new QuestionnaireRosterStructure(evnt.Payload.Source, evnt.EventSequence), evnt.EventSourceId);
        }
    }
}
