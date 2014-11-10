using System;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.Synchronization;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    public class QuestionnaireDenormalizer : BaseDenormalizer, IEventHandler<TemplateImported>, IEventHandler<PlainQuestionnaireRegistered>, 
        IEventHandler<QuestionnaireDeleted>, IEventHandler<QuestionnaireAssemblyImported>
    {
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned> documentStorage;
        private readonly ISynchronizationDataStorage synchronizationDataStorage;
        private readonly IQuestionnaireCacheInitializer questionnaireCacheInitializer;
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;
        private readonly IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor;

        public QuestionnaireDenormalizer(
            IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned> documentStorage, 
            ISynchronizationDataStorage synchronizationDataStorage,
            IQuestionnaireCacheInitializer questionnaireCacheInitializer,
            IPlainQuestionnaireRepository plainQuestionnaireRepository, 
            IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor)
        {
            this.documentStorage = documentStorage;
            this.synchronizationDataStorage = synchronizationDataStorage;
            this.questionnaireCacheInitializer = questionnaireCacheInitializer;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;

            this.questionnareAssemblyFileAccessor = questionnareAssemblyFileAccessor;
        }

        public override object[] Writers
        {
            get { return new object[] { documentStorage, synchronizationDataStorage }; }
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            Guid id = evnt.EventSourceId;
            long version = evnt.Payload.Version ?? evnt.EventSequence;
            QuestionnaireDocument questionnaireDocument = evnt.Payload.Source;

            this.StoreQuestionnaire(id, version, questionnaireDocument, evnt.Payload.AllowCensusMode, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<PlainQuestionnaireRegistered> evnt)
        {
            Guid id = evnt.EventSourceId;
            long version = evnt.Payload.Version;
            QuestionnaireDocument questionnaireDocument = this.plainQuestionnaireRepository.GetQuestionnaireDocument(id, version);

            this.StoreQuestionnaire(id, version, questionnaireDocument, evnt.Payload.AllowCensusMode, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<QuestionnaireDeleted> evnt)
        {
            this.documentStorage.Remove(evnt.EventSourceId, evnt.Payload.QuestionnaireVersion);
            this.questionnareAssemblyFileAccessor.RemoveAssembly(evnt.EventSourceId, evnt.Payload.QuestionnaireVersion);

            this.synchronizationDataStorage.DeleteQuestionnaire(evnt.EventSourceId, evnt.Payload.QuestionnaireVersion, evnt.EventTimeStamp);
        }

        private void StoreQuestionnaire(Guid id, long version, QuestionnaireDocument questionnaireDocument, bool allowCensusMode, DateTime timestamp)
        {
            var document = questionnaireDocument.Clone() as QuestionnaireDocument;
            if (document == null)
                return;

            this.questionnaireCacheInitializer.InitializeQuestionnaireDocumentWithCaches(document);

            this.documentStorage.Store(
                new QuestionnaireDocumentVersioned() { Questionnaire = document, Version = version },
                id);

            this.synchronizationDataStorage.SaveQuestionnaire(document, version, allowCensusMode, timestamp);
            
        }

        public void Handle(IPublishedEvent<QuestionnaireAssemblyImported> evnt)
        {
            var assemblyAsBase64String = this.questionnareAssemblyFileAccessor.GetAssemblyAsBase64String(evnt.EventSourceId, evnt.Payload.Version);

            this.synchronizationDataStorage.SaveTemplateAssembly(evnt.EventSourceId, evnt.Payload.Version, assemblyAsBase64String, evnt.EventTimeStamp);
        }
    }
}