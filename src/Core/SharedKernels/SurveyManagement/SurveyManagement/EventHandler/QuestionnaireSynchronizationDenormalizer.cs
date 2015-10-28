using System;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;

using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
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
        private readonly ISerializer serializer;
        private readonly IReadSideRepositoryWriter<QuestionnaireSyncPackageMeta> syncPackageWriter;

        public QuestionnaireSynchronizationDenormalizer(
            IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor,
            IPlainQuestionnaireRepository plainQuestionnaireRepository, 
            ISerializer serializer,
            IReadSideRepositoryWriter<QuestionnaireSyncPackageMeta> syncPackageWriter)
        {
            this.questionnareAssemblyFileAccessor = questionnareAssemblyFileAccessor;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.serializer = serializer;
            this.syncPackageWriter = syncPackageWriter;
        }

        public override object[] Writers
        {
            get { return new object[] { this.syncPackageWriter }; }
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
            
            this.StoreChunk(questionnaireId, 
                questionnaireVersion, 
                SyncItemType.DeleteQuestionnaire, 
                questionnaireId.ToString(), 
                this.GetItemAsContent(questionnaireMetadata), 
                evnt.EventTimeStamp,
                evnt.EventIdentifier.FormatGuid(), 
                evnt.GlobalSequence);
        }

        public void Handle(IPublishedEvent<QuestionnaireAssemblyImported> evnt)
        {
            string assemblyAsBase64String = this.questionnareAssemblyFileAccessor.GetAssemblyAsBase64String(evnt.EventSourceId, evnt.Payload.Version);
            Guid publicKey = evnt.EventSourceId;
            long version = evnt.Payload.Version;
            var meta = new QuestionnaireAssemblyMetadata(publicKey, version);

            this.StoreChunk(evnt.EventSourceId, 
                version, 
                SyncItemType.QuestionnaireAssembly, 
                assemblyAsBase64String, 
                this.GetItemAsContent(meta), 
                evnt.EventTimeStamp,
                evnt.EventIdentifier.FormatGuid(), 
                evnt.GlobalSequence);
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            this.SaveQuestionnaire(evnt.Payload.Source, 
                evnt.Payload.Version ?? evnt.EventSequence, 
                evnt.Payload.AllowCensusMode,
                evnt.EventTimeStamp,
                evnt.EventIdentifier.FormatGuid(),
                evnt.GlobalSequence);
        }

        public void Handle(IPublishedEvent<PlainQuestionnaireRegistered> evnt)
        {
            QuestionnaireDocument questionnaireDocument = this.plainQuestionnaireRepository.GetQuestionnaireDocument(evnt.EventSourceId, evnt.Payload.Version);
            this.SaveQuestionnaire(questionnaireDocument, 
                evnt.Payload.Version, 
                evnt.Payload.AllowCensusMode,
                evnt.EventTimeStamp,
                evnt.EventIdentifier.FormatGuid(), 
                evnt.GlobalSequence);
        }

        private void SaveQuestionnaire(QuestionnaireDocument questionnaireDocument, long version, bool allowCensusMode, DateTime timestamp, string packageId, long globalSequence)
        {
            questionnaireDocument.IsDeleted = false;

            var questionnaireMetadata = new QuestionnaireMetadata(questionnaireDocument.PublicKey, version, allowCensusMode);

            this.StoreChunk(questionnaireDocument.PublicKey, 
                version, 
                SyncItemType.Questionnaire, 
                this.GetItemAsContent(questionnaireDocument), 
                this.GetItemAsContent(questionnaireMetadata), 
                timestamp,
                packageId,
                globalSequence);
        }

        protected string GetItemAsContent(object item)
        {
            return this.serializer.Serialize(item, TypeSerializationSettings.AllTypes);
        }

        public void StoreChunk(Guid questionnaireId, long questionnaireVersion, string itemType, string content, string metaInfo, DateTime timestamp, string packageId, long globalSequence)
        {
            var syncPackageMeta = new QuestionnaireSyncPackageMeta(
                questionnaireId,
                questionnaireVersion,
                timestamp,
                itemType,
                string.IsNullOrEmpty(content) ? 0 : content.Length,
                string.IsNullOrEmpty(metaInfo) ? 0 : metaInfo.Length)
            {
                Content = content,
                Meta = metaInfo,
                PackageId = packageId,
                SortIndex = globalSequence
            };

            syncPackageWriter.Store(syncPackageMeta, packageId);
        }
    }
}