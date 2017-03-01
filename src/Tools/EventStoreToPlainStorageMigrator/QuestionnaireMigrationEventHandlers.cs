using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Npgsql;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.EventHandler.WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Supervisor.Factories;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.EventHandler.WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace EventStoreToPlainStorageMigrator
{
    public class QuestionnaireMigrationEventHandlers
    {
        internal readonly ObsoleteEventHandleDescriptor[] eventHandlers;

        internal QuestionnaireMigrationEventHandlers(
            IPlainSessionProvider plainPostgresTransactionManager, 
            WB.Core.GenericSubdomains.Portable.Services.ILogger logger,
            PostgresPlainStorageSettings postgresPlainStorageSettings)
        {
            var questionnaireDocumentRepository =
                   new PostgresPlainKeyValueStorage<QuestionnaireDocument>(postgresPlainStorageSettings, logger);

            ThrowIfTableNotEmpty(postgresPlainStorageSettings.ConnectionString, "questionnairedocuments");
            this.plainQuestionnaireRepository = new PlainQuestionnaireRepositoryWithCache(questionnaireDocumentRepository);

            this.questionnaireExportStructureStorage = new PostgresPlainKeyValueStorage<QuestionnaireExportStructure>(postgresPlainStorageSettings, logger);
            ThrowIfTableNotEmpty(postgresPlainStorageSettings.ConnectionString, "questionnaireexportstructures");
            this.questionnaireRosterStructureStorage = new PostgresPlainKeyValueStorage<QuestionnaireRosterStructure>(postgresPlainStorageSettings, logger);
            ThrowIfTableNotEmpty(postgresPlainStorageSettings.ConnectionString, "questionnairerosterstructures");
            this.referenceInfoForLinkedQuestionsStorage =
                new PostgresPlainKeyValueStorage<ReferenceInfoForLinkedQuestions>(postgresPlainStorageSettings, logger);
            this.questionnaireQuestionsInfoStorage = new PostgresPlainKeyValueStorage<QuestionnaireQuestionsInfo>(postgresPlainStorageSettings, logger);
            ThrowIfTableNotEmpty(postgresPlainStorageSettings.ConnectionString, "questionnairequestionsinfos");

            this.questionnaireBrowseItemStorage =
                new PostgresPlainStorageRepository<QuestionnaireBrowseItem>(plainPostgresTransactionManager);
            ThrowIfTableNotEmpty(postgresPlainStorageSettings.ConnectionString, "questionnairebrowseitems");

            eventHandlers =  new ObsoleteEventHandleDescriptor[]
            {
                new ObsoleteEventHandleDescriptor<TemplateImported>(HandleTemplateImportedIfPossible),
                new ObsoleteEventHandleDescriptor<QuestionnaireDisabled>(HandleQuestionnaireDisabledIfPossible),
                new ObsoleteEventHandleDescriptor<QuestionnaireDeleted>(HandleQuestionnaireDeletedIfPossible)
            };
        }

        private void ThrowIfTableNotEmpty(string connectionString, string tableName)
        {
            bool isTableEmpty = false;
            using (var existsCommand = new NpgsqlCommand())
            {
                long count = 0;
                existsCommand.CommandText = $"SELECT count(*) FROM {tableName}";

                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    existsCommand.Connection = connection;
                    count = (long) existsCommand.ExecuteScalar();
                }

                isTableEmpty = count == 0;
            }
            if (!isTableEmpty)
            {
                throw new InvalidOperationException($"Table {tableName} is not empty. That means something already have created users in the plain storage. Please think twice before cleaning the table and run the tool again!!!");
            }
        }
        private  void HandleTemplateImportedIfPossible(TemplateImported templateImportedEvent, Guid eventSourceId,
            long eventSequence)
        {
            StoreQuestionnaireToPlainStorage(templateImportedEvent, eventSourceId,
                eventSequence);
        }

        private  void HandleQuestionnaireDisabledIfPossible(QuestionnaireDisabled questionnaireDisabledEvent,
            Guid eventSourceId,
            long eventSequence)
        {
            var questionnaireBrowseItem =
                questionnaireBrowseItemStorage.GetById(
                    new QuestionnaireIdentity(eventSourceId, questionnaireDisabledEvent.QuestionnaireVersion)
                        .ToString());
            if (questionnaireBrowseItem != null)
            {
                questionnaireBrowseItem.Disabled = true;
                questionnaireBrowseItemStorage.GetById(
                    new QuestionnaireIdentity(eventSourceId, questionnaireDisabledEvent.QuestionnaireVersion)
                        .ToString());
            }
        }

        private  void HandleQuestionnaireDeletedIfPossible(QuestionnaireDeleted questionnaireDeletedEvent,
            Guid eventSourceId,
            long eventSequence)
        {
            var questionnaireBrowseItem =
                questionnaireBrowseItemStorage.GetById(
                    new QuestionnaireIdentity(eventSourceId,
                        questionnaireDeletedEvent.QuestionnaireVersion).ToString());
            if (questionnaireBrowseItem != null)
            {
                questionnaireBrowseItem.IsDeleted = true;
                questionnaireBrowseItemStorage.GetById(
                    new QuestionnaireIdentity(eventSourceId,
                        questionnaireDeletedEvent.QuestionnaireVersion).ToString());
            }
        }

        private  void StoreQuestionnaireToPlainStorage(TemplateImported templateImportedEvent,
            Guid questionnaireId, long eventSequence)
        {
            var document = templateImportedEvent.Source;
            var newVersion = templateImportedEvent.Version ?? eventSequence;
            plainQuestionnaireRepository.StoreQuestionnaire(questionnaireId, newVersion, document);
            questionnaireBrowseItemStorage.Store(
                new QuestionnaireBrowseItem(document, newVersion, templateImportedEvent.AllowCensusMode,
                    templateImportedEvent.ContentVersion ?? 1),
                new QuestionnaireIdentity(questionnaireId, newVersion).ToString());

            var questionnaireEntityId = new QuestionnaireIdentity(questionnaireId, newVersion).ToString();

            referenceInfoForLinkedQuestionsStorage.Store(
                referenceInfoForLinkedQuestionsFactory.CreateReferenceInfoForLinkedQuestions(document, newVersion),
                questionnaireEntityId);
            questionnaireExportStructureStorage.Store(
                exportViewFactory.CreateQuestionnaireExportStructure(document, newVersion), questionnaireEntityId);
            questionnaireRosterStructureStorage.Store(
                questionnaireRosterStructureFactory.CreateQuestionnaireRosterStructure(document, newVersion),
                questionnaireEntityId);
            questionnaireQuestionsInfoStorage.Store(new QuestionnaireQuestionsInfo
            {
                QuestionIdToVariableMap =
                    document.Find<IQuestion>(question => true).ToDictionary(x => x.PublicKey, x => x.StataExportCaption)
            }, questionnaireEntityId);
        }
        private readonly IReferenceInfoForLinkedQuestionsFactory referenceInfoForLinkedQuestionsFactory =
            new ReferenceInfoForLinkedQuestionsFactory();

        private readonly IQuestionnaireRosterStructureFactory questionnaireRosterStructureFactory =
            new QuestionnaireRosterStructureFactory();

        private readonly IExportViewFactory exportViewFactory = new ExportViewFactory(
            new QuestionnaireRosterStructureFactory(), new FileSystemIOAccessor());

        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage;

        private readonly IPlainKeyValueStorage<ReferenceInfoForLinkedQuestions> referenceInfoForLinkedQuestionsStorage;
        private readonly IPlainKeyValueStorage<QuestionnaireRosterStructure> questionnaireRosterStructureStorage;
        private readonly IPlainKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureStorage;
        private readonly IPlainKeyValueStorage<QuestionnaireQuestionsInfo> questionnaireQuestionsInfoStorage;
    }
}