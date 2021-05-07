using System;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Accessors
{
    public static class InterviewReaderExtensions
    {
        public static QuestionnaireIdentity GetQuestionnaireIdentity(
            this IQueryableReadSideRepositoryReader<InterviewSummary> summaries,
            Guid interviewId)
        {
            var questionnaireIdentity = summaries
                .Query(_ => _.Where(s => s.InterviewId == interviewId)
                    .Select(x => x.QuestionnaireIdentity).FirstOrDefault());

            if (questionnaireIdentity != null)
                return QuestionnaireIdentity.Parse(questionnaireIdentity);

            return null;
        }

        public static QuestionnaireIdentity GetQuestionnaireIdentity(this IMemoryCache cache,
            IQueryableReadSideRepositoryReader<InterviewSummary> summaries, Guid interviewId)
        {
            return cache.GetOrCreate("gqi:" + interviewId, entry =>
            {
                entry.SetSlidingExpiration(TimeSpan.FromMinutes(5));
                return summaries.GetQuestionnaireIdentity(interviewId);
            });
        }

        public static void SetQuestionnaireIdentity(this IMemoryCache cache,
            Guid interviewId, QuestionnaireIdentity questionnaireIdentity)
        {
            cache.Set("gqi:" + interviewId, questionnaireIdentity);
        }
    }
}
