using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.ModelBinding;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Headquarters.API.Filters;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.API.Export
{
    public class InterviewsExportApiController : ApiController
    {
        private readonly IInterviewsToExportViewFactory viewFactory;
        private readonly IInterviewFactory interviewFactory;
        private readonly IInterviewDiagnosticsFactory interviewDiagnosticsFactory;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewStatuses;

        public InterviewsExportApiController(
            IInterviewsToExportViewFactory viewFactory,
            IInterviewFactory interviewFactory,
            IInterviewDiagnosticsFactory interviewDiagnosticsFactory,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewStatuses)
        {
            this.viewFactory = viewFactory ?? throw new ArgumentNullException(nameof(viewFactory));
            this.interviewFactory = interviewFactory ?? throw new ArgumentNullException(nameof(interviewFactory));
            this.interviewDiagnosticsFactory = interviewDiagnosticsFactory ?? throw new ArgumentNullException(nameof(interviewDiagnosticsFactory));
            this.interviewStatuses = interviewStatuses ?? throw new ArgumentNullException(nameof(interviewStatuses));
        }

        [Route("api/export/v1/interview")]
        [ServiceApiKeyAuthorization]
        [HttpGet]
        [ApiNoCache]
        public HttpResponseMessage Get([QueryString] string questionnaireIdentity, [FromUri] GetInterviewsArgs args)
        {
            var result = viewFactory.GetInterviewsToExport(QuestionnaireIdentity.Parse(questionnaireIdentity), args?.status, args?.fromDate, args?.toDate);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        public class GetInterviewsArgs
        {
            public InterviewStatus? status { get; set; }
            public DateTime? fromDate { get; set; }
            public DateTime? toDate { get; set; }
        }

        [Route("api/export/v1/interview/{id:guid}")]
        [ServiceApiKeyAuthorization]
        [HttpGet]
        [ApiNoCache]
        public HttpResponseMessage GetInterview(Guid id, [FromUri] Guid[] entityId = null)
        {
            List<InterviewEntity> entities = this.interviewFactory.GetInterviewEntities(id);

            return Request.CreateResponse(HttpStatusCode.OK, entities);
        }

        [Route("api/export/v1/interviews")]
        [ServiceApiKeyAuthorization]
        [HttpGet]
        [ApiNoCache]
        public HttpResponseMessage GetInterviews([FromUri]Guid[] id, [FromUri] Guid[] entityId = null)
        {
            var entities = this.interviewFactory.GetInterviewEntities(id, entityId).ToList();

            return Request.CreateResponse(HttpStatusCode.OK, entities);
        }

        [Route("api/export/v1/interview/batch/commentaries")]
        [ServiceApiKeyAuthorization]
        [HttpGet]
        [ApiNoCache]
        public HttpResponseMessage GetInterviewCommentariesBatch([FromUri] Guid[] id)
        {
            var response = new List<(Guid id, List<InterviewApiComment> comments)>();

            foreach (var interviewId in id)
            {
                var entities = this.viewFactory.GetInterviewComments(interviewId);
                response.Add((interviewId, entities));
            }

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [Route("api/export/v1/interview/batch/diagnosticsInfo")]
        [ServiceApiKeyAuthorization]
        [HttpGet]
        [ApiNoCache]
        public HttpResponseMessage GetInterviewDiagnosticsBatch([FromUri] Guid[] id)
        {
            var entities = this.interviewDiagnosticsFactory.GetByBatchIds(id);
            return Request.CreateResponse(HttpStatusCode.OK, entities);
        }

        [Route("api/export/v1/interview/batch/summaries")]
        [ServiceApiKeyAuthorization]
        [HttpGet]
        [ApiNoCache]
        public HttpResponseMessage GetInterviewSummariesBatch([FromUri] Guid[] id)
        {
            var interviews =
                this.interviewStatuses.Query(_ => _
                    .Where(i => id.Contains(i.InterviewId))
                    .SelectMany(interviewWithStatusHistory => interviewWithStatusHistory.InterviewCommentedStatuses,
                        (interview, status) => new { interview.InterviewId, interview.Key, StatusHistory = status })
                    .Select(i => new
                    {
                        i.InterviewId,
                        i.Key,
                        i.StatusHistory.Status,
                        i.StatusHistory.StatusChangeOriginatorName,
                        i.StatusHistory.StatusChangeOriginatorRole,
                        i.StatusHistory.Timestamp,
                        i.StatusHistory.SupervisorName,
                        i.StatusHistory.InterviewerName,
                        i.StatusHistory.Position
                    })
                    .OrderBy(x => x.InterviewId)
                    .ThenBy(x => x.Position).ToList());

            return Request.CreateResponse(HttpStatusCode.OK, interviews);
        }

    }
}
