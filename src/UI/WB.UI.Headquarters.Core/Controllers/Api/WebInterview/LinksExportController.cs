using System.IO;
using Ionic.Zip;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Headquarters.Controllers.Api.WebInterview
{
    [Authorize(Roles = "Administrator, Headquarter")]
    [Route("api/{controller}/{action}/{id?}")]
    public class LinksExportController : ControllerBase
    {
        private readonly ISampleWebInterviewService sampleWebInterviewService;
        private readonly IFileSystemAccessor fileNameService;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseView;
        private readonly IVirtualPathService pathService;

        public LinksExportController(ISampleWebInterviewService sampleWebInterviewService,
            IFileSystemAccessor fileNameService,
            IQuestionnaireBrowseViewFactory questionnaireBrowseView,
            IVirtualPathService pathService)
        {
            this.sampleWebInterviewService = sampleWebInterviewService;
            this.fileNameService = fileNameService;
            this.questionnaireBrowseView = questionnaireBrowseView;
            this.pathService = pathService;
        }

        [HttpGet]
        public IActionResult Download(string id)
        {
            var questionnaireIdentity = QuestionnaireIdentity.Parse(id);
            
            byte[] uncompressedDataStream = this.sampleWebInterviewService.Generate(questionnaireIdentity,
                pathService.GetAbsolutePath($"~/WebInterview"));

            var compressedBytes = Compress(uncompressedDataStream);

            var fileContentResult = File(compressedBytes, "application/octet-stream");
            fileContentResult.FileDownloadName = this.GetOutputFileName(questionnaireIdentity);
            return fileContentResult;
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
            using MemoryStream memoryStream = new MemoryStream();
            using (ZipFile zip = new ZipFile())
            {
                zip.AddEntry("interviews.tab", uncompressedDataStream);
                zip.Save(memoryStream);
            }

            var compressedBytes = memoryStream.ToArray();
            return compressedBytes;
        }
    }
}
