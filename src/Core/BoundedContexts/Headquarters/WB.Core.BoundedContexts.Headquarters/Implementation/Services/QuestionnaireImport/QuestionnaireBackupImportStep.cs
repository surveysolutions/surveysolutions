using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.Designer;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Enumerator.Native.Questionnaire;
using WB.Infrastructure.Native.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services.QuestionnaireImport
{
    public class QuestionnaireBackupImportStep : IQuestionnaireImportStep
    {
        private readonly IDesignerApi designerApi;
        private readonly QuestionnaireIdentity questionnaireIdentity;
        private readonly QuestionnaireDocument questionnaire;
        private readonly IPlainKeyValueStorage<QuestionnaireBackup> questionnaireBackupStorage;
        private readonly ILogger logger;
        
        private RestFile backupFile;

        private readonly IArchiveUtils archiveUtils;

        private readonly ICategoriesImporter categoriesImportService;
        private readonly ITranslationImporter translationImporter;

        private readonly IAttachmentContentService attachmentContentService;
        private readonly ITranslationManagementService translationManagementService;
        private readonly IPlainKeyValueStorage<QuestionnaireLookupTable> lookupTablesStorage;
        private readonly IReusableCategoriesStorage reusableCategoriesStorage;
        private Dictionary<string, long> filesInBackup = new Dictionary<string, long>();

        public QuestionnaireBackupImportStep(QuestionnaireIdentity questionnaireIdentity,
            QuestionnaireDocument questionnaire,
            IDesignerApi designerApi, 
            IPlainKeyValueStorage<QuestionnaireBackup> questionnaireBackupStorage, 
            ILogger logger,
            IAttachmentContentService attachmentContentService,
            ITranslationManagementService translationManagementService,
            IPlainKeyValueStorage<QuestionnaireLookupTable> lookupTablesStorage,
            IReusableCategoriesStorage reusableCategoriesStorage,
            IArchiveUtils archiveUtils,
            ICategoriesImporter categoriesImportService, 
            ITranslationImporter translationImporter)
        {
            this.questionnaireIdentity = questionnaireIdentity;
            this.designerApi = designerApi;
            this.questionnaireBackupStorage = questionnaireBackupStorage;
            this.logger = logger;
            this.questionnaire = questionnaire;

            this.archiveUtils = archiveUtils;
            this.categoriesImportService = categoriesImportService;
            this.translationImporter = translationImporter;

            this.attachmentContentService = attachmentContentService;
            this.translationManagementService = translationManagementService;
            this.lookupTablesStorage = lookupTablesStorage;
            this.reusableCategoriesStorage = reusableCategoriesStorage;
        }

        public bool IsNeedProcessing() => true;

        public async Task DownloadFromDesignerAsync(IProgress<int> progress)
        {
            logger.Verbose($"Downloading Questionnaire Backup: {questionnaireIdentity.QuestionnaireId} ver.{questionnaireIdentity.Version}");

            backupFile = await designerApi.DownloadQuestionnaireBackup(questionnaireIdentity.QuestionnaireId);

            progress.Report(100);
        }

        public void SaveData(IProgress<int> progress)
        {
            if (backupFile != null)
            {
                this.questionnaireBackupStorage.Store(new QuestionnaireBackup { Content = backupFile.Content }, questionnaireIdentity.ToString());

                ProcessDependingObjects();
            }

            progress.Report(100);
        }

        private void ProcessDependingObjects()
        {
            filesInBackup = archiveUtils.GetArchivedFileNamesAndSize(backupFile.Content);

            ProcessAttachments();
            ProcessTranslations();
            ProcessLookupTables();
            ProcessCategories();
        }

        private void ProcessAttachments()
        {
            if (questionnaire.Attachments?.Count > 0)
            {
                foreach (var questionnaireAttachment in questionnaire.Attachments)
                {
                    if (!attachmentContentService.HasAttachmentContent(questionnaireAttachment.ContentId))
                    {
                        var attachmentName = questionnaireAttachment.AttachmentId.FormatGuid();
                        var attachmentFiles = 
                            filesInBackup.Where(x => x.Key.Contains(attachmentName, StringComparison.InvariantCultureIgnoreCase))
                                .Select(x => x).ToList();

                        if (attachmentFiles.Any())
                        {
                            string contentType = string.Empty;
                            ExtractedFile file = null;

                            foreach (var attachmentFile in attachmentFiles)
                            {
                                if (attachmentFile.Key.ToLower().EndsWith("content-type.txt"))
                                {
                                    var contentTypeFile = archiveUtils.GetFileFromArchive(backupFile.Content, attachmentFile.Key);
                                    contentType = Encoding.UTF8.GetString(contentTypeFile.Bytes);
                                }
                                else
                                {
                                    file = archiveUtils.GetFileFromArchive(backupFile.Content, attachmentFile.Key);
                                    file.Name = attachmentFile.Key.Split(
                                        new[] {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar},
                                        StringSplitOptions.RemoveEmptyEntries).Last();
                                }
                            }

                            if (!string.IsNullOrEmpty(contentType) && file != null)
                            {
                                attachmentContentService.SaveAttachmentContent(
                                    questionnaireAttachment.ContentId,
                                    contentType,
                                    file.Name,
                                    file.Bytes);
                            }
                        }
                    }
                }
            }
        }

        private void ProcessLookupTables()
        {
            foreach (var lookupId in questionnaire.LookupTables)
            {
                this.logger.Debug($"Saving lookup table for questionnaire {questionnaireIdentity}. Lookup id {lookupId}");

                var lookupName = lookupId.Key.FormatGuid();

                var lookupFile = filesInBackup.Keys.FirstOrDefault(x => x.Contains(lookupName, StringComparison.InvariantCultureIgnoreCase));

                if (lookupFile != null)
                {
                    var lookup = archiveUtils.GetFileFromArchive(backupFile.Content, lookupFile);

                    if (lookup != null)
                    {
                        lookupTablesStorage.Store(new QuestionnaireLookupTable()
                        {
                            FileName = lookupId.Value.FileName,
                            Content = Encoding.UTF8.GetString(lookup.Bytes)

                        }, questionnaireIdentity, lookupId.Key);
                    }
                }
            }
        }

        private void ProcessCategories()
        {
            if (questionnaire?.Categories?.Count > 0)
            {
                foreach (var category in questionnaire.Categories)
                {
                    this.logger.Debug($"Loading reusable category for questionnaire {questionnaireIdentity}. Category id {category.Id}");

                    var lookupName = category.Id.FormatGuid();

                    var categoryFile = filesInBackup.Keys.FirstOrDefault(x => x.Contains(lookupName, StringComparison.InvariantCultureIgnoreCase));

                    if (categoryFile != null)
                    {
                        var categoryContent = archiveUtils.GetFileFromArchive(backupFile.Content, categoryFile);
                        var reusableCategories = this.categoriesImportService.ExtractCategoriesFromExcelFile(new MemoryStream(categoryContent.Bytes));

                        reusableCategoriesStorage.Store(questionnaireIdentity, category.Id, reusableCategories);
                    }
                }
            }
        }

        private void ProcessTranslations()
        {
            if (questionnaire.Translations?.Count != 0)
            {
                translationManagementService.Delete(questionnaireIdentity);

                foreach (var translation in questionnaire.Translations)
                {
                    var translationName = translation.Id.FormatGuid();

                    var translationFile = filesInBackup.Keys.FirstOrDefault(x => x.Contains(translationName, StringComparison.InvariantCultureIgnoreCase));

                    if (translationFile != null)
                    {
                        var translationContent = archiveUtils.GetFileFromArchive(backupFile.Content, translationFile);
                        List<TranslationInstance> translations = GetTranslations(translation.Id, translationContent.Bytes);

                        translationManagementService.Store(translations);
                    }
                }
            }
        }

        private List<TranslationInstance> GetTranslations(Guid translationId, byte[] content)
        {
            return translationImporter.GetTranslationInstancesFromExcelFile(questionnaire, questionnaireIdentity,
                translationId, content);
        }
    }
}
