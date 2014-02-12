using System;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Supervisor.Factories;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Capi.EventHandler
{
    public class QuestionnaireRosterStructureDenormalizer : IEventHandler<TemplateImported>
    {
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireRosterStructure> questionnries;
        private readonly IQuestionnaireRosterStructureFactory questionnaireRosterStructureFactory;

        public QuestionnaireRosterStructureDenormalizer(
            IVersionedReadSideRepositoryWriter<QuestionnaireRosterStructure> questionnries, IQuestionnaireRosterStructureFactory questionnaireRosterStructureFactory)
        {
            this.questionnries = questionnries;
            this.questionnaireRosterStructureFactory = questionnaireRosterStructureFactory;
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            this.questionnries.Store(questionnaireRosterStructureFactory.CreateQuestionnaireRosterStructure(evnt.Payload.Source, evnt.EventSequence), evnt.EventSourceId);
        }
    }
}
