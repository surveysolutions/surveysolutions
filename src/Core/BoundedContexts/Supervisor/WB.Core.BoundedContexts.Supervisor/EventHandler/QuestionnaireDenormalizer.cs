using System;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.FunctionalDenormalization;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Factories;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.Synchronization;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.BoundedContexts.Supervisor.EventHandler
{
    public class QuestionnaireDenormalizer : IEventHandler<TemplateImported>, IEventHandler
    {
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned> documentStorage;
        private readonly ISynchronizationDataStorage synchronizationDataStorage;
        private readonly IQuestionnaireCacheInitializer questionnaireCacheInitializer;

        public QuestionnaireDenormalizer(
            IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned> documentStorage, 
            ISynchronizationDataStorage synchronizationDataStorage,
            IQuestionnaireCacheInitializer questionnaireCacheInitializer)
        {
            this.documentStorage = documentStorage;
            this.synchronizationDataStorage = synchronizationDataStorage;
            this.questionnaireCacheInitializer = questionnaireCacheInitializer;
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            var document = evnt.Payload.Source.Clone() as QuestionnaireDocument;
            if (document == null)
                return;

            this.questionnaireCacheInitializer.InitializeQuestionnaireDocumentWithCaches(document);

            this.documentStorage.Store(
                new QuestionnaireDocumentVersioned() { Questionnaire = document, Version = evnt.EventSequence },
                document.PublicKey);

            this.synchronizationDataStorage.SaveQuestionnaire(document, evnt.EventSequence);
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