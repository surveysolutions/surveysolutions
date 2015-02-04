using System;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    internal class QuestionnaireSynchronizationDenormalizer : BaseSynchronizationDenormalizer,
        IEventHandler<QuestionnaireDeleted>,
        IEventHandler<QuestionnaireAssemblyImported>,
        IEventHandler<TemplateImported>,
        IEventHandler<PlainQuestionnaireRegistered>
    {
        private readonly IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor;
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;

        public static Guid AssemblySeed = new Guid("371EF2E6-BF1D-4E36-927D-2AC13C41EF7B");

        public QuestionnaireSynchronizationDenormalizer(IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor,
            IPlainQuestionnaireRepository plainQuestionnaireRepository,
            IArchiveUtils archiver, IJsonUtils jsonUtils,
            IReadSideRepositoryWriter<SynchronizationDelta> syncStorage,
            IQueryableReadSideRepositoryReader<SynchronizationDelta> syncStorageReader)
            : base(archiver, jsonUtils, syncStorage, syncStorageReader)
        {
            this.questionnareAssemblyFileAccessor = questionnareAssemblyFileAccessor;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
        }

        public override object[] Writers
        {
            get { return new object[] { syncStorage }; }
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

            var syncItem = this.CreateSyncItem(
                questionnaireId.Combine(questionnaireVersion), 
                SyncItemType.DeleteTemplate, 
                questionnaireId.ToString(),
                GetItemAsContent(questionnaireMetadata));

            StoreChunk(syncItem, null, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<QuestionnaireAssemblyImported> evnt)
        {
            var assemblyAsBase64String = this.questionnareAssemblyFileAccessor.GetAssemblyAsBase64String(evnt.EventSourceId, evnt.Payload.Version);
            Guid publicKey = evnt.EventSourceId;
            long version = evnt.Payload.Version;
            var meta = new QuestionnaireAssemblyMetadata(publicKey, version);

            var syncItem = this.CreateSyncItem(publicKey.Combine(AssemblySeed).Combine(version), SyncItemType.QuestionnaireAssembly,
                GetItemAsContent(assemblyAsBase64String), GetItemAsContent(meta));

            StoreChunk(syncItem, null, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            this.SaveQuestionnaire(evnt.Payload.Source, evnt.Payload.Version ?? evnt.EventSequence, evnt.Payload.AllowCensusMode,
                evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<PlainQuestionnaireRegistered> evnt)
        {
            QuestionnaireDocument questionnaireDocument = this.plainQuestionnaireRepository.GetQuestionnaireDocument(evnt.EventSourceId, evnt.Payload.Version);
            this.SaveQuestionnaire(questionnaireDocument, evnt.Payload.Version, evnt.Payload.AllowCensusMode,
                evnt.EventTimeStamp);
        }

        public void SaveQuestionnaire(QuestionnaireDocument doc, long version, bool allowCensusMode, DateTime timestamp)
        {
            doc.IsDeleted = false;

            var questionnaireMetadata = new QuestionnaireMetadata(doc.PublicKey, version, allowCensusMode);
            var syncItem = CreateSyncItem(doc.PublicKey.Combine(version), SyncItemType.Template, GetItemAsContent(doc),
                GetItemAsContent(questionnaireMetadata));

            StoreChunk(syncItem, null, timestamp);
        }
    }
}