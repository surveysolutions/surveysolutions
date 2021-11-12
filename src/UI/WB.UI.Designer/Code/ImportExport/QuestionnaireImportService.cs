using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;
using Main.Core.Documents;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.ImportExport;
using WB.Core.BoundedContexts.Designer.ImportExport.Models;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Services.Restore;

namespace WB.UI.Designer.Code.ImportExport
{
    public class QuestionnaireImportService : IQuestionnaireRestoreService
    {
        private readonly ILogger<QuestionnaireImportService> logger;
        private readonly ICommandService commandService;
        private readonly ILookupTableService lookupTableService;
        private readonly IAttachmentService attachmentService;
        private readonly ITranslationsService translationsService;
        private readonly ITranslationImportExportService translationImportExportService;
        private readonly DesignerDbContext dbContext;
        private readonly ICategoriesService categoriesService;
        private readonly IImportExportQuestionnaireMapper importExportQuestionnaireMapper;
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage;
        private readonly IQuestionnaireSerializer questionnaireSerializer;
        private readonly ICategoriesImportExportService categoriesImportExportService;


        public QuestionnaireImportService(ILogger<QuestionnaireImportService> logger,
            ICommandService commandService, 
            ILookupTableService lookupTableService,
            IAttachmentService attachmentService, 
            ITranslationsService translationsService, 
            ITranslationImportExportService translationImportExportService, 
            DesignerDbContext dbContext,
            ICategoriesService categoriesService, 
            IImportExportQuestionnaireMapper importExportQuestionnaireMapper,
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage,
            IQuestionnaireSerializer questionnaireSerializer,
            ICategoriesImportExportService categoriesImportExportService)
        {
            this.logger = logger;
            this.commandService = commandService;
            this.lookupTableService = lookupTableService;
            this.attachmentService = attachmentService;
            this.translationsService = translationsService;
            this.translationImportExportService = translationImportExportService;
            this.dbContext = dbContext;
            this.categoriesService = categoriesService;
            this.importExportQuestionnaireMapper = importExportQuestionnaireMapper;
            this.questionnaireStorage = questionnaireStorage;
            this.questionnaireSerializer = questionnaireSerializer;
            this.categoriesImportExportService = categoriesImportExportService;
        }

        public Guid RestoreQuestionnaire(Stream archive, Guid responsibleId, RestoreState state, bool createNew)
        {
            using var zipStream = new ZipInputStream(archive);

            var questionnaire = RestoreQuestionnaireFromZipFileOrThrow(zipStream, responsibleId, state, createNew);
            var questionnaireDocument = importExportQuestionnaireMapper.Map(questionnaire);
            if (createNew)
            {
                var publicKey = Guid.NewGuid();
                questionnaireDocument.Id = publicKey.FormatGuid();
                questionnaireDocument.PublicKey = publicKey;
                questionnaireDocument.CreationDate = DateTime.UtcNow;
                questionnaireDocument.CreatedBy = responsibleId;
            }
            
            this.translationsService.DeleteAllByQuestionnaireId(questionnaireDocument.PublicKey);
            this.categoriesService.DeleteAllByQuestionnaireId(questionnaireDocument.PublicKey);

            state.Success.AppendLine($"[document.json]");
            state.Success.AppendLine($"    Restored questionnaire document '{questionnaireDocument.Title}' with id '{questionnaireDocument.PublicKey.FormatGuid()}'.");
            state.RestoredEntitiesCount++;

            var command = new ImportQuestionnaire(responsibleId, questionnaireDocument);
            this.commandService.Execute(command);

            zipStream.Seek(0, SeekOrigin.Begin);
            ZipEntry zipEntry;
            while ((zipEntry = zipStream.GetNextEntry()) != null)
            {
                this.RestoreDataFromZipFileEntry(zipEntry, zipStream, responsibleId, state, questionnaire, questionnaireDocument);
            }

            questionnaireStorage.Store(questionnaireDocument, questionnaireDocument.Id);

            return questionnaireDocument.PublicKey;
        }
        
