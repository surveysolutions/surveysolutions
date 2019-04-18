using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.UI.Headquarters.API.Filters;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.API.Export
{
    public class InterviewsExportApiController : ApiController
    {
        private readonly IInterviewsToExportViewFactory viewFactory;
        private readonly IInterviewDiagnosticsFactory interviewDiagnosticsFactory;
        private readonly IInterviewHistoryFactory interviewHistoryFactory;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewStatuses;

        public InterviewsExportApiController(
            IInterviewsToExportViewFactory viewFactory,
            IInterviewDiagnosticsFactory interviewDiagnosticsFactory,
            IInterviewHistoryFactory interviewHistoryFactory,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewStatuses)
        {
            this.viewFactory = viewFactory ?? throw new ArgumentNullException(nameof(viewFactory));
            this.interviewDiagnosticsFactory = interviewDiagnosticsFactory ?? throw new ArgumentNullException(nameof(interviewDiagnosticsFactory));
            this.interviewHistoryFactory = interviewHistoryFactory ?? throw new ArgumentNullException(nameof(interviewHistoryFactory));
            this.interviewStatuses = interviewStatuses ?? throw new ArgumentNullException(nameof(interviewStatuses));
        }

        [Route("api/export/v1/interview/batch/commentaries")]
        [ServiceApiKeyAuthorization]
        [HttpGet]
        [ApiNoCache]
        public HttpResponseMessage GetInterviewCommentariesBatch([FromUri] Guid[] id)
        {
            var keys = this.interviewStatuses.Query(_ => _
                    .Where(x => id.Contains(x.InterviewId))
                    .Select(x => new { x.InterviewId, x.Key }).ToList())
                .ToDictionary(x => x.InterviewId.FormatGuid(), x => x.Key);

            var result = this.viewFactory.GetInterviewComments(id)
                .Select(c => new
                {
                    c.InterviewId, c.Variable,
                    c.Comment, c.CommentSequence,
                    c.OriginatorName, c.OriginatorRole,
                    c.Roster, RosterVector = new RosterVector(c.RosterVector).Array,
                    c.Timestamp, InterviewKey = keys.ContainsKey(c.InterviewId)? keys[c.InterviewId] : ""
                })
                .ToList();

            return Request.CreateResponse(HttpStatusCode.OK, result);
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

        [Route("api/export/v1/interview/batch/history")]
        [ServiceApiKeyAuthorization]
        [HttpGet]
        [ApiNoCache]
        public HttpResponseMessage GetInterviewHistory([FromUri] Guid[] id)
        {
            var items = this.interviewHistoryFactory.Load(id);

            return Request.CreateResponse(HttpStatusCode.OK, items.Select(i => new
            {
                i.InterviewId, i.Records
            }));
        }
    }
}
