using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Factories
{
    internal class InterviewsToExportViewFactory : IInterviewsToExportViewFactory
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries;

        public InterviewsToExportViewFactory(IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries)
        {
            this.interviewSummaries = interviewSummaries ?? throw new ArgumentNullException(nameof(interviewSummaries));
        }

        public List<InterviewToExport> GetInterviewsToExport(QuestionnaireIdentity questionnaireIdentity,
            InterviewStatus? status, DateTime? fromDate, DateTime? toDate)
        {
            if (questionnaireIdentity == null) throw new ArgumentNullException(nameof(questionnaireIdentity));

            List<InterviewToExport> batchInterviews = 
                this.interviewSummaries.Query(_ => this.Filter(_, questionnaireIdentity, status, fromDate, toDate)
                        .OrderBy(x => x.InterviewId)
                        .Select(x => new InterviewToExport(x.InterviewId, x.Key, x.ErrorsCount, x.Status))
                        .ToList());

            return batchInterviews;
        }

        private IQueryable<InterviewSummary> Filter(IQueryable<InterviewSummary> queryable,
            QuestionnaireIdentity questionnaireIdentity, InterviewStatus? status, DateTime? fromDate, DateTime? toDate)
        {
            var stringQuestionnaireId = questionnaireIdentity.ToString();

            queryable = queryable.Where(x => x.QuestionnaireIdentity == stringQuestionnaireId);

            if (status.HasValue)
            {
                var filteredByStatus = status.Value;
                queryable = queryable.Where(x => x.Status == filteredByStatus);
            }

            if (fromDate.HasValue)
            {
                var filteredFromDate = fromDate.Value;
                queryable = queryable.Where(x => x.UpdateDate >= filteredFromDate);
            }

            if(toDate.HasValue)
            {
                var filteredToDate = toDate.Value;
                queryable = queryable.Where(x => x.UpdateDate < filteredToDate);
            }

            return queryable;
        }
    }
}