        private bool ValidateBySchema(string json, out IList<ValidationError> errors)
        {
            var testType = typeof(QuestionnaireImportService);
            var readResourceFile = $"{testType.Namespace}.QuestionnaireSchema.json";

            using Stream? stream = testType.Assembly.GetManifestResourceStream(readResourceFile);
            if (stream == null)
                throw new ArgumentException("Can't find json schema for questionnaire");
            using StreamReader reader = new StreamReader(stream);
            string schemaText = reader.ReadToEnd();

            var schema = JSchema.Parse(schemaText);

            JToken jToken = JToken.Parse(json);
            var isValid = jToken.IsValid(schema, out IList<ValidationError> jsonErrors);
            errors = jsonErrors;
            return isValid;
        }

        private Questionnaire RestoreQuestionnaireFromZipFileOrThrow(ZipInputStream zipStream, Guid responsibleId,
            RestoreState state, bool createNew)
        {
            string? textContent = null;
            ZipEntry zipEntry;
            while ((zipEntry = zipStream.GetNextEntry()) != null)
            {
                if (zipEntry.IsDirectory) 
                    continue;
                
                try
                {
                    string[] zipEntryPathChunks = zipEntry.FileName.Split(
                        new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                        StringSplitOptions.RemoveEmptyEntries);

                    if (zipEntryPathChunks.Length == 1 && zipEntryPathChunks[0].ToLower().Equals("document.json"))
                    {
                        textContent = new StreamReader(zipStream, Encoding.UTF8).ReadToEnd();
                    }
                }
                catch (Exception exception)
                {
                    this.logger.LogWarning(exception, $"Error processing zip file entry '{zipEntry.FileName}' during questionnaire restore from backup.");
                    state.Error = $"Error processing zip file entry '{zipEntry.FileName}'.{Environment.NewLine}{exception}";
                    logger.LogError(state.Error);
                }
            }

            if (textContent == null)
            {
                state.Error = "Invalid format. Questionnaire was not found.";
                throw new Exception("Invalid format. Questionnaire was not found.");
            }

            try
            {
                if (!ValidateBySchema(textContent, out var errors))
                {
                    state.Error =
                        "Questionnaire json file has schema errors. Please fix it." +
                        errors.Select(v => $"{v.Path}(line {v.LineNumber}): {v.Message}").Aggregate("", (current, message) => current + "\r\n" + message);
                    throw new ArgumentException("Questionnaire json file has schema errors. Please fix it.");
                }
            }
            catch (JsonReaderException e)
            {
                state.Error = "Invalid format. Questionnaire has invalid format.";
                throw;
            }

                        
            var questionnaire = questionnaireSerializer.Deserialize(textContent);
            return questionnaire;
        }

