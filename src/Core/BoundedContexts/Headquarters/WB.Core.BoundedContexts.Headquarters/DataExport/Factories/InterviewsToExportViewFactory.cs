using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Factories
{
    internal class InterviewsToExportViewFactory : IInterviewsToExportViewFactory
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewComment> comments;

        public InterviewsToExportViewFactory(IQueryableReadSideRepositoryReader<InterviewComment> comments)
        {
            this.comments = comments;
        }

        public List<Views.InterviewApiComment> GetInterviewComments(params Guid[] interviewIds)
        {
           var result = this.comments.Query(_ =>
                        _.Where(x => interviewIds.Contains(x.InterviewCommentaries.InterviewId))
                            .Select(i => new Views.InterviewApiComment
                                {
                                    InterviewId = i.InterviewCommentaries.SummaryId,
                                    CommentSequence = i.CommentSequence,
                                    OriginatorName = i.OriginatorName,
                                    OriginatorRole = i.OriginatorRole,
                                    Timestamp = i.Timestamp,
                                    Variable = i.Variable,
                                    Roster = i.Roster,
                                    RosterVector = i.RosterVector,
                                    Comment = i.Comment
                                }).OrderBy(i => i.Timestamp).ToList());
            return result;
        }
    }
}
