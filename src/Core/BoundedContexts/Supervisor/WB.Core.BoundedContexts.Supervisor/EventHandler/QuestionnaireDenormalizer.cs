using System;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.ServiceModel.Bus.ViewConstructorEventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.Synchronization;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.BoundedContexts.Supervisor.EventHandler
{
    public class QuestionnaireDenormalizer : IEventHandler<TemplateImported>, IEventHandler
    {
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireDocument> documentStorage;
        private readonly ISynchronizationDataStorage synchronizationDataStorage;
        public QuestionnaireDenormalizer(IVersionedReadSideRepositoryWriter<QuestionnaireDocument> documentStorage, ISynchronizationDataStorage synchronizationDataStorage)
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
            this.documentStorage.Store(document, document.PublicKey,evnt.EventSequence);
            this.synchronizationDataStorage.SaveQuestionnaire(document);
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
            get { return new Type[] { typeof(QuestionnaireDocument), typeof(SynchronizationDelta) }; }
        }
    }
}