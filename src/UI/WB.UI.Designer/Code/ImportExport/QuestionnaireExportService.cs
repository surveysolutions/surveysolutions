using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.ImportExport;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.UI.Designer.Extensions;

namespace WB.UI.Designer.Code.ImportExport
{
    public class QuestionnaireExportService : IQuestionnaireExportService
    {
        private readonly IQuestionnaireViewFactory questionnaireViewFactory;
        private readonly IAttachmentService attachmentService;
        private readonly ILookupTableService lookupTableService;
        private readonly ITranslationImportExportService translationsService;
        private readonly ICategoriesImportExportService categoriesService;
        private readonly ILogger<QuestionnaireExportService> logger;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly DesignerDbContext questionnaireChangeItemStorage;
        private readonly IImportExportQuestionnaireMapper importExportQuestionnaireMapper;
        private readonly IQuestionnaireSerializer questionnaireSerializer;

        public QuestionnaireExportService(
            IQuestionnaireViewFactory questionnaireViewFactory, 
            IAttachmentService attachmentService, 
            ILookupTableService lookupTableService, 
            ITranslationImportExportService translationsService, 
            ICategoriesImportExportService categoriesService, 
            ILogger<QuestionnaireExportService> logger, 
            IFileSystemAccessor fileSystemAccessor,
            DesignerDbContext questionnaireChangeItemStorage,
            IImportExportQuestionnaireMapper importExportQuestionnaireMapper,
            IQuestionnaireSerializer questionnaireSerializer)
        {
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.attachmentService = attachmentService;
            this.lookupTableService = lookupTableService;
            this.translationsService = translationsService;
            this.categoriesService = categoriesService;
            this.logger = logger;
            this.fileSystemAccessor = fileSystemAccessor;
            this.questionnaireChangeItemStorage = questionnaireChangeItemStorage;
            this.importExportQuestionnaireMapper = importExportQuestionnaireMapper;
            this.questionnaireSerializer = questionnaireSerializer;
        }
        
        public Stream? GetBackupQuestionnaire(QuestionnaireRevision id, out string questionnaireFileName)
        {
            var questionnaireView = questionnaireViewFactory.Load(id);
            if (questionnaireView == null)
            {
                questionnaireFileName = String.Empty;
                return null;
            }

            questionnaireFileName = fileSystemAccessor.MakeValidFileName(questionnaireView.Title);
            
            var questionnaireDocument = questionnaireView.Source;
            
            var maxSequenceByQuestionnaire = id.Version ?? 
                this.questionnaireChangeItemStorage.QuestionnaireChangeRecords
                .Where(y => y.QuestionnaireId == id.QuestionnaireId.FormatGuid())
                .Select(y => (int?)y.Sequence)
                .Max();
            
            questionnaireDocument.Revision = maxSequenceByQuestionnaire ?? 0;
            var questionnaire = importExportQuestionnaireMapper.Map(questionnaireDocument);
            var output = new MemoryStream();

            ZipOutputStream zipStream = new ZipOutputStream(output);

            for (int attachmentIndex = 0; attachmentIndex < questionnaireDocument.Attachments.Count; attachmentIndex++)
            {
                try
                {
                    var attachmentReference = questionnaireDocument.Attachments[attachmentIndex];

                    var attachmentContent = this.attachmentService.GetContent(attachmentReference.ContentId);

                    if (attachmentContent?.Content != null)
                    {
                        var attachmentMeta = this.attachmentService.GetAttachmentMeta(attachmentReference.AttachmentId);

                        //var attachmentFileName = attachmentMeta?.FileName ?? "unknown-file-name";
                        var attachmentFileName = attachmentMeta.AttachmentId.FormatGuid() + Path.GetExtension(attachmentMeta.FileName);
                        zipStream.PutFileEntry($"Attachments/{attachmentFileName}", attachmentContent.Content);

                        var attachmentInfo = questionnaire.Attachments.Single(a => a.Name == attachmentReference.Name);
                        attachmentInfo.FileName = attachmentFileName;
                        attachmentInfo.ContentType = attachmentContent.ContentType; 
                    }
                    else
                    {
                        zipStream.PutTextFileEntry(
                            $"Attachments/Invalid/missing attachment #{attachmentIndex + 1} ({attachmentReference.AttachmentId.FormatGuid()}).txt",
                            $"Attachment '{attachmentReference.Name}' is missing.");
                    }
                }
                catch (Exception exception)
                {
                    this.logger.LogWarning($"Failed to backup attachment #{attachmentIndex + 1} from questionnaire '{questionnaireView.Title}' ({id}).", exception);
                    zipStream.PutTextFileEntry(
                        $"Attachments/Invalid/broken attachment #{attachmentIndex + 1}.txt",
                        $"Failed to backup attachment. See error below.{Environment.NewLine}{exception}");
                }
            }

            foreach (KeyValuePair<Guid, LookupTable> lookupTable in questionnaireDocument.LookupTables)
            {
                var fileName = lookupTable.Key.FormatGuid() + ".txt";
                var lookupTableContent = this.lookupTableService.GetLookupTableContentFile(id, lookupTable.Key);
                zipStream.PutFileEntry($"Lookup Tables/{fileName}", lookupTableContent!.Content);
                questionnaire.LookupTables.Single(l => l.TableName == lookupTable.Value.TableName).FileName = fileName;
            }

            foreach (var translation in questionnaireDocument.Translations)
            {
                var json = this.translationsService.GetTranslationsJson(questionnaireDocument, translation.Id);
                var fileName = translation.Id.FormatGuid() + ".json";
                zipStream.PutTextFileEntry($"Translations/{fileName}", json);
                questionnaire.Translations.Single(t => t.Name == translation.Name).FileName = fileName;
            }

            foreach (var categories in questionnaireDocument.Categories)
            {
                var json = this.categoriesService.GetCategoriesJson(questionnaireDocument.PublicKey, categories.Id);
                var fileName = categories.Id.FormatGuid() + ".json";
                zipStream.PutTextFileEntry($"Categories/{fileName}", json);
                questionnaire.Categories.Single(c => c.Name == categories.Name).FileName = fileName;
            }

            var questionnaireJson = questionnaireSerializer.Serialize(questionnaire);
            zipStream.PutTextFileEntry($"document.json", questionnaireJson);

            zipStream.Finish();
            output.Position = 0;

            return output;
        }
    }
}