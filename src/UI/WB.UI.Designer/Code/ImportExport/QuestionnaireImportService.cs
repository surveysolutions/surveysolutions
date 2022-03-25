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
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ImportExport;
using WB.Core.BoundedContexts.Designer.ImportExport.Models;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Services.Restore;
using Group = WB.Core.BoundedContexts.Designer.ImportExport.Models.Group;

namespace WB.UI.Designer.Code.ImportExport
{
    public class QuestionnaireImportService : IQuestionnaireImportService
    {
        private readonly ILogger<QuestionnaireImportService> logger;
        private readonly ICommandService commandService;
        private readonly ILookupTableService lookupTableService;
        private readonly IAttachmentService attachmentService;
        private readonly ITranslationsService translationsService;
        private readonly ITranslationImportExportService translationImportExportService;
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
            this.categoriesService = categoriesService;
            this.importExportQuestionnaireMapper = importExportQuestionnaireMapper;
            this.questionnaireStorage = questionnaireStorage;
            this.questionnaireSerializer = questionnaireSerializer;
            this.categoriesImportExportService = categoriesImportExportService;
        }
        
        private class ImportStructure
        {
            public Dictionary<Guid, byte[]> Attachments { get; set; } = new Dictionary<Guid, byte[]>();
            public Dictionary<Guid, string> LookupTables { get; set; } = new Dictionary<Guid, string>();
            public Dictionary<Guid, string> Translations { get; set; } = new Dictionary<Guid, string>();
            public Dictionary<Guid, string> Categories { get; set; } = new Dictionary<Guid, string>();
        }

        public Guid RestoreQuestionnaire(Stream archive, Guid responsibleId, RestoreState state, bool createNew)
        {
            using var zipStream = new ZipInputStream(archive);

            var questionnaire = RestoreQuestionnaireFromZipFileOrThrow(zipStream, state);
            var questionnaireDocument = importExportQuestionnaireMapper.Map(questionnaire);

            var verificationMessages = ValidateQuestionnaireDocument(questionnaireDocument);
            var verificationErrors = verificationMessages.Where(m => m.MessageLevel >= VerificationMessageLevel.General).ToList();
            
            if (verificationErrors.Any())
            {
                state.Error = string.Join("\r\n", verificationErrors.Select(v => $"[{v.Code}] {v.Message} - {v.References.FirstOrDefault()?.Id}").Take(20).ToArray());
                throw new Exception("Please fix verification errors");
            }

            if (createNew)
            {
                var publicKey = Guid.NewGuid();
                questionnaireDocument.Id = publicKey.FormatGuid();
                questionnaireDocument.PublicKey = publicKey;
                questionnaireDocument.CreationDate = DateTime.UtcNow;
                questionnaireDocument.CreatedBy = responsibleId;
            }
            
            state.Success.AppendLine($"[document.json]");
            state.Success.AppendLine($"    Restored questionnaire document '{questionnaireDocument.Title}' with id '{questionnaireDocument.PublicKey.FormatGuid()}'.");
            state.RestoredEntitiesCount++;

            ImportStructure importStructure = GetStructure(zipStream, questionnaire, questionnaireDocument, state);

            this.translationsService.DeleteAllByQuestionnaireId(questionnaireDocument.PublicKey);
            this.categoriesService.DeleteAllByQuestionnaireId(questionnaireDocument.PublicKey);
           
            this.RestoreDataFromZipFileEntry(importStructure, state, questionnaire, questionnaireDocument);

            var command = new ImportQuestionnaire(responsibleId, questionnaireDocument);
            this.commandService.Execute(command);

            return questionnaireDocument.PublicKey;
        } 

        private List<QuestionnaireVerificationMessage> ValidateQuestionnaireDocument(QuestionnaireDocument questionnaireDocument)
        {
            var readOnlyQuestionnaireDocument = questionnaireDocument.AsReadOnly();
            var verifier = new ImportQuestionnaireVerifier();
            var result = verifier.Verify(readOnlyQuestionnaireDocument).ToList();
            return result;
        }

        private ImportStructure GetStructure(ZipInputStream zipStream, Questionnaire questionnaire,
            QuestionnaireDocument questionnaireDocument, RestoreState state)
        {
            ImportStructure importStructure = new ImportStructure();
            
            foreach (var attachmentInfo in questionnaire.Attachments)
            {
                var attachment = questionnaireDocument.Attachments.Single(s =>
                    s.Name == attachmentInfo.Name);
                var attachmentId = attachment.AttachmentId;
                var zipEntity = GetZipEntity(zipStream, "attachments", attachmentInfo.FileName, state);
                byte[] binaryContent = zipStream.ReadToEnd();
                importStructure.Attachments.Add(attachmentId, binaryContent);
            }

            foreach (var lookupTableInfo in questionnaire.LookupTables)
            {
                var lookupTable = questionnaireDocument.LookupTables.Single(s =>
                    s.Value.TableName == lookupTableInfo.TableName);
                var lookupTableId = lookupTable.Key;
                var zipEntity = GetZipEntity(zipStream, "lookup tables", lookupTableInfo.FileName, state);
                string fileContent = new StreamReader(zipStream, Encoding.UTF8).ReadToEnd();
                importStructure.LookupTables.Add(lookupTableId, fileContent);
            }

            foreach (var translationInfo in questionnaire.Translations.Items)
            {
                var translation = questionnaireDocument.Translations.Single(s =>
                    s.Name == translationInfo.Name);
                var translationId = translation.Id;
                var zipEntity = GetZipEntity(zipStream, "translations", translationInfo.FileName, state);
                string fileContent = new StreamReader(zipStream, Encoding.UTF8).ReadToEnd();
                importStructure.Translations.Add(translationId, fileContent);
            }

            foreach (var categoriesInfo in questionnaire.Categories)
            {
                var categories = questionnaireDocument.Categories.Single(s =>
                    s.Name == categoriesInfo.Name);
                var categoriesId = categories.Id;
                var zipEntity = GetZipEntity(zipStream, "categories", categoriesInfo.FileName, state);
                string fileContent = new StreamReader(zipStream, Encoding.UTF8).ReadToEnd();
                importStructure.Categories.Add(categoriesId, fileContent);
            }

            return importStructure;
        }

        private ZipEntry GetZipEntity(ZipInputStream zipStream, string folder, string fileName, RestoreState state)
        {
            zipStream.Seek(0, SeekOrigin.Begin);

            ZipEntry zipEntry;
            while ((zipEntry = zipStream.GetNextEntry()) != null)
            {
                if (zipEntry.IsDirectory) 
                    continue;
                
                string[] zipEntryPathChunks = zipEntry.FileName.Split(
                    new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                    StringSplitOptions.RemoveEmptyEntries);

                bool isFound =
                    zipEntryPathChunks.Length == 2 &&
                    string.Equals(zipEntryPathChunks[0], folder, StringComparison.CurrentCultureIgnoreCase) &&
                    string.Equals(zipEntryPathChunks[1], fileName, StringComparison.CurrentCultureIgnoreCase);

                if (isFound)
                    return zipEntry;
            }

            var message = $"File {fileName} in folder {folder} don't found";
            state.Error = message;
            throw new ArgumentException(message);
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

        private Questionnaire RestoreQuestionnaireFromZipFileOrThrow(ZipInputStream zipStream, RestoreState state)
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
                    var message = $"Error processing questionnaire document file entry '{zipEntry.FileName}' during questionnaire restore from backup.";
                    state.Error = message;
                    logger.LogError(exception, message);
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
            catch (JsonReaderException)
            {
                state.Error = "Questionnaire has invalid format.";
                throw;
            }

            var questionnaire = questionnaireSerializer.Deserialize(textContent);
            return questionnaire;
        }

