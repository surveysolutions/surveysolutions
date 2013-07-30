using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.SharedKernels.DataCollection.EventHandler
{
    public class QuestionnaireDenormalizer : IEventHandler<TemplateImported>
    {
        private readonly IReadSideRepositoryWriter<QuestionnaireDocument> documentStorage;

        public QuestionnaireDenormalizer(IReadSideRepositoryWriter<QuestionnaireDocument> documentStorage)
        {
            this.documentStorage = documentStorage;
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            var document = evnt.Payload.Source;

            this.documentStorage.Store(document.Clone() as QuestionnaireDocument, document.PublicKey);
        }
    }
}