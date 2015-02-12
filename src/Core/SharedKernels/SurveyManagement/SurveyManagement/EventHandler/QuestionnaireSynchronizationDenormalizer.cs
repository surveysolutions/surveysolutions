using System;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.RepositoryAccessors;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    internal class QuestionnaireSynchronizationDenormalizer : BaseDenormalizer,
        IEventHandler<QuestionnaireDeleted>,
        IEventHandler<QuestionnaireAssemblyImported>,
        IEventHandler<TemplateImported>,
        IEventHandler<PlainQuestionnaireRegistered>
    {
        private readonly IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor;
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;
        private readonly IJsonUtils jsonUtils;
        private readonly IOrderableSyncPackageWriter<QuestionnaireSyncPackage> questionnairePackageStorageWriter;

        public QuestionnaireSynchronizationDenormalizer(
            IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor,
            IPlainQuestionnaireRepository plainQuestionnaireRepository, 
            IJsonUtils jsonUtils,
            IOrderableSyncPackageWriter<QuestionnaireSyncPackage> questionnairePackageStorageWriter)
        {
            this.questionnareAssemblyFileAccessor = questionnareAssemblyFileAccessor;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.jsonUtils = jsonUtils;
            this.questionnairePackageStorageWriter = questionnairePackageStorageWriter;
        }

        public override object[] Writers
        {
            get { return new object[] { this.questionnairePackageStorageWriter }; }
        }

        public override object[] Readers
        {
            get { return new object[] { }; }
        }

        public void Handle(IPublishedEvent<QuestionnaireDeleted> evnt)
        {
            Guid questionnaireId = evnt.EventSourceId;
            long questionnaireVersion = evnt.Payload.QuestionnaireVersion;
            var questionnaireMetadata = new QuestionnaireMetadata(questionnaireId, questionnaireVersion, false);
            
            this.StoreChunk(questionnaireId, questionnaireVersion, SyncItemType.DeleteQuestionnaire, questionnaireId.ToString(), this.GetItemAsContent(questionnaireMetadata), evnt.EventTimeStamp, evnt.EventSequence);
        }

        public void Handle(IPublishedEvent<QuestionnaireAssemblyImported> evnt)
        {
            string assemblyAsBase64String = this.questionnareAssemblyFileAccessor.GetAssemblyAsBase64String(evnt.EventSourceId, evnt.Payload.Version);
            Guid publicKey = evnt.EventSourceId;
            long version = evnt.Payload.Version;
            var meta = new QuestionnaireAssemblyMetadata(publicKey, version);

            this.StoreChunk(evnt.EventSourceId, version, SyncItemType.QuestionnaireAssembly, assemblyAsBase64String, this.GetItemAsContent(meta), evnt.EventTimeStamp, evnt.EventSequence);
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            this.SaveQuestionnaire(evnt.Payload.Source, evnt.Payload.Version ?? evnt.EventSequence, evnt.Payload.AllowCensusMode,
                evnt.EventTimeStamp, evnt.EventSequence);
        }

        public void Handle(IPublishedEvent<PlainQuestionnaireRegistered> evnt)
        {
            QuestionnaireDocument questionnaireDocument = this.plainQuestionnaireRepository.GetQuestionnaireDocument(evnt.EventSourceId, evnt.Payload.Version);
            this.SaveQuestionnaire(questionnaireDocument, evnt.Payload.Version, evnt.Payload.AllowCensusMode, evnt.EventTimeStamp, evnt.EventSequence);
        }

        public void SaveQuestionnaire(QuestionnaireDocument questionnaireDocument, long version, bool allowCensusMode, DateTime timestamp, long eventSuquence)
        {
            questionnaireDocument.IsDeleted = false;

            var questionnaireMetadata = new QuestionnaireMetadata(questionnaireDocument.PublicKey, version, allowCensusMode);

            this.StoreChunk(questionnaireDocument.PublicKey, 
                version, 
                SyncItemType.Questionnaire, 
                this.GetItemAsContent(questionnaireDocument), 
                this.GetItemAsContent(questionnaireMetadata), 
                timestamp,
                eventSuquence);
        }

        public void StoreChunk(Guid questionnaireId, long questionnaireVersion, string itemType, string content, string metaInfo, DateTime timestamp, long eventSuquence)
        {
            long sortIndex = questionnairePackageStorageWriter.GetNextOrder();

            var synchronizationDelta = new QuestionnaireSyncPackage(
                questionnaireId, 
                questionnaireVersion,
                itemType, 
                content, 
                metaInfo, sortIndex, timestamp);

            this.questionnairePackageStorageWriter.Store(synchronizationDelta, synchronizationDelta.PackageId);
        }

        protected string GetItemAsContent(object item)
        {
            return this.jsonUtils.Serialize(item, TypeSerializationSettings.AllTypes);
        }
    }
}