        private void RestoreDataFromZipFileEntry(ImportStructure importStructure, RestoreState state, Questionnaire questionnaire, QuestionnaireDocument questionnaireDocument)
        {
            var questionnaireId = questionnaireDocument.PublicKey;
            
            foreach (var attachmentRecord in importStructure.Attachments)
            {
                try
                {
                    var attachment = questionnaireDocument.Attachments.Single(s =>
                        s.AttachmentId == attachmentRecord.Key);
                    var attachmentInfo = questionnaire.Attachments.Single(a => a.Name == attachment.Name);
                    var attachmentId = attachment.AttachmentId;
                    byte[] binaryContent = attachmentRecord.Value;
                    string attachmentContentId = this.attachmentService.CreateAttachmentContentId(binaryContent);
                    attachment.ContentId = attachmentContentId;

                    this.attachmentService.SaveContent(attachmentContentId, attachmentInfo.ContentType!, binaryContent);
                    this.attachmentService.SaveMeta(attachmentId, questionnaireId, attachmentContentId,
                        attachmentInfo.FileName);

                    state.Success.AppendLine($"[Attachment {attachmentInfo.FileName}]");
                    state.Success.AppendLine($"    Found file data '{attachmentInfo.FileName}' for attachment '{attachmentId.FormatGuid()}'.");
                    state.Success.AppendLine($"    Restored attachment '{attachmentId.FormatGuid()}' for questionnaire '{questionnaireId.FormatGuid()}' using file '{attachmentInfo.FileName}' and content-type '{attachmentInfo.ContentType}'.");
                    state.RestoredEntitiesCount++;
                }
                catch (Exception exception)
                {
                    var error = $"Error processing attachment file entry '{attachmentRecord.Key}'  during questionnaire restore from backup.";
                    state.Error = error;
                    logger.LogError(exception, error);
                    throw;
                }
            }

            foreach (var lookupTableRecord in importStructure.LookupTables)
            {
                try
                {
                    string textContent = lookupTableRecord.Value;

                    this.lookupTableService.SaveLookupTableContent(questionnaireId, lookupTableRecord.Key, textContent);

                    state.Success.AppendLine($"[Lookup table - {lookupTableRecord.Key}].");
                    state.Success.AppendLine($"    Restored lookup table '{lookupTableRecord.Key.FormatGuid()}' for questionnaire '{questionnaireId.FormatGuid()}'.");
                    state.RestoredEntitiesCount++;
                }
                catch (Exception exception)
                {
                    var message = $"Error processing lookup table file entry '{lookupTableRecord.Key}' during questionnaire restore from backup.";
                    state.Error = message;
                    logger.LogError(exception, message);
                    throw;
                }
            }

            foreach (var translationRecord in importStructure.Translations)
            {
                try
                {
                    string jsonContent = translationRecord.Value;
                    this.translationImportExportService.StoreTranslationsFromJson(questionnaireDocument,
                        translationRecord.Key, jsonContent);

                    state.Success.AppendLine($"[Translation - {translationRecord.Key}].");
                    state.Success.AppendLine($"    Restored translation '{translationRecord.Key}' for questionnaire '{questionnaireId.FormatGuid()}'.");
                    state.RestoredEntitiesCount++;
                }
                catch (Exception exception)
                {
                    var error = $"Error processing translation file entry '{translationRecord.Key}'  during questionnaire restore from backup.";
                    state.Error = error;
                    logger.LogError(exception, error);
                    throw;
                }
            }

            foreach (var categoryRecord in importStructure.Categories)
            {
                try
                {
                    string jsonContent = categoryRecord.Value;
                    this.categoriesImportExportService.StoreCategoriesFromJson(questionnaireDocument.PublicKey,
                        categoryRecord.Key, jsonContent);

                    state.Success.AppendLine($"[Categories - {categoryRecord.Key}].");
                    state.Success.AppendLine($"    Restored categories '{categoryRecord.Key}' for questionnaire '{questionnaireId.FormatGuid()}'.");
                    state.RestoredEntitiesCount++;
                }
                catch (Exception exception)
                {
                    var error = $"Error processing categories file entry '{categoryRecord.Key}'  during questionnaire restore from backup.";
                    state.Error = error;
                    logger.LogError(exception, error);
                    throw;
                }
            }
        }
    }
}
