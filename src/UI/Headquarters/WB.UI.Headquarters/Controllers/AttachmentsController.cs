using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Web.Http;
using ImageResizer;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [Authorize]
    public class AttachmentsController : ApiController
    {
        private readonly IAttachmentContentService attachmentContentService;

        public AttachmentsController(IAttachmentContentService attachmentContentService)
        {
            this.attachmentContentService = attachmentContentService;
        }
        
        [HttpGet]
        public HttpResponseMessage Content(string id, int? maxSize = null)
        {
            if (this.Request.Headers.IfNoneMatch.Any(x => x.Tag.Trim('"') == id))
                return this.Request.CreateResponse(HttpStatusCode.NotModified);

            var attachmentContent = this.attachmentContentService.GetAttachmentContent(id);

            if(attachmentContent == null)
                return this.Request.CreateResponse(HttpStatusCode.NotFound);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(GetTrasformedContent(attachmentContent.Content, maxSize))
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue(attachmentContent.ContentType);
            response.Headers.ETag = new EntityTagHeaderValue("\"" + attachmentContent.ContentHash + "\"");
            response.Headers.CacheControl = new CacheControlHeaderValue()
            {
                MaxAge = TimeSpan.MaxValue,
                Public = true
            };

            return response;
        }

        private static byte[] GetTrasformedContent(byte[] source, int? sizeToScale = null)
        {
            if (!sizeToScale.HasValue) return source;

            //later should handle video and produce image preview 
            using (var outputStream = new MemoryStream())
            {
                ImageBuilder.Current.Build(source, outputStream, new ResizeSettings
                {
                    MaxWidth = sizeToScale.Value,
                    MaxHeight = sizeToScale.Value
                });

                return outputStream.ToArray();
            }
        }
    }
}