        public void RestoreDataFromZipFileEntry(ZipEntry zipEntry, ZipInputStream zipStream, Guid responsibleId, RestoreState state, Questionnaire questionnaire, QuestionnaireDocument questionnaireDocument)
        {
            var questionnaireId = questionnaireDocument.PublicKey;
            try
            {
                string[] zipEntryPathChunks = zipEntry.FileName.Split(
                    new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                    StringSplitOptions.RemoveEmptyEntries);

                bool isAttachmentEntry =
                    zipEntryPathChunks.Length == 2 &&
                    string.Equals(zipEntryPathChunks[0], "attachments", StringComparison.CurrentCultureIgnoreCase);

                bool isLookupTableEntry =
                    zipEntryPathChunks.Length == 2 &&
                    string.Equals(zipEntryPathChunks[0], "lookup tables", StringComparison.CurrentCultureIgnoreCase) &&
                    zipEntryPathChunks[1].ToLower().EndsWith(".txt");

                bool isTranslationEntry =
                    zipEntryPathChunks.Length == 2 &&
                    string.Equals(zipEntryPathChunks[0], "translations", StringComparison.CurrentCultureIgnoreCase) &&
                    ".json".Equals(Path.GetExtension(zipEntryPathChunks[1]), StringComparison.InvariantCultureIgnoreCase);

                bool isCategoriesEntry =
                    zipEntryPathChunks.Length == 2 &&
                    string.Equals(zipEntryPathChunks[0], "categories", StringComparison.CurrentCultureIgnoreCase) &&
                    ".json".Equals(Path.GetExtension(zipEntryPathChunks[1]), StringComparison.InvariantCultureIgnoreCase);

                if (isAttachmentEntry)
                {
                    var fileName = zipEntryPathChunks[1];
                    byte[] binaryContent = zipStream.ReadToEnd();
                    var attachmentInfo = questionnaire.Attachments.Single(a => a.FileName == fileName);
                    var attachment = questionnaireDocument.Attachments.Single(s =>
                        s.Name == attachmentInfo.Name);
                    var attachmentId = attachment.AttachmentId;

                    string attachmentContentId = this.attachmentService.CreateAttachmentContentId(binaryContent);
                    attachment.ContentId = attachmentContentId;
  
                    this.attachmentService.SaveContent(attachmentContentId, attachmentInfo.ContentType!, binaryContent);
                    this.attachmentService.SaveMeta(attachmentId, questionnaireId, attachmentContentId, fileName);

                    state.Success.AppendLine($"[{zipEntry.FileName}]");
                    state.Success.AppendLine($"    Found file data '{fileName}' for attachment '{attachmentId.FormatGuid()}'.");
                    state.Success.AppendLine($"    Restored attachment '{attachmentId.FormatGuid()}' for questionnaire '{questionnaireId.FormatGuid()}' using file '{attachmentInfo.FileName}' and content-type '{attachmentInfo.ContentType}'.");
                    state.RestoredEntitiesCount++;
                }
                else if (isLookupTableEntry)
                {
                    var fileName = zipEntryPathChunks[1];
                    var lookupTableInfo = questionnaire.LookupTables.Single(a => a.FileName == fileName);
                    var lookupTable = questionnaireDocument.LookupTables.Single(s =>
                        s.Value.TableName == lookupTableInfo.TableName);
                    var lookupTableId = lookupTable.Key;

                    string textContent = new StreamReader(zipStream, Encoding.UTF8).ReadToEnd();

                    this.lookupTableService.SaveLookupTableContent(questionnaireId, lookupTableId, textContent);

                    state.Success.AppendLine($"[{zipEntry.FileName}].");
                    state.Success.AppendLine($"    Restored lookup table '{lookupTableId.FormatGuid()}' for questionnaire '{questionnaireId.FormatGuid()}'.");
                    state.RestoredEntitiesCount++;
                }
                else if (isTranslationEntry)
                {
                    var fileName = zipEntryPathChunks[1];
                    string textContent = new StreamReader(zipStream, Encoding.UTF8).ReadToEnd();

                    var translationInfo = questionnaire.Translations.Single(a => a.FileName == fileName);
                    var translation = questionnaireDocument.Translations.Single(s =>
                        s.Name == translationInfo.Name);
                    var translationId = translation.Id;

                    this.translationImportExportService.StoreTranslationsFromJson(questionnaireDocument, translationId, textContent);

                    state.Success.AppendLine($"[{zipEntry.FileName}].");
                    state.Success.AppendLine($"    Restored translation '{translationId}' for questionnaire '{questionnaireId.FormatGuid()}'.");
                    state.RestoredEntitiesCount++;
                }
                else if (isCategoriesEntry)
                {
                    var fileName = zipEntryPathChunks[1];
                    string textContent = new StreamReader(zipStream, Encoding.UTF8).ReadToEnd();

                    var categoriesInfo = questionnaire.Categories.Single(a => a.FileName == fileName);
                    var categories = questionnaireDocument.Categories.Single(s =>
                        s.Name == categoriesInfo.Name);
                    var collectionsId = categories.Id;

                    this.categoriesImportExportService.StoreCategoriesFromJson(questionnaireDocument, collectionsId, textContent);

                    state.Success.AppendLine($"[{zipEntry.FileName}].");
                    state.Success.AppendLine($"    Restored categories '{collectionsId}' for questionnaire '{questionnaireId.FormatGuid()}'.");
                    state.RestoredEntitiesCount++;
                }
            }
            catch (Exception exception)
            {
                this.logger.LogWarning(exception, $"Error processing zip file entry '{zipEntry.FileName}' during questionnaire restore from backup.");
                state.Error = $"Error processing zip file entry '{zipEntry.FileName}'.{Environment.NewLine}{exception}";
                logger.LogError(state.Error);
                throw;
            }
        }
    }
}