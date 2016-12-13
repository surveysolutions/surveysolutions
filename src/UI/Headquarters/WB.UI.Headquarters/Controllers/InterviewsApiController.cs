using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.PreloadedData;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Infrastructure.Native.Threading;
using WB.UI.Headquarters.Services;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator, Headquarter")]
    public class InterviewsApiController : BaseApiController
    {
        private readonly IPreloadedDataRepository preloadedDataRepository;
        private readonly IInterviewImportService interviewImportService;
        private readonly IArchiveUtils archiver;
        private readonly IGlobalInfoProvider globalInfoProvider;

        public InterviewsApiController(
            ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger,
            IPreloadedDataRepository preloadedDataRepository, 
            IInterviewImportService interviewImportService,
            IArchiveUtils archiver,
            IGlobalInfoProvider globalInfoProvider)
            : base(commandService, globalInfo, logger)
        {
            this.preloadedDataRepository = preloadedDataRepository;
            this.interviewImportService = interviewImportService;
            this.archiver = archiver;
            this.globalInfoProvider = globalInfoProvider;
        }

        
        [HttpGet]
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

            var isSupervisorRequired = !request.WasResponsibleProvided && !request.SupervisorId.HasValue;

            var headquartersId = this.globalInfoProvider.GetCurrentUser().Id;

            if (!isSupervisorRequired)
            {
                if (this.interviewImportService.Status.IsInProgress)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable,
                           "Import interviews is in progress. Wait until current operation is finished.");
                }

                ThreadMarkerManager.MarkCurrentThreadAsIsolated();

                try
                {
                    this.interviewImportService.ImportInterviews(supervisorId: request.SupervisorId,
                        questionnaireIdentity: questionnaireIdentity, interviewImportProcessId: request.InterviewImportProcessId, 
                        isPanel: request.PreloadingType == PreloadedContentType.Panel,
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

        public class ImportInterviewsResponseApiView
        {
            public bool IsSupervisorRequired { get; set; }
        }

        public class ImportInterviewsRequestApiView
        {
            public Guid QuestionnaireId { get; set; }
            public int QuestionnaireVersion { get; set; }

            public string InterviewImportProcessId { get; set; }
            public Guid? SupervisorId { get; set; }

            public PreloadedContentType PreloadingType { get; set; }

            public bool WasResponsibleProvided { get; set; }
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
            public string ElapsedTime { get; set; }
            public string EstimatedTime { get; set; }
            public bool HasErrors { get; set; }
        }
    }
}