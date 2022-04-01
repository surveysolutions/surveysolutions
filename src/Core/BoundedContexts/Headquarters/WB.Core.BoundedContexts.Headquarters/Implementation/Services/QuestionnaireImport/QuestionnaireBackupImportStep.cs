using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Headquarters.Designer;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.HttpServices.HttpClient;
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
        private readonly ILogger<QuestionnaireBackupImportStep> logger;
        private RestFile backupFile;
        private readonly IArchiveUtils archiveUtils;

        private Dictionary<string, long> filesInBackup = new Dictionary<string, long>();
        private readonly IServiceLocator serviceLocator;

        public QuestionnaireBackupImportStep(QuestionnaireIdentity questionnaireIdentity,
            QuestionnaireDocument questionnaire,
            IDesignerApi designerApi,
            IServiceLocator serviceLocator,
            IArchiveUtils archiveUtils)
        {
            this.questionnaireIdentity = questionnaireIdentity;
            this.designerApi = designerApi;
            this.serviceLocator = serviceLocator;
            this.logger = serviceLocator.GetInstance<ILogger<QuestionnaireBackupImportStep>>();
            
            this.questionnaire = questionnaire;

            this.archiveUtils = archiveUtils;
        }

        public bool IsNeedProcessing() => true;

        public async Task DownloadFromDesignerAsync(IProgress<int> progress)
        {
            logger.LogInformation($"Downloading Questionnaire Backup.", questionnaireIdentity);

            backupFile = await designerApi.DownloadQuestionnaireBackup(questionnaireIdentity.QuestionnaireId);

            progress.Report(100);
        }

        public void SaveData(IProgress<int> progress)
        {
            if (backupFile != null)
            {
                var questionnaireBackupStorage = serviceLocator.GetInstance<IPlainKeyValueStorage<QuestionnaireBackup>>();
                    questionnaireBackupStorage.Store(new QuestionnaireBackup { Content = backupFile.Content }, questionnaireIdentity.ToString());

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
                var attachmentContentService = serviceLocator.GetInstance<IAttachmentContentService>();

                foreach (var questionnaireAttachment in questionnaire.Attachments)
                {
                    if (!attachmentContentService.HasAttachmentContent(questionnaireAttachment.ContentId))
                    {
                        this.logger.LogInformation("Saving attachment.", questionnaireIdentity, 
                            questionnaireAttachment.AttachmentId, questionnaireAttachment.ContentId);
                        
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

                            if (string.IsNullOrEmpty(contentType) || file == null)
                            {
                                throw new InvalidOperationException($"Attachment {questionnaireAttachment.AttachmentId} for questionnaire {questionnaireIdentity} cannot be imported");
                            }

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

        private void ProcessLookupTables()
        {
            foreach (var lookupId in questionnaire.LookupTables)
            {
                var lookupTablesStorage = serviceLocator.GetInstance<IPlainKeyValueStorage<QuestionnaireLookupTable>>();

                this.logger.LogInformation("Saving lookup table.", questionnaireIdentity, lookupId.Key);

                var lookupName = lookupId.Key.FormatGuid();

                var lookupFile = filesInBackup.Keys.FirstOrDefault(x => x.Contains(lookupName, StringComparison.InvariantCultureIgnoreCase));

                if (lookupFile == null)
                {
                    throw new InvalidOperationException($"Lookup table {lookupId.Key} for questionnaire {questionnaireIdentity} was not found and cannot be imported");
                }

                var lookup = archiveUtils.GetFileFromArchive(backupFile.Content, lookupFile);

                if (lookup == null)
                {
                    throw new InvalidOperationException($"Lookup table {lookupId.Key} for questionnaire {questionnaireIdentity} cannot be imported");
                }

                lookupTablesStorage.Store(new QuestionnaireLookupTable()
                    {
                        FileName = lookupId.Value.FileName,
                        Content = Encoding.UTF8.GetString(lookup.Bytes)

                    }, questionnaireIdentity, lookupId.Key);
            }
        }

        private void ProcessCategories()
        {
            if (questionnaire.Categories.Count <= 0) return;

            var reusableCategoriesStorage = serviceLocator.GetInstance<IReusableCategoriesStorage>();

            var categoriesImportService = serviceLocator.GetInstance<ICategoriesImporter>();

            foreach (var category in questionnaire.Categories)
            {
                this.logger.LogInformation("Saving reusable category.", questionnaireIdentity, category.Id);

                var categoryName = category.Id.FormatGuid();

                var categoryFile = filesInBackup.Keys.FirstOrDefault(x => x.Contains(categoryName, StringComparison.InvariantCultureIgnoreCase));

                if (categoryFile == null)
                {
                    throw new InvalidOperationException($"Categories {category.Id} for questionnaire {questionnaireIdentity} cannot be imported");
                }

                var categoryContent = archiveUtils.GetFileFromArchive(backupFile.Content, categoryFile);
                var reusableCategories = categoriesImportService.ExtractCategoriesFromExcelFile(new MemoryStream(categoryContent.Bytes));

                reusableCategoriesStorage.Store(questionnaireIdentity, category.Id, reusableCategories);
                    
            }
        }

        private void ProcessTranslations()
        {
            if (questionnaire.Translations?.Count != 0)
            {
                var translationManagementService = serviceLocator.GetInstance<ITranslationManagementService>();

                translationManagementService.Delete(questionnaireIdentity);

                foreach (var translation in questionnaire.Translations)
                {
                    this.logger.LogInformation("Saving translation.", questionnaireIdentity, translation.Id);

                    var translationName = translation.Id.FormatGuid();

                    var translationFile = filesInBackup.Keys.FirstOrDefault(x => x.Contains(translationName, StringComparison.InvariantCultureIgnoreCase));

                    if (translationFile == null)
                    {
                        throw new InvalidOperationException($"Translation {translation.Id} for questionnaire {questionnaireIdentity} cannot be imported");
                    }

                    var translationContent = archiveUtils.GetFileFromArchive(backupFile.Content, translationFile);
                        List<TranslationInstance> translations = GetTranslations(translation.Id, translationContent.Bytes);

                        translationManagementService.Store(translations);
                    
                }
            }
        }

        private List<TranslationInstance> GetTranslations(Guid translationId, byte[] content)
        {
            return serviceLocator.GetInstance<ITranslationImporter>().GetTranslationInstancesFromExcelFile(questionnaire, questionnaireIdentity,
                translationId, content);
        }
    }
}
