using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.UI.Designer.Api
{
    [RoutePrefix("translations")]
    public class TranslationsController : ApiController
    {
        private readonly ITranslationsService translationsService;
        private readonly IFileSystemAccessor fileSystemAccessor;

        public TranslationsController(ITranslationsService translationsService,
            IFileSystemAccessor fileSystemAccessor)
        {
            this.translationsService = translationsService;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        [HttpGet]
        [Route("{id:Guid}/template")]
        public HttpResponseMessage Get(Guid id)
        {
            var translationFile = this.translationsService.GetTemplateAsExcelFile(id);
            return this.GetTranslation(translationFile, "xlsx", "application/vnd.oasis.opendocument.spreadsheet");
        }

        [HttpGet]
        [Route("{id:Guid}/xlsx/{translationId:Guid}")]
        public HttpResponseMessage Get(Guid id, Guid translationId )
        {
            var translationFile = this.translationsService.GetAsExcelFile(id, translationId);
            
            return this.GetTranslation(translationFile, "xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        private HttpResponseMessage GetTranslation(TranslationFile translationFile, string fileExtension, string mediaType)
        {
            if (translationFile.ContentAsExcelFile == null) return this.Request.CreateResponse(HttpStatusCode.NotFound);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(translationFile.ContentAsExcelFile)
            };

            var translationName = string.IsNullOrEmpty(translationFile.TranslationName)
                ? "New translation"
                : translationFile.TranslationName;
            var filename = this.fileSystemAccessor.MakeValidFileName($"[{translationName}]{translationFile.QuestionnaireTitle}");

            response.Content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileNameStar = $"{filename}.{fileExtension}"
            };

            return response;
        }
    }
}
