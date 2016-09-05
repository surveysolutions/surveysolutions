using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.UI.Headquarters.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.Api;
using WB.Infrastructure.Native.Threading;
using WB.UI.Headquarters.Services;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator, Headquarter")]
    public class InterviewsApiController : BaseApiController
    {
        private readonly IInterviewImportService interviewImportService;
        private readonly IArchiveUtils archiver;
        private readonly IIdentityManager identityManager;

        public InterviewsApiController(
            ICommandService commandService, 
            IIdentityManager identityManager, 
            ILogger logger, 
            IInterviewImportService interviewImportService,
            IArchiveUtils archiver)
            : base(commandService, logger)
        {
            this.interviewImportService = interviewImportService;
            this.archiver = archiver;
            this.identityManager = identityManager;
        }

        [ObserverNotAllowedApi]
        [ApiValidationAntiForgeryToken]
        public void ImportPanelData(BatchUploadModel model)
        {
            PreloadedDataByFile[] preloadedData = this.preloadedDataRepository.GetPreloadedDataOfPanel(model.InterviewId);
            Guid responsibleHeadquarterId = this.identityManager.CurrentUserId;

            new Task(() =>
            {
                ThreadMarkerManager.MarkCurrentThreadAsIsolated();
                ThreadMarkerManager.MarkCurrentThreadAsNoTransactional();
                try
                {
                    var sampleImportService = this.sampleImportServiceFactory.Invoke();

                    sampleImportService.CreatePanel(
                        model.QuestionnaireId,
                        model.QuestionnaireVersion,
                        model.InterviewId,
                        preloadedData,
                        responsibleHeadquarterId,
                        model.ResponsibleSupervisor);
                }
                finally
                {
                    ThreadMarkerManager.ReleaseCurrentThreadFromIsolation();
                    ThreadMarkerManager.RemoveCurrentThreadFromNoTransactional();
                }
            }).Start();
        }

        [HttpGet]
        [CamelCase]
        public InterviewImportStatusApiView GetImportInterviewsStatus()
        {
            var status = this.interviewImportService.Status;
            return new InterviewImportStatusApiView
            {
                QuestionnaireId = status.QuestionnaireId,
                QuestionnaireVersion = status.QuestionnaireVersion,
                QuestionnaireTitle = status.QuestionnaireTitle,
                IsInProgress = status.IsInProgress,
                TotalInterviewsCount = status.TotalInterviewsCount,
                CreatedInterviewsCount = status.CreatedInterviewsCount,
                EstimatedTime = TimeSpan.FromMilliseconds(status.EstimatedTime).ToString(@"dd\.hh\:mm\:ss"),
                ElapsedTime = TimeSpan.FromMilliseconds(status.ElapsedTime).ToString(@"dd\.hh\:mm\:ss"),
                HasErrors = status.State.Errors.Any(),
                InterviewsWithError = status.State.Errors.Count,
                InterviewImportProcessId = status.InterviewImportProcessId
            };
        }

        [HttpPost]
        [ObserverNotAllowedApi]
        [ApiValidationAntiForgeryToken]
        public HttpResponseMessage ImportInterviews(ImportInterviewsRequestApiView request)
        {
            if (this.interviewImportService.Status.IsInProgress)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable,
                       "Import interviews is in progress. Wait until current operation is finished.");
            }

            var questionnaireIdentity = new QuestionnaireIdentity(request.QuestionnaireId, request.QuestionnaireVersion);

            var isSupervisorRequired = !this.interviewImportService.HasResponsibleColumn(request.InterviewImportProcessId) &&
                                       !request.SupervisorId.HasValue;

            var headquartersId = this.identityManager.CurrentUserId;

            if (!isSupervisorRequired)
            {
                ThreadMarkerManager.MarkCurrentThreadAsIsolated();

                try
                {
                    this.interviewImportService.ImportInterviews(supervisorId: request.SupervisorId,
                        questionnaireIdentity: questionnaireIdentity, interviewImportProcessId: request.InterviewImportProcessId,
                        headquartersId: headquartersId);
                }
                finally
                {
                    ThreadMarkerManager.ReleaseCurrentThreadFromIsolation();
                }
            }

            return Request.CreateResponse(new ImportInterviewsResponseApiView
            {
                IsSupervisorRequired = isSupervisorRequired
            });
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
        }
    }
}