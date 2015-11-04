using System;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    internal class QuestionnaireExportStructureDenormalizer : BaseDenormalizer, IEventHandler<TemplateImported>, IEventHandler<PlainQuestionnaireRegistered>, IEventHandler<QuestionnaireDeleted>
    {
        private readonly IReadSideKeyValueStorage<QuestionnaireExportStructure> readsideRepositoryWriter;

        private readonly IExportViewFactory exportViewFactory;
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;

        private readonly IEnvironmentContentService environmentContentService;
        private readonly IFilebasedExportedDataAccessor filebasedExportedDataAccessor;

        private readonly IFileSystemAccessor fileSystemAccessor;

        public QuestionnaireExportStructureDenormalizer(
            IReadSideKeyValueStorage<QuestionnaireExportStructure> readsideRepositoryWriter,
            IExportViewFactory exportViewFactory, IPlainQuestionnaireRepository plainQuestionnaireRepository, 
            IEnvironmentContentService environmentContentService, 
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor, 
            IFileSystemAccessor fileSystemAccessor)
        {
            this.readsideRepositoryWriter = readsideRepositoryWriter;
            this.exportViewFactory = exportViewFactory;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.environmentContentService = environmentContentService;
            this.filebasedExportedDataAccessor = filebasedExportedDataAccessor;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public override object[] Writers
        {
            get { return new object[] { readsideRepositoryWriter}; }
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            Guid id = evnt.EventSourceId;
            long version = evnt.Payload.Version ?? evnt.EventSequence;
            QuestionnaireDocument questionnaireDocument = evnt.Payload.Source;

            this.StoreExportStructure(id, version, questionnaireDocument);
        }

        public void Handle(IPublishedEvent<PlainQuestionnaireRegistered> evnt)
        {
            Guid id = evnt.EventSourceId;
            long version = evnt.Payload.Version;
            QuestionnaireDocument questionnaireDocument = this.plainQuestionnaireRepository.GetQuestionnaireDocument(id, version);

            this.StoreExportStructure(id, version, questionnaireDocument);
        }

        public void Handle(IPublishedEvent<QuestionnaireDeleted> evnt)
        {
            this.readsideRepositoryWriter.AsVersioned().Remove(evnt.EventSourceId.FormatGuid(), evnt.Payload.QuestionnaireVersion);
            DeleteExportedDataForQuestionnaireVersion(evnt.EventSourceId, evnt.Payload.QuestionnaireVersion);
        }

        private void StoreExportStructure(Guid id, long version, QuestionnaireDocument questionnaireDocument)
        {
            var questionnaireExportStructure = this.exportViewFactory.CreateQuestionnaireExportStructure(questionnaireDocument, version);
            CreateExportStructureByTemplate(questionnaireExportStructure);
            this.readsideRepositoryWriter.AsVersioned().Store(questionnaireExportStructure, id.FormatGuid(), version);
        }


        public void CreateExportStructureByTemplate(QuestionnaireExportStructure questionnaireExportStructure)
        {
            var dataFolderForTemplatePath =
              this.filebasedExportedDataAccessor.GetFolderPathOfDataByQuestionnaire(questionnaireExportStructure.QuestionnaireId,
                  questionnaireExportStructure.Version);

            if (this.fileSystemAccessor.IsDirectoryExists(dataFolderForTemplatePath))
            {
                this.fileSystemAccessor.DeleteDirectory(dataFolderForTemplatePath);
            }

            this.fileSystemAccessor.CreateDirectory(dataFolderForTemplatePath);
            foreach (var headerStructureForLevel in questionnaireExportStructure.HeaderToLevelMap.Values)
            {
                this.environmentContentService.CreateContentOfAdditionalFile(headerStructureForLevel,
                    ExportFileSettings.GetContentFileName(headerStructureForLevel.LevelName), dataFolderForTemplatePath);
            }
        }

        public void DeleteExportedDataForQuestionnaireVersion(Guid questionnaireId, long questionnaireVersion)
        {
            var dataFolderForTemplatePath =
                this.filebasedExportedDataAccessor.GetFolderPathOfDataByQuestionnaire(questionnaireId,
                    questionnaireVersion);

            this.fileSystemAccessor.DeleteDirectory(dataFolderForTemplatePath);

            var filesFolderForTemplatePath =
                this.filebasedExportedDataAccessor.GetFolderPathOfFilesByQuestionnaire(questionnaireId,
                    questionnaireVersion);

            this.fileSystemAccessor.DeleteDirectory(filesFolderForTemplatePath);
        }
    }
}
