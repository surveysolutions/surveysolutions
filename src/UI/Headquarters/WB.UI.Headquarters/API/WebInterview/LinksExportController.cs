using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Ionic.Zip;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.Headquarters.API.WebInterview
{
    [Authorize(Roles = "Administrator, Headquarter")]
    public class LinksExportController : ApiController
    {
        private readonly ISampleWebInterviewService sampleWebInterviewService;
        private readonly IFileSystemAccessor fileNameService;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseView;

        public LinksExportController(ISampleWebInterviewService sampleWebInterviewService,
            IFileSystemAccessor fileNameService,
            IQuestionnaireBrowseViewFactory questionnaireBrowseView)
        {
            this.sampleWebInterviewService = sampleWebInterviewService;
            this.fileNameService = fileNameService;
            this.questionnaireBrowseView = questionnaireBrowseView;
        }

        [HttpGet]
        public HttpResponseMessage Download(string id)
        {
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            var questionnaireIdentity = QuestionnaireIdentity.Parse(id);

            byte[] uncompressedDataStream = this.sampleWebInterviewService.Generate(questionnaireIdentity, this.Url.Content("~/WebInterview"));

            var compressedBytes = Compress(uncompressedDataStream);

            response.Content = new ByteArrayContent(compressedBytes);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            response.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true
            };
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = this.GetOutputFileName(questionnaireIdentity)
            };

            return response;
        }

        private string GetOutputFileName(QuestionnaireIdentity questionnaireIdentity)
        {
            var questionnaireBrowseItem = this.questionnaireBrowseView.GetById(questionnaireIdentity);

            var fileName = this.fileNameService.MakeValidFileName(
                $"Web {questionnaireBrowseItem.Title} (ver. {questionnaireBrowseItem.Version}).zip");
            return fileName;
        }

        private static byte[] Compress(byte[] uncompressedDataStream)
        {
            byte[] compressedBytes;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (ZipFile zip = new ZipFile())
                {
                    zip.AddEntry("interviews.tab", uncompressedDataStream);
                    zip.Save(memoryStream);
                }

                compressedBytes = memoryStream.ToArray();
            }
            return compressedBytes;
        }
    }
}