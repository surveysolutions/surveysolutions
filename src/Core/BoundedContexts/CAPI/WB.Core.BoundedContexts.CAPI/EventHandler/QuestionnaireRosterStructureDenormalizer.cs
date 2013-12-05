using System;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Capi.EventHandler
{
    public class QuestionnaireRosterStructureDenormalizer : IEventHandler<TemplateImported>
    {
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireRosterStructure> questionnries;

        public QuestionnaireRosterStructureDenormalizer(
            IVersionedReadSideRepositoryWriter<QuestionnaireRosterStructure> questionnries)
        {
            this.questionnries = questionnries;
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            this.questionnries.Store(new QuestionnaireRosterStructure(evnt.Payload.Source, evnt.EventSequence), evnt.EventSourceId);
        }
    }
}
