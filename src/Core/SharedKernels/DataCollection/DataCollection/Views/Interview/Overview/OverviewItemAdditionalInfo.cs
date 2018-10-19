using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;

namespace WB.Core.SharedKernels.DataCollection.Views.Interview.Overview
{
    public class OverviewItemAdditionalInfo
    {
        public OverviewItemAdditionalInfo(InterviewTreeQuestion treeQuestion, IStatefulInterview interview, Guid currentUserId)
        {
            this.Errors = interview.GetFailedValidationMessages(treeQuestion.Identity, null).ToArray();
            this.Warnings = interview.GetFailedWarningMessages(treeQuestion.Identity, string.Empty).ToArray();
            this.Comments = GetComments(treeQuestion.AnswerComments, currentUserId).ToList();
        }

        public OverviewItemAdditionalInfo(InterviewTreeStaticText treeText, IStatefulInterview interview)
        {
            this.Errors = interview.GetFailedValidationMessages(treeText.Identity, null).ToArray();
            this.Warnings = interview.GetFailedWarningMessages(treeText.Identity, string.Empty).ToArray();
        }

        private static IEnumerable<OverviewAnswerComment> GetComments(List<AnswerComment> answerComments, Guid currentUserId)
        {
            return answerComments.Select(x => new OverviewAnswerComment
            {
                Text = x.Comment,
                IsOwnComment = x.UserId == currentUserId,
                UserRole = x.UserRole,
                CommentTimeUtc = x.CommentTime,
                EntityId = x.QuestionIdentity.ToString()
            });
        }

        public List<OverviewAnswerComment> Comments { get; set; } = new List<OverviewAnswerComment>();

        public string[] Warnings { get; set; }

        public string[] Errors { get; set; }
    }
}
