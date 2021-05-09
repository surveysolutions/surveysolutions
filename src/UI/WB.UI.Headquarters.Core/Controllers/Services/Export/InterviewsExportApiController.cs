using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.ServicesIntegration.Export;
using WB.UI.Headquarters.Code.Authentication;
using InterviewDiagnosticsInfo = WB.Core.BoundedContexts.Headquarters.Factories.InterviewDiagnosticsInfo;
using InterviewHistoricalRecordView = WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory.InterviewHistoricalRecordView;
using UserRoles = Main.Core.Entities.SubEntities.UserRoles;

namespace WB.UI.Headquarters.Controllers.Services.Export
{
    public class InterviewCommentariesDto
    {
        public string InterviewId { get; set; }
        public string Variable { get; set; }
        public string Comment { get; set; }
        public int CommentSequence { get; set; }
        public string OriginatorName { get; set; }
        public UserRoles OriginatorRole { get; set; }
        public string Roster { get; set; }
        public int[] RosterVector { get; set; }
        public DateTime Timestamp { get; set; }
        public string InterviewKey { get; set; }
    }

    public class InterviewSummariesDto
    {
        public Guid InterviewId { get; set; }
        public string Key { get; set; }
        public InterviewExportedAction Status { get; set; }
        public string StatusChangeOriginatorName { get; set; }
        public UserRoles StatusChangeOriginatorRole { get; set; }
        public DateTime Timestamp { get; set; }
        public string SupervisorName { get; set; }
        public string InterviewerName { get; set; }
        public int Position { get; set; }
    }

    public class InterviewHistoryDto
    {
        public Guid InterviewId { get; set; }
        public List<InterviewHistoricalRecordView> Records { get; set; }
    }

    [Authorize(AuthenticationSchemes = AuthType.TenantToken)]
    public class InterviewsExportApiController : Controller
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
        [HttpGet]
        [ApiNoCache]
        public ActionResult<List<InterviewCommentariesDto>> GetInterviewCommentariesBatch([FromQuery(Name = "id")] Guid[] id)
        {
            var keys = this.interviewStatuses.Query(_ => _
                    .Where(x => id.Contains(x.InterviewId))
                    .Select(x => new { x.InterviewId, x.Key }).ToList())
                .ToDictionary(x => x.InterviewId.FormatGuid(), x => x.Key);

            var result = this.viewFactory.GetInterviewComments(id)
                .Select(c => new InterviewCommentariesDto
                {
                    InterviewId = c.InterviewId, 
                    Variable = c.Variable, 
                    Comment = c.Comment,
                    CommentSequence = c.CommentSequence,
                    OriginatorName = c.OriginatorName,
                    OriginatorRole = c.OriginatorRole,
                    Roster = c.Roster,
                    RosterVector = new RosterVector(c.RosterVector).Array,
                    Timestamp = c.Timestamp,
                    InterviewKey = keys.ContainsKey(c.InterviewId)? keys[c.InterviewId] : ""
                })
                .ToList();

            return result;
        }

        [Route("api/export/v1/interview/batch/diagnosticsInfo")]
        [HttpGet]
        [ApiNoCache]
        public ActionResult<List<InterviewDiagnosticsInfo>> GetInterviewDiagnosticsBatch([FromQuery(Name = "id")] Guid[] id)
        {
            var entities = this.interviewDiagnosticsFactory.GetByBatchIds(id);
            return entities;
        }

        [Route("api/export/v1/interview/batch/summaries")]
        [HttpGet]
        [ApiNoCache]
        public ActionResult<List<InterviewSummariesDto>> GetInterviewSummariesBatch([FromQuery(Name = "id")] Guid[] id)
        {
            var interviews =
                this.interviewStatuses.Query(_ => _
                    .Where(i => id.Contains(i.InterviewId))
                    .SelectMany(interviewWithStatusHistory => interviewWithStatusHistory.InterviewCommentedStatuses,
                        (interview, status) => new { interview.InterviewId, interview.Key, StatusHistory = status })
                    .Select(i => new InterviewSummariesDto
                    {
                        InterviewId = i.InterviewId, Key = i.Key, Status = i.StatusHistory.Status,
                        StatusChangeOriginatorName = i.StatusHistory.StatusChangeOriginatorName,
                        StatusChangeOriginatorRole = i.StatusHistory.StatusChangeOriginatorRole,
                        Timestamp = i.StatusHistory.Timestamp,
                        SupervisorName = i.StatusHistory.SupervisorName,
                        InterviewerName = i.StatusHistory.InterviewerName,
                        Position = i.StatusHistory.Position
                    })
                    .OrderBy(x => x.InterviewId)
                    .ThenBy(x => x.Position).ToList());

            return interviews;
        }

        [Route("api/export/v1/interview/batch/history")]
        [HttpGet]
        [ApiNoCache]
        public ActionResult<List<InterviewHistoryDto>> GetInterviewHistory([FromQuery(Name = "id")] Guid[] id)
        {
            var items = this.interviewHistoryFactory.Load(id);
            return items.Select(i => new InterviewHistoryDto {InterviewId = i.InterviewId, Records = i.Records}).ToList();
        }
    }
}
