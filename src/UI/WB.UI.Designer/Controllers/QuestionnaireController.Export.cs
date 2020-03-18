using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.UI.Designer.Extensions;

namespace WB.UI.Designer.Controllers
{
    public partial class QuestionnaireController
    {
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public FileResult Backup(Guid id)
        {
            var questionnaireView = questionnaireViewFactory.Load(new QuestionnaireViewInputModel(id));
            if (questionnaireView == null)
                return null;

            var questionnaireFileName = Path.GetInvalidFileNameChars()
                .Aggregate(questionnaireView.Title.Substring(0, Math.Min(questionnaireView.Title.Length, 255)),
                    (current, c) => current.Replace(c, 'x'));

            var questionnaireDocument = questionnaireView.Source;

            string questionnaireJson = this.serializer.Serialize(questionnaireDocument);

            var output = new MemoryStream();

            ZipOutputStream zipStream = new ZipOutputStream(output);

            string questionnaireFolderName = $"{questionnaireFileName} ({id.FormatGuid()})";

            zipStream.PutTextFileEntry($"{questionnaireFolderName}/{questionnaireFileName}.json", questionnaireJson);

            for (int attachmentIndex = 0; attachmentIndex < questionnaireDocument.Attachments.Count; attachmentIndex++)
            {
                try
                {
                    Attachment attachmentReference = questionnaireDocument.Attachments[attachmentIndex];

                    var attachmentContent = this.attachmentService.GetContent(attachmentReference.ContentId);

                    if (attachmentContent?.Content != null)
                    {
                        var attachmentMeta = this.attachmentService.GetAttachmentMeta(attachmentReference.AttachmentId);

                        zipStream.PutFileEntry($"{questionnaireFolderName}/Attachments/{attachmentReference.AttachmentId.FormatGuid()}/{attachmentMeta?.FileName ?? "unknown-file-name"}", attachmentContent.Content);
                        zipStream.PutTextFileEntry($"{questionnaireFolderName}/Attachments/{attachmentReference.AttachmentId.FormatGuid()}/Content-Type.txt", attachmentContent.ContentType);
                    }
                    else
                    {
                        zipStream.PutTextFileEntry(
                            $"{questionnaireFolderName}/Attachments/Invalid/missing attachment #{attachmentIndex + 1} ({attachmentReference.AttachmentId.FormatGuid()}).txt",
                            $"Attachment '{attachmentReference.Name}' is missing.");
                    }
                }
                catch (Exception exception)
                {
                    this.logger.LogWarning($"Failed to backup attachment #{attachmentIndex + 1} from questionnaire '{questionnaireView.Title}' ({questionnaireFolderName}).", exception);
                    zipStream.PutTextFileEntry(
                        $"{questionnaireFolderName}/Attachments/Invalid/broken attachment #{attachmentIndex + 1}.txt",
                        $"Failed to backup attachment. See error below.{Environment.NewLine}{exception}");
                }
            }

            Dictionary<Guid, string> lookupTables = this.lookupTableService.GetQuestionnairesLookupTables(id);

            foreach (KeyValuePair<Guid, string> lookupTable in lookupTables)
            {
                zipStream.PutTextFileEntry($"{questionnaireFolderName}/Lookup Tables/{lookupTable.Key.FormatGuid()}.txt", lookupTable.Value);
            }

            foreach (var translation in questionnaireDocument.Translations)
            {
                TranslationFile excelFile = this.translationsService.GetAsExcelFile(id, translation.Id);
                zipStream.PutFileEntry($"{questionnaireFolderName}/Translations/{translation.Id.FormatGuid()}.xlsx", excelFile.ContentAsExcelFile);
            }

            foreach (var categories in questionnaireDocument.Categories)
            {
                var excelFile = this.categoriesService.GetAsExcelFile(id, categories.Id);
                zipStream.PutFileEntry($"{questionnaireFolderName}/Categories/{categories.Id.FormatGuid()}.xlsx", excelFile.Content);
            }

            zipStream.Finish();
            output.Position = 0;
            

            return File(output, "application/zip", $"{questionnaireFileName}.zip");
        }
    }
}
