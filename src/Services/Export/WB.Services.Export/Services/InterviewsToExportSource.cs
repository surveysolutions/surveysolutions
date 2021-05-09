using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.ServicesIntegration.Export;
using ILogger = Amazon.Runtime.Internal.Util.ILogger;

namespace WB.Services.Export.Services
{
    public interface IInterviewsToExportSource
    {
        List<InterviewToExport> GetInterviewsToExport(QuestionnaireIdentity questionnaireIdentity,
            InterviewStatus? status, DateTime? fromDate, DateTime? toDate);
    }

    public class InterviewsToExportSource : IInterviewsToExportSource
    {
        private readonly TenantDbContext dbContext;
        private readonly ILogger<InterviewsToExportSource> logger;

        public InterviewsToExportSource(TenantDbContext dbContext, ILogger<InterviewsToExportSource> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public List<InterviewToExport> GetInterviewsToExport(QuestionnaireIdentity questionnaireIdentity,
            InterviewStatus? status, DateTime? fromDate, DateTime? toDate)
        {
            if (questionnaireIdentity == null) throw new ArgumentNullException(nameof(questionnaireIdentity));

            var questionnaireIdString = questionnaireIdentity.ToString();
            var queryable = this.dbContext.InterviewReferences
                    .Where(x => x.QuestionnaireId == questionnaireIdString && x.DeletedAtUtc == null);

            if (status.HasValue)
            {
                var filteredByStatus = status.Value;
                queryable = queryable.Where(x => x.Status == filteredByStatus);
            }

            if (fromDate.HasValue)
            {
                var filteredFromDate = fromDate.Value;
                queryable = queryable.Where(x => x.UpdateDateUtc >= filteredFromDate);
            }

            if(toDate.HasValue)
            {
                var filteredToDate = toDate.Value;
                queryable = queryable.Where(x => x.UpdateDateUtc < filteredToDate);
            }
            var result = queryable.Select(x => new InterviewToExport(x.InterviewId, x.Key, x.Status, x.AssignmentId)).ToList();

            this.logger.LogTrace("Get Interviews to Export: {questionnaire} Status: {status}. {from}-{to}. Got {count} interviews",
                questionnaireIdentity.ToString(), status, fromDate, toDate, result.Count);

            return result;
        }
    }
}
