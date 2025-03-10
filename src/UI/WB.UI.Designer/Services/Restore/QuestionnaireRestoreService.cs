﻿using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Main.Core.Documents;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.ImportExport;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;

namespace WB.UI.Designer.Services.Restore
{
    class QuestionnaireRestoreService : IQuestionnaireRestoreService
    {
        private readonly ILogger<QuestionnaireRestoreService> logger;
        private readonly ISerializer serializer;
        private readonly ICommandService commandService;
        private readonly ILookupTableService lookupTableService;
        private readonly IAttachmentService attachmentService;
        private readonly ITranslationsService translationsService;
        private readonly DesignerDbContext dbContext;
        private readonly IReusableCategoriesService reusableCategoriesService;
        private readonly IImportExportQuestionnaireMapper importExportQuestionnaireMapper;
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage;

        public QuestionnaireRestoreService(ILogger<QuestionnaireRestoreService> logger, ISerializer serializer, ICommandService commandService, ILookupTableService lookupTableService, IAttachmentService attachmentService, ITranslationsService translationsService, DesignerDbContext dbContext, IReusableCategoriesService reusableCategoriesService, IImportExportQuestionnaireMapper importExportQuestionnaireMapper, IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage)
        {
            this.logger = logger;
            this.serializer = serializer;
            this.commandService = commandService;
            this.lookupTableService = lookupTableService;
            this.attachmentService = attachmentService;
            this.translationsService = translationsService;
            this.dbContext = dbContext;
            this.reusableCategoriesService = reusableCategoriesService;
            this.importExportQuestionnaireMapper = importExportQuestionnaireMapper;
            this.questionnaireStorage = questionnaireStorage;
        }

        public Guid RestoreQuestionnaire(Stream archive, Guid responsibleId, RestoreState state, bool createNew)
        {
            using var zipStream = new ZipArchive(archive);

            var questionnaire = RestoreQuestionnaireFromZipFileOrThrow(zipStream, responsibleId, state, createNew);

            foreach (var zipEntry in zipStream.Entries)
            {            
                this.RestoreDataFromZipFileEntry(zipEntry, zipStream, responsibleId, state, questionnaire);
            }

            state.Error = "";
            foreach (Guid attachmentId in state.GetPendingAttachments())
            {
                state.Error += $"Attachment '{attachmentId.FormatGuid()}' was not restored because there are not enough data for it in it's folder." + Environment.NewLine;
            }

            questionnaireStorage.Store(questionnaire, questionnaire.Id);
            dbContext.SaveChanges();

            return questionnaire.PublicKey;
        }

        public QuestionnaireDocument RestoreQuestionnaireFromZipFileOrThrow(ZipArchive zipStream, Guid responsibleId,
            RestoreState state, bool createNew)
        {
            foreach (var zipEntry in zipStream.Entries)
            {
                if (zipEntry.IsDirectory()) continue;
                
                try
                {
                    string[] zipEntryPathChunks = zipEntry.FullName.Split(
                        new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                        StringSplitOptions.RemoveEmptyEntries);

                    if (zipEntryPathChunks.Length == 1 && zipEntryPathChunks[0].ToLower().Equals("document.json"))
                    {
                        byte[] binaryContent;
                        using (var memoryStream = new MemoryStream())
                        {
                            zipEntry.Open().CopyTo(memoryStream);
                            binaryContent = memoryStream.ToArray();
                        }
                        
                        string textContent = Encoding.UTF8.GetString(binaryContent);
                        //string textContent = new StreamReader(zipStream, Encoding.UTF8).ReadToEnd();
                        
                        var questionnaireDocument = this.serializer.Deserialize<QuestionnaireDocument>(textContent);

                        if (createNew)
                            questionnaireDocument.PublicKey = Guid.NewGuid();

                        this.translationsService.DeleteAllByQuestionnaireId(questionnaireDocument.PublicKey);
                        this.reusableCategoriesService.DeleteAllByQuestionnaireId(questionnaireDocument.PublicKey);

                        state.Success.AppendLine($"[{zipEntry.FullName}]");
                        state.Success.AppendLine($"    Restored questionnaire document '{questionnaireDocument.Title}' with id '{questionnaireDocument.PublicKey.FormatGuid()}'.");
                        state.RestoredEntitiesCount++;

                        var command = new ImportQuestionnaire(responsibleId, questionnaireDocument);
                        this.commandService.Execute(command);

                        dbContext.SaveChanges();

                        return questionnaireDocument;
                    }
                }
                catch (Exception exception)
                {
                        this.logger.LogWarning(exception, $"Error processing zip file entry '{zipEntry.FullName}' during questionnaire restore from backup.");
                        state.Error = $"Error processing zip file entry '{zipEntry.FullName}'.{Environment.NewLine}{exception}";
                        logger.LogError(state.Error);
                }
            }

            state.Error = "Questionnaire document was not found.";
            throw new Exception("Questionnaire document was not found.");
        }

        public void RestoreDataFromZipFileEntry(ZipArchiveEntry zipEntry, ZipArchive zipStream, Guid responsibleId, RestoreState state, QuestionnaireDocument questionnaire)
        {
            var questionnaireId = questionnaire.PublicKey;
            try
            {
                string[] zipEntryPathChunks = zipEntry.FullName.Split(
                    new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                    StringSplitOptions.RemoveEmptyEntries);

                bool isAttachmentEntry =
                    zipEntryPathChunks.Length == 3 &&
                    string.Equals(zipEntryPathChunks[0], "attachments", StringComparison.CurrentCultureIgnoreCase);

                bool isLookupTableEntry =
                    zipEntryPathChunks.Length == 2 &&
                    string.Equals(zipEntryPathChunks[0], "lookup tables", StringComparison.CurrentCultureIgnoreCase) &&
                    zipEntryPathChunks[1].ToLower().EndsWith(".txt");

                bool isTranslationEntry =
                    zipEntryPathChunks.Length == 2 &&
                    string.Equals(zipEntryPathChunks[0], "translations", StringComparison.CurrentCultureIgnoreCase) &&
                    (".xlsx".Equals(Path.GetExtension(zipEntryPathChunks[1]), StringComparison.InvariantCultureIgnoreCase) ||
                     ".ods".Equals(Path.GetExtension(zipEntryPathChunks[1]), StringComparison.InvariantCultureIgnoreCase));

                bool isCollectionsEntry =
                    zipEntryPathChunks.Length == 2 &&
                    string.Equals(zipEntryPathChunks[0], "categories", StringComparison.CurrentCultureIgnoreCase) &&
                    (".xlsx".Equals(Path.GetExtension(zipEntryPathChunks[1]), StringComparison.InvariantCultureIgnoreCase) ||
                     ".ods".Equals(Path.GetExtension(zipEntryPathChunks[1]), StringComparison.InvariantCultureIgnoreCase));

                if (isAttachmentEntry)
                {
                    var attachmentId = Guid.Parse(zipEntryPathChunks[1]);
                    var fileName = zipEntryPathChunks[2];

                    if (string.Equals(fileName, "content-type.txt", StringComparison.CurrentCultureIgnoreCase))
                    {
                        string textContent = Encoding.UTF8.GetString(zipEntry.GetContent());
                        //string textContent = new StreamReader(zipStream, Encoding.UTF8).ReadToEnd();

                        state.StoreAttachmentContentType(attachmentId, textContent);
                        state.Success.AppendLine($"[{zipEntry.FullName}]");
                        state.Success.AppendLine($"    Found content-type '{textContent}' for attachment '{attachmentId.FormatGuid()}'.");
                    }
                    else
                    {
                        state.StoreAttachmentFile(attachmentId, fileName, zipEntry.GetContent());
                        state.Success.AppendLine($"[{zipEntry.FullName}]");
                        state.Success.AppendLine($"    Found file data '{fileName}' for attachment '{attachmentId.FormatGuid()}'.");
                    }

                    var attachment = state.GetAttachment(attachmentId);

                    if (attachment.HasAllDataForRestore())
                    {
                        string attachmentContentId = this.attachmentService.CreateAttachmentContentId(attachment.BinaryContent!);
                        questionnaire.Attachments
                            .Single(s => s.AttachmentId == attachmentId)
                            .ContentId = attachmentContentId;
                        
                        this.attachmentService.SaveContent(attachmentContentId, attachment.ContentType!, attachment.BinaryContent!);
                        this.attachmentService.SaveMeta(attachmentId, questionnaireId, attachmentContentId, attachment.FileName!);

                        state.RemoveAttachment(attachmentId);

                        state.Success.AppendLine($"    Restored attachment '{attachmentId.FormatGuid()}' for questionnaire '{questionnaireId.FormatGuid()}' using file '{attachment.FileName}' and content-type '{attachment.ContentType}'.");
                        state.RestoredEntitiesCount++;
                    }
                }
                else if (isLookupTableEntry)
                {
                    var lookupTableId = Guid.Parse(Path.GetFileNameWithoutExtension(zipEntryPathChunks[1]));

                    string textContent = Encoding.UTF8.GetString(zipEntry.GetContent());
                    //string textContent = new StreamReader(zipStream, Encoding.UTF8).ReadToEnd();

                    this.lookupTableService.SaveLookupTableContent(questionnaireId, lookupTableId, textContent);

                    state.Success.AppendLine($"[{zipEntry.FullName}].");
                    state.Success.AppendLine($"    Restored lookup table '{lookupTableId.FormatGuid()}' for questionnaire '{questionnaireId.FormatGuid()}'.");
                    state.RestoredEntitiesCount++;
                }
                else if (isTranslationEntry)
                {
                    var translationIdString = Path.GetFileNameWithoutExtension(zipEntryPathChunks[1]);
                    var translationId = Guid.Parse(translationIdString);
                    
                    this.translationsService.Store(questionnaireId, translationId, zipEntry.GetContent());

                    state.Success.AppendLine($"[{zipEntry.FullName}].");
                    state.Success.AppendLine($"    Restored translation '{translationId}' for questionnaire '{questionnaireId.FormatGuid()}'.");
                    state.RestoredEntitiesCount++;
                }
                else if (isCollectionsEntry)
                {
                    var collectionsIdString = Path.GetFileNameWithoutExtension(zipEntryPathChunks[1]);
                    var collectionsId = Guid.Parse(collectionsIdString);

                    using var entryStream = zipEntry.Open();
                    using (var memoryStream = new MemoryStream())
                    {
                        entryStream.CopyTo(memoryStream);
                        memoryStream.Seek(0, SeekOrigin.Begin);

                        this.reusableCategoriesService.Store(questionnaireId, collectionsId, memoryStream,
                            CategoriesFileType.Excel);
                    }

                    state.Success.AppendLine($"[{zipEntry.FullName}].");
                    state.Success.AppendLine($"    Restored categories '{collectionsId}' for questionnaire '{questionnaireId.FormatGuid()}'.");
                    state.RestoredEntitiesCount++;
                }
                else
                {
                }

                dbContext.SaveChanges();
            }
            catch (Exception exception)
            {
                this.logger.LogWarning(exception, $"Error processing zip file entry '{zipEntry.FullName}' during questionnaire restore from backup.");
                state.Error = $"Error processing zip file entry '{zipEntry.FullName}'.{Environment.NewLine}{exception}";
                logger.LogError(state.Error);
            }
        }
    }
}
