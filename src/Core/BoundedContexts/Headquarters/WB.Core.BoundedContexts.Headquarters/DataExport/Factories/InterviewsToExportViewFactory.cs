using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Factories
{
    internal class InterviewsToExportViewFactory : IInterviewsToExportViewFactory
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewCommentaries> comments;

        public InterviewsToExportViewFactory(IQueryableReadSideRepositoryReader<InterviewCommentaries> comments)
        {
            this.comments = comments;
        }

        public List<Views.InterviewApiComment> GetInterviewComments(params Guid[] interviewIds)
        {
            var stringids = interviewIds.Select(x => x.FormatGuid()).ToArray();

               var result = this.comments.Query(_ =>
                            _.Where(x => stringids.Contains(x.InterviewId))
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
    }
}
