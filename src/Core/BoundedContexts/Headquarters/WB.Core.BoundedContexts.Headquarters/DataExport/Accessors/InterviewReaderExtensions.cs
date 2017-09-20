using System;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Accessors
{
    public static class InterviewReaderExtensions
    {
        public static QuestionnaireIdentity GetQuestionnaireIdentity(
            this IQueryableReadSideRepositoryReader<InterviewSummary> summaries, string interviewId)
        {
            var summaryData = summaries.Query(_ => _.Where(x => x.SummaryId == interviewId).Select(x => x.QuestionnaireIdentity).FirstOrDefault());
            if (summaryData != null)
            {
                return QuestionnaireIdentity.Parse(summaryData);
            }

            return null;
        }

        public static QuestionnaireIdentity GetQuestionnaireIdentity(
            this IQueryableReadSideRepositoryReader<InterviewSummary> summaries, Guid interviewId)
        {
            return summaries.GetQuestionnaireIdentity(interviewId.FormatGuid());
        }
    }
}