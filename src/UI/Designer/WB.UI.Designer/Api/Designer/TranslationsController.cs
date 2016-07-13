using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.Infrastructure.FileSystem;

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
        [Route("{id:Guid}/{translationId:Guid?}")]
        public HttpResponseMessage Get(Guid id, Guid? translationId = null)
        {
            var translationFile = this.translationsService.GetAsExcelFile(id, translationId);

            if (translationFile.ContentAsExcelFile == null) return this.Request.CreateResponse(HttpStatusCode.NotFound);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(translationFile.ContentAsExcelFile)
            };

            var translationName = string.IsNullOrEmpty(translationFile.TranslationName)
                ? "New translation"
                : translationFile.TranslationName;
            var filename = this.fileSystemAccessor.MakeValidFileName($"[{translationName}]{translationFile.QuestionnaireTitle}.xlsx");

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = filename
            };
            
            return response;
        }
    }
}
