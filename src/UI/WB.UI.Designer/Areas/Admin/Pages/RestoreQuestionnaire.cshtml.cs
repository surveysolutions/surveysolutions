using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;
using Main.Core.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.MembershipProvider.Roles;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.UI.Designer.Extensions;

namespace WB.UI.Designer.Areas.Admin.Pages
{
    [Authorize(Roles = nameof(SimpleRoleEnum.Administrator))]
    public class RestoreQuestionnaireModel : PageModel
    {
        private readonly ILogger<RestoreQuestionnaireModel> logger;
        private readonly ISerializer serializer;
        private readonly ICommandService commandService;
        private readonly ILookupTableService lookupTableService;
        private readonly IAttachmentService attachmentService;
        private readonly ITranslationsService translationsService;
        private readonly DesignerDbContext dbContext;
        private readonly ICategoriesService categoriesService;

        public RestoreQuestionnaireModel(ILogger<RestoreQuestionnaireModel> logger, 
            ISerializer serializer, 
            ICommandService commandService, 
            ILookupTableService lookupTableService, 
            IAttachmentService attachmentService, 
            ITranslationsService translationsService,
            DesignerDbContext dbContext,
            ICategoriesService categoriesService)
        {
            this.logger = logger;
            this.serializer = serializer;
            this.commandService = commandService;
            this.lookupTableService = lookupTableService;
            this.attachmentService = attachmentService;
            this.translationsService = translationsService;
            this.dbContext = dbContext;
            this.categoriesService = categoriesService;
        }

        public void OnGet()
        {
            this.Error = null;
            this.Success = null;
        }

        [BindProperty]
        public IFormFile? Upload { get; set; }

        public IActionResult OnPost()
        {
            try
            {
                var areFileNameAndContentAvailable = Upload?.FileName != null && Upload.Length > 0;
                if (!areFileNameAndContentAvailable)
                {
                    this.Error = "Uploaded file is not specified or empty.";
                    return Page();
                }

                if (Upload!.FileName!.ToLower().EndsWith(".tmpl"))
                {
                    this.Error = "You are trying to restore old format questionnaire. Please use 'Old Format Restore' option.";
                    return Page();
                }

                if (!Upload.FileName.ToLower().EndsWith(".zip"))
                {
                    this.Error = "Only zip archives are supported. Please upload correct zip backup.";
                    return Page();
                }

                var state = new RestoreState();

                var openReadStream = Upload.OpenReadStream();
                using (var zipStream = new ZipInputStream(openReadStream))
                {
                    ZipEntry zipEntry = zipStream.GetNextEntry();

                    while (zipEntry != null)
                    {
                        this.RestoreDataFromZipFileEntry(zipEntry, zipStream, state);

                        zipEntry = zipStream.GetNextEntry();
                    }
                }

                foreach (Guid attachmentId in state.GetPendingAttachments())
                {
                    this.Error = "";
                    this.Error += $"Attachment '{attachmentId.FormatGuid()}' was not restored because there are not enough data for it in it's folder.";
                }

                this.Success = $"Restore finished. Restored {state.RestoredEntitiesCount} entities.";

                return Page();
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "Unexpected error occurred during restore of questionnaire from backup.");
                this.Error = $"Unexpected error occurred.{Environment.NewLine}{exception}";
                return Page();
            }
        }

        [TempData]
        public string? Success { get; set; } = string.Empty;

        [TempData]
        public string? Error { get; set; } = string.Empty;

