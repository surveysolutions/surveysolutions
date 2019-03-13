using System;
using System.Collections.Generic;
using System.Linq;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Services
{
    public interface IInterviewsToExportSource
    {
        List<InterviewToExport> GetInterviewsToExport(QuestionnaireId questionnaireIdentity,
            InterviewStatus? status, DateTime? fromDate, DateTime? toDate);
    }

    public class InterviewsToExportSource : IInterviewsToExportSource
    {
        private readonly TenantDbContext dbContext;

        public InterviewsToExportSource(TenantDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public List<InterviewToExport> GetInterviewsToExport(QuestionnaireId questionnaireIdentity,
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
            var result = queryable.Select(x => new InterviewToExport(x.InterviewId, x.Key, x.Status)).ToList();
            return result;
        }
    }
}
