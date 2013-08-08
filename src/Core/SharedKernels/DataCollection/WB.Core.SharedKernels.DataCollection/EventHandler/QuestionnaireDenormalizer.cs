using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Synchronization;

namespace WB.Core.SharedKernels.DataCollection.EventHandler
{
    public class QuestionnaireDenormalizer : IEventHandler<TemplateImported>
    {
        private readonly IReadSideRepositoryWriter<QuestionnaireDocument> documentStorage;
        private readonly ISynchronizationDataStorage synchronizationDataStorage;
        public QuestionnaireDenormalizer(IReadSideRepositoryWriter<QuestionnaireDocument> documentStorage, ISynchronizationDataStorage synchronizationDataStorage)
        {
            this.documentStorage = documentStorage;
            this.synchronizationDataStorage = synchronizationDataStorage;
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            var document = evnt.Payload.Source.Clone() as QuestionnaireDocument;
            if(document==null)
                return;
            this.documentStorage.Store(document, document.PublicKey);
            this.synchronizationDataStorage.SaveQuestionnaire(document);
        }
    }
}