        private void RestoreDataFromZipFileEntry(ZipEntry zipEntry, ZipInputStream zipStream, RestoreState state)
        {
            try
            {
                string[] zipEntryPathChunks = zipEntry.FileName.Split(
                    new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                    StringSplitOptions.RemoveEmptyEntries);

                Guid questionnaireId;
                bool isInsideQuestionnaireFolder = Guid.TryParse(
                    zipEntryPathChunks[0].Split(new[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries).Last(),
                    out questionnaireId);

                if (!isInsideQuestionnaireFolder)
                {
                    return;
                }

                bool isQuestionnaireDocumentEntry =
                    zipEntryPathChunks.Length == 2 &&
                    zipEntryPathChunks[1].ToLower().EndsWith(".json");

                bool isAttachmentEntry =
                    zipEntryPathChunks.Length == 4 &&
                    string.Equals(zipEntryPathChunks[1], "attachments", StringComparison.CurrentCultureIgnoreCase);

                bool isLookupTableEntry =
                    zipEntryPathChunks.Length == 3 &&
                    string.Equals(zipEntryPathChunks[1], "lookup tables", StringComparison.CurrentCultureIgnoreCase) &&
                    zipEntryPathChunks[2].ToLower().EndsWith(".txt");

                bool isTranslationEntry =
                    zipEntryPathChunks.Length == 3 &&
                    string.Equals(zipEntryPathChunks[1], "translations", StringComparison.CurrentCultureIgnoreCase) &&
                    (".xlsx".Equals(Path.GetExtension(zipEntryPathChunks[2]), StringComparison.InvariantCultureIgnoreCase) ||
                    ".ods".Equals(Path.GetExtension(zipEntryPathChunks[2]), StringComparison.InvariantCultureIgnoreCase));

                bool isCollectionsEntry =
                    zipEntryPathChunks.Length == 3 &&
                    string.Equals(zipEntryPathChunks[1], "categories", StringComparison.CurrentCultureIgnoreCase) &&
                    (".xlsx".Equals(Path.GetExtension(zipEntryPathChunks[2]), StringComparison.InvariantCultureIgnoreCase) ||
                     ".ods".Equals(Path.GetExtension(zipEntryPathChunks[2]), StringComparison.InvariantCultureIgnoreCase));

                if (isQuestionnaireDocumentEntry)
                {
                    string textContent = new StreamReader(zipStream, Encoding.UTF8).ReadToEnd();

                    var questionnaireDocument = this.serializer.Deserialize<QuestionnaireDocument>(textContent);
                    questionnaireDocument.PublicKey = questionnaireId;

                    var command = new ImportQuestionnaire(
                        User.GetId(), questionnaireDocument);
                    this.commandService.Execute(command);

                    this.translationsService.DeleteAllByQuestionnaireId(questionnaireDocument.PublicKey);
                    this.categoriesService.DeleteAllByQuestionnaireId(questionnaireDocument.PublicKey);

                    this.Success += $"[{zipEntry.FileName}]";
                    this.Success += $"    Restored questionnaire document '{questionnaireDocument.Title}' with id '{questionnaireDocument.PublicKey.FormatGuid()}'.";
                    state.RestoredEntitiesCount++;
                }
                else if (isAttachmentEntry)
                {
                    var attachmentId = Guid.Parse(zipEntryPathChunks[2]);
                    var fileName = zipEntryPathChunks[3];

                    if (string.Equals(fileName, "content-type.txt", StringComparison.CurrentCultureIgnoreCase))
                    {
                        string textContent = new StreamReader(zipStream, Encoding.UTF8).ReadToEnd();

                        state.StoreAttachmentContentType(attachmentId, textContent);
                        this.Success += $"[{zipEntry.FileName}]";
                        this.Success += $"    Found content-type '{textContent}' for attachment '{attachmentId.FormatGuid()}'.";
                    }
                    else
                    {
                        byte[] binaryContent = zipStream.ReadToEnd();

                        state.StoreAttachmentFile(attachmentId, fileName, binaryContent);
                        this.Success += $"[{zipEntry.FileName}]";
                        this.Success += $"    Found file data '{fileName}' for attachment '{attachmentId.FormatGuid()}'.";
                    }

                    var attachment = state.GetAttachment(attachmentId);

                    if (attachment.HasAllDataForRestore())
                    {
                        string attachmentContentId = this.attachmentService.CreateAttachmentContentId(attachment.BinaryContent!);

                        this.attachmentService.SaveContent(attachmentContentId, attachment.ContentType!, attachment.BinaryContent!);
                        this.attachmentService.SaveMeta(attachmentId, questionnaireId, attachmentContentId, attachment.FileName!);

                        state.RemoveAttachment(attachmentId);

                        this.Success += $"    Restored attachment '{attachmentId.FormatGuid()}' for questionnaire '{questionnaireId.FormatGuid()}' using file '{attachment.FileName}' and content-type '{attachment.ContentType}'.";
                        state.RestoredEntitiesCount++;
                    }
                }
                else if (isLookupTableEntry)
                {
                    var lookupTableId = Guid.Parse(Path.GetFileNameWithoutExtension(zipEntryPathChunks[2]));

                    string textContent = new StreamReader(zipStream, Encoding.UTF8).ReadToEnd();

                    this.lookupTableService.SaveLookupTableContent(questionnaireId, lookupTableId, textContent);

                    this.Success += $"[{zipEntry.FileName}].";
                    this.Success += $"    Restored lookup table '{lookupTableId.FormatGuid()}' for questionnaire '{questionnaireId.FormatGuid()}'.";
                    state.RestoredEntitiesCount++;
                }
                else if (isTranslationEntry)
                {
                    var translationIdString = Path.GetFileNameWithoutExtension(zipEntryPathChunks[2]);
                    byte[]? excelContent = null;

                    using (var memoryStream = new MemoryStream())
                    {
                        zipStream.CopyTo(memoryStream);
                        excelContent = memoryStream.ToArray();
                    }

                    var translationId = Guid.Parse(translationIdString);
                    this.translationsService.Store(questionnaireId, translationId, excelContent);

                    this.Success += $"[{zipEntry.FileName}].";
                    this.Success += $"    Restored translation '{translationId}' for questionnaire '{questionnaireId.FormatGuid()}'.";
                    state.RestoredEntitiesCount++;
                }
                else if (isCollectionsEntry)
                {
                    var collectionsIdString = Path.GetFileNameWithoutExtension(zipEntryPathChunks[2]);
                    var collectionsId = Guid.Parse(collectionsIdString);

                    this.categoriesService.Store(questionnaireId, collectionsId, zipStream, CategoriesFileType.Excel);

                    this.Success += $"[{zipEntry.FileName}].";
                    this.Success += $"    Restored categories '{collectionsId}' for questionnaire '{questionnaireId.FormatGuid()}'.";
                    state.RestoredEntitiesCount++;
                }
                else
                {
                }

                dbContext.SaveChanges();
            }
            catch (Exception exception)
            {
                this.logger.LogWarning(exception, $"Error processing zip file entry '{zipEntry.FileName}' during questionnaire restore from backup.");
                this.Error += $"Error processing zip file entry '{zipEntry.FileName}'.{Environment.NewLine}{exception}";
            }
        }

        private class RestoreState
        {
            public class Attachment
            {
                public string? FileName { get; set; }
                public string? ContentType { get; set; }
                public byte[]? BinaryContent { get; set; }

                public bool HasAllDataForRestore()
                    => this.FileName != null
                       && this.ContentType != null
                       && this.BinaryContent != null;
            }

            private readonly Dictionary<Guid, Attachment> attachments = new Dictionary<Guid, Attachment>();

            public int RestoredEntitiesCount { get; set; }

            public void StoreAttachmentContentType(Guid attachmentId, string contentType)
            {
                this.attachments.GetOrAdd(attachmentId).ContentType = contentType;
            }

            public void StoreAttachmentFile(Guid attachmentId, string fileName, byte[] binaryContent)
            {
                this.attachments.GetOrAdd(attachmentId).FileName = fileName;
                this.attachments.GetOrAdd(attachmentId).BinaryContent = binaryContent;
            }

            public Attachment GetAttachment(Guid attachmentId)
                => this.attachments[attachmentId];

            public void RemoveAttachment(Guid attachmentId)
                => this.attachments.Remove(attachmentId);

            public IEnumerable<Guid> GetPendingAttachments()
                => this.attachments.Keys;
        }
    }
}
