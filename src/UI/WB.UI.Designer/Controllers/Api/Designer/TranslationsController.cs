using System;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.UI.Designer.Controllers.Api.Designer
{
    [AuthorizeOrAnonymousQuestionnaire]
    [QuestionnairePermissions]
    [Route("translations")]
    public class TranslationsController : Controller
    {
        private readonly IDesignerTranslationService translationsService;
        private readonly IFileSystemAccessor fileSystemAccessor;

        public TranslationsController(IDesignerTranslationService translationsService,
            IFileSystemAccessor fileSystemAccessor)
        {
            this.translationsService = translationsService;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        [HttpGet]
        [Route("{id:Guid}/template")]
        public IActionResult Get(QuestionnaireRevision id)
        {
            var translationFile = this.translationsService.GetTemplateAsExcelFile(id);
            return this.GetTranslation(translationFile, "xlsx", "application/vnd.oasis.opendocument.spreadsheet");
        }

        [HttpGet]
        [Route("{id:Guid}/xlsx/{translationId:Guid}")]
        public IActionResult Get(QuestionnaireRevision id, Guid translationId )
        {
            var translationFile = this.translationsService.GetAsExcelFile(id, translationId);

            var actionResult = this.GetTranslation(translationFile, "xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            return actionResult;
        }

        private IActionResult GetTranslation(TranslationFile translationFile, string fileExtension, string mediaType)
        {
            if (translationFile.ContentAsExcelFile == null) return NotFound();

            var translationName = string.IsNullOrEmpty(translationFile.TranslationName)
                ? "New translation"
                : translationFile.TranslationName;
            var filename = this.fileSystemAccessor.MakeValidFileName($"[{translationName}]{translationFile.QuestionnaireTitle}");

            return File(translationFile.ContentAsExcelFile, mediaType, $"{filename}.{fileExtension}");
        }
    }
}
