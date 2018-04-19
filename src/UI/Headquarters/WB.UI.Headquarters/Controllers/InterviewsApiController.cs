using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator, Headquarter")]
    [ApiNoCache]
    public class InterviewsApiController : BaseApiController
    {
        private readonly IArchiveUtils archiver;
        private readonly IAssignmentsImportService assignmentsImportService;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;

        public InterviewsApiController(
            ICommandService commandService, 
            ILogger logger,
            IArchiveUtils archiver,
            IAssignmentsImportService assignmentsImportService,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory)
            : base(commandService, logger)
        {
            this.archiver = archiver;
            this.assignmentsImportService = assignmentsImportService;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
        }

        [HttpGet]
        [CamelCase]
        public InterviewImportStatusApiView GetImportInterviewsStatus()
        {
            var status = this.assignmentsImportService.GetImportStatus();
            if (status == null) return null;

            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(status.QuestionnaireIdentity);

            return new InterviewImportStatusApiView
            {
                QuestionnaireId = status.QuestionnaireIdentity.QuestionnaireId,
                QuestionnaireVersion = status.QuestionnaireIdentity.Version,
                QuestionnaireTitle = questionnaireInfo?.Title,
                TotalInterviewsCount = status.TotalAssignments,
                CreatedInterviewsCount = status.ProcessedCount,
                VerifiedInterviewsCount = status.VerifiedAssignments,
                InterviewsWithError = status.AssingmentsWithErrors
            };
        }

        [HttpGet]
        [ObserverNotAllowedApi]
        public HttpResponseMessage GetInvalidInterviewsByLastImport()
        {
            var sb = new StringBuilder();
            
            foreach (var interviewImportError in this.assignmentsImportService.GetImportAssignmentsErrors())
            {
                sb.AppendLine(interviewImportError);
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
            public Guid QuestionnaireId { get; set; }
            public long QuestionnaireVersion { get; set; }
            public string QuestionnaireTitle { get; set; }
            public long TotalInterviewsCount { get; set; }
            public long CreatedInterviewsCount { get; set; }
            public long VerifiedInterviewsCount { get; set; }
            public long InterviewsWithError { get; set; }
        }
    }
}
