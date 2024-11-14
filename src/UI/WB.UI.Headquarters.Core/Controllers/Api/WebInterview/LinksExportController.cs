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
        private readonly IArchiveUtils archiveUtils;

        public LinksExportController(ISampleWebInterviewService sampleWebInterviewService,
            IFileSystemAccessor fileNameService,
            IQuestionnaireBrowseViewFactory questionnaireBrowseView,
            IVirtualPathService pathService,
            IArchiveUtils archiveUtils)
        {
            this.sampleWebInterviewService = sampleWebInterviewService;
            this.fileNameService = fileNameService;
            this.questionnaireBrowseView = questionnaireBrowseView;
            this.pathService = pathService;
            this.archiveUtils = archiveUtils;
        }

        [HttpGet]
        public IActionResult Download(string id)
        {
            var questionnaireIdentity = QuestionnaireIdentity.Parse(id);
            
            byte[] uncompressedData = this.sampleWebInterviewService.Generate(questionnaireIdentity,
                pathService.GetAbsolutePath($"~/WebInterview"));

            var compressedBytes = archiveUtils.CompressContentToEntity(uncompressedData, "interviews.tab");

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
    }
}
