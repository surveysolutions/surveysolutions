using System;
using System.IO;
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
            var attachment = this.attachmentContentService.GetAttachmentContent(id);

            if (attachment == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            if (maxSize.HasValue)
            {
                return ResizeAndCreateResponse(attachment, maxSize.Value);
            }
            
            return CreateResponse(attachment);
        }

        private HttpResponseMessage CreateResponse(AttachmentContent attachment)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(attachment.Content)
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue(attachment.ContentType);
            response.Headers.ETag = new EntityTagHeaderValue("\"" + attachment.ContentHash + "\"");

            return response;
        }

        private HttpResponseMessage ResizeAndCreateResponse(AttachmentContent attachmentContent, int sizeToScale)
        {
            var resizeSettings = new ResizeSettings
            {
                MaxWidth = sizeToScale,
                MaxHeight = sizeToScale
            };

            byte[] transformedContent;
            using (var outputStream = new MemoryStream())
            {
                ImageBuilder.Current.Build(attachmentContent.Content, outputStream, resizeSettings);
                transformedContent = outputStream.ToArray();
            }
            
            string transformedContentHash;
            using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
            {
                transformedContentHash = BitConverter.ToString(sha1.ComputeHash(transformedContent)).Replace("-", string.Empty);
            }


            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(transformedContent)
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(attachmentContent.ContentType);
            response.Headers.ETag = new EntityTagHeaderValue("\"" + transformedContentHash + "\"");

            response.Headers.CacheControl = new CacheControlHeaderValue
            {
                Public = true,
                MaxAge = TimeSpan.FromDays(10)
            };

            return response;
        }
    }
}