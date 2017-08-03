using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Services;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator, Headquarter")]
    [ApiNoCache]
    public class InterviewsApiController : BaseApiController
    {
        private readonly IInterviewImportService interviewImportService;
        private readonly IArchiveUtils archiver;

        public InterviewsApiController(
            ICommandService commandService, 
            ILogger logger, 
            IInterviewImportService interviewImportService,
            IArchiveUtils archiver)
            : base(commandService, logger)
        {
            this.interviewImportService = interviewImportService;
            this.archiver = archiver;
        }

        [HttpGet]
        [CamelCase]
        public InterviewImportStatusApiView GetImportInterviewsStatus()
        {
            var status = this.interviewImportService.Status;
            return new InterviewImportStatusApiView
            {
                Stage = status.Stage.ToString(),
                QuestionnaireId = status.QuestionnaireId.QuestionnaireId,
                QuestionnaireVersion = status.QuestionnaireId.Version,
                QuestionnaireTitle = status.QuestionnaireTitle,
                IsInProgress = status.IsInProgress,
                TotalInterviewsCount = status.TotalCount,
                CreatedInterviewsCount = status.ProcessedCount,
                EstimatedTime = TimeSpan.FromMilliseconds(status.EstimatedTime).ToString(@"dd\.hh\:mm\:ss"),
                ElapsedTime = TimeSpan.FromMilliseconds(status.ElapsedTime).ToString(@"dd\.hh\:mm\:ss"),
                HasErrors = status.State.Errors.Any() || status.VerificationState.Errors.Any(),
                InterviewsWithError = status.State.Errors.Count + status.VerificationState.Errors.Count,
                InterviewImportProcessId = status.InterviewImportProcessId
            };
        }
        
        [HttpGet]
        [ObserverNotAllowedApi]
        public HttpResponseMessage GetInvalidInterviewsByLastImport()
        {
            var interviewImportState = this.interviewImportService.Status.State;

            var sb = new StringBuilder();
            
            foreach (var interviewImportError in interviewImportState.Errors)
            {
                sb.AppendLine(interviewImportError.ErrorMessage);
            }

            var invalidInterviewsFileName = "invalid-interviews";

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(new MemoryStream(this.archiver.CompressStringToByteArray($"{invalidInterviewsFileName}.tab", sb.ToString())))
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = $"{invalidInterviewsFileName}.zip"
            };

            return response;
        }

        public class InterviewImportStatusApiView
        {
            public string InterviewImportProcessId { get; set; }
            public Guid QuestionnaireId { get; set; }
            public long QuestionnaireVersion { get; set; }
            public string QuestionnaireTitle { get; set; }
            public bool IsInProgress { get; set; }
            public int TotalInterviewsCount { get; set; }
            public int CreatedInterviewsCount { get; set; }
            public int InterviewsWithError { get; set; }
            public string ElapsedTime { get; set; }
            public string EstimatedTime { get; set; }
            public bool HasErrors { get; set; }
            public string Stage { get; set; }
        }
    }
}