using System;
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
            var questionnaireIdentity = summaries.GetById(interviewId)?.QuestionnaireIdentity;
            if (questionnaireIdentity != null)
                return QuestionnaireIdentity.Parse(questionnaireIdentity);

            return null;
        }

        public static QuestionnaireIdentity GetQuestionnaireIdentity(
            this IQueryableReadSideRepositoryReader<InterviewSummary> summaries, Guid interviewId)
        {
            return summaries.GetQuestionnaireIdentity(interviewId.FormatGuid());
        }
    }
}
