using System;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.ServiceModel.Bus.ViewConstructorEventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.SharedKernels.DataCollection.EventHandler
{
    public class QuestionnaireDenormalizer : IEventHandler<TemplateImported>, IEventHandler
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
            get { return new Type[] { typeof(QuestionnaireDocument) }; }
        }
    }
}