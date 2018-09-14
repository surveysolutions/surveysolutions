using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Factories
{
    internal class InterviewsToExportViewFactory : IInterviewsToExportViewFactory
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries;
        private readonly IQueryableReadSideRepositoryReader<InterviewCommentaries> comments;

        public InterviewsToExportViewFactory(IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries,
            IQueryableReadSideRepositoryReader<InterviewCommentaries> comments)
        {
            this.interviewSummaries = interviewSummaries ?? throw new ArgumentNullException(nameof(interviewSummaries));
            this.comments = comments;
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

        public List<Views.InterviewApiComment> GetInterviewComments(Guid interviewId)
        {
               var result = this.comments.Query(_ =>
                            _.Where(x => x.InterviewId == interviewId.FormatGuid())
                                .SelectMany(
                                    interviewComments => interviewComments.Commentaries,
                                    (interview, comment) => new { interview.InterviewId, Comments = comment })
                                .Select(i => new Views.InterviewApiComment
                                    {
                                        InterviewId = i.InterviewId,
                                        CommentSequence = i.Comments.CommentSequence,
                                        OriginatorName = i.Comments.OriginatorName,
                                        OriginatorRole = i.Comments.OriginatorRole,
                                        Timestamp = i.Comments.Timestamp,
                                        Variable = i.Comments.Variable,
                                        Roster = i.Comments.Roster,
                                        RosterVector = i.Comments.RosterVector,
                                        Comment = i.Comments.Comment
                                    }).OrderBy(i => i.Timestamp).ToList());
            return result;
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
