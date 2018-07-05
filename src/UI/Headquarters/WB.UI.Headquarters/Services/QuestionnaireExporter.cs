using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zlib;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Enumerator.Native.Questionnaire;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.UI.Shared.Web.Extensions;

namespace WB.UI.Headquarters.Services
{
    class QuestionnaireExporter : IQuestionnaireExporter
    {
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IAttachmentContentService contentService;
        private readonly IPlainKeyValueStorage<QuestionnaireLookupTable> lookupTablesStorage;
        private readonly ITranslationsExportService translationsExportService;
        private readonly ITranslationManagementService translationManagementService;
        private readonly IEntitySerializer<QuestionnaireDocument> serializer;
        private readonly ILogger logger;

        public QuestionnaireExporter(IQuestionnaireStorage questionnaireStorage, IAttachmentContentService contentService,
            IEntitySerializer<QuestionnaireDocument> serializer, IPlainKeyValueStorage<QuestionnaireLookupTable> lookupTablesStorage, 
            ITranslationsExportService translationsExportService, ITranslationManagementService translationManagementService, 
            ILoggerProvider loggerProvider)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.contentService = contentService;
            this.serializer = serializer;
            this.lookupTablesStorage = lookupTablesStorage;
            this.translationsExportService = translationsExportService;
            this.translationManagementService = translationManagementService;
            this.logger = loggerProvider.GetForType(this.GetType());
        }

        [Localizable(false)]
        public File CreateZipExportFile(QuestionnaireIdentity questionnaireIdentity)
        {
            var questionnaire = this.questionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity);
            
            if (questionnaire == null) return null;
            
            var title = questionnaire.Title.ToValidFileName();
            var variable = questionnaire.VariableName ?? title;

            logger.Debug($"Begin export of questionnaire: {title} # {questionnaireIdentity}");

            var output = new MemoryStream();

            using (IZipArchive zipArchive = new IonicZipArchive(output, String.Empty, CompressionLevel.BestCompression, true))
            {
                string questionnaireFolderName = $"{variable} ({questionnaireIdentity.QuestionnaireId.FormatGuid()})";

                void PutEntry(string path, byte[] content) =>
                    zipArchive.CreateEntry($"{questionnaireFolderName}/{path}", content);

                void PutTextFileEntry(string path, string content) => PutEntry(path, Encoding.UTF8.GetBytes(content));

                PutTextFileEntry($"{variable}.json", this.serializer.Serialize(questionnaire));

                for (var attachmentIndex = 0; attachmentIndex < questionnaire.Attachments.Count; attachmentIndex++)
                {
                    var attachment = questionnaire.Attachments[attachmentIndex];
                    
                    try
                    {
                        var attachmentContent = this.contentService.GetAttachmentContent(attachment.ContentId);
                        
                        if (attachmentContent != null)
                        {
                            var attachmentFolder = $"Attachments/{attachment.AttachmentId.FormatGuid()}";
                            var contentExt = attachmentContent.ContentType.Split('/').Last();
                            PutEntry($"{attachmentFolder}/{attachment.Name ?? "unknown-file-name"}.{contentExt}", attachmentContent.Content);
                            PutTextFileEntry($"{attachmentFolder}/Content-Type.txt", attachmentContent.ContentType);
                        }
                        else
                        {
                            PutTextFileEntry(
                                $"Attachments/Invalid/missing attachment #{attachmentIndex + 1} ({attachment.AttachmentId.FormatGuid()}).txt",
                                $"Attachment '{attachment.Name}' is missing.");
                        }
                    }
                    catch (Exception exception)
                    {
                        this.logger.Warn($"Failed to backup attachment #{attachmentIndex + 1} from questionnaire '{title}' ({questionnaireFolderName}).", exception);

                        PutTextFileEntry($"Attachments/Invalid/broken attachment #{attachmentIndex + 1}.txt",
                            $"Failed to backup attachment. See error below.{Environment.NewLine}{exception}");
                    }
                }

                foreach (var lookup in questionnaire.LookupTables)
                {
                    var lookupContent = this.lookupTablesStorage.Get(questionnaireIdentity, lookup.Key);
                    var lookupBytes = Convert.FromBase64String(lookupContent.Content);
                    PutEntry($"Lookup Tables/{lookup.Key.FormatGuid()}.txt", lookupBytes);
                }

                foreach (var translation in questionnaire.Translations)
                {
                    var translations = this.translationManagementService.GetAll(questionnaireIdentity, translation.Id);
                    var translationFile = this.translationsExportService.GenerateTranslationFile(questionnaire,
                        translation.Id,
                        new QuestionnaireTranslation(translations));

                    PutEntry($"Translations/{translation.Id}.xlsx", translationFile.ContentAsExcelFile);
                }
            }

            output.Seek(0, SeekOrigin.Begin);

            this.logger.Info($"Questionnaire {title} # {questionnaireIdentity} exported");

            return new File
            {
                FileStream = output,
                Filename = variable + ".zip"
            };
        }
    }
}
