using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;

namespace WB.Core.SharedKernels.DataCollection.Views.Interview.Overview
{
    public class OverviewQuestion : OverviewNode
    {
        public OverviewQuestion(InterviewTreeQuestion treeQuestion, IStatefulInterview interview) : base(treeQuestion)
        {
            this.Answer = treeQuestion.GetAnswerAsString(CultureInfo.CurrentCulture);
            
            if (!treeQuestion.IsAnswered())
            {
                this.State = OverviewNodeState.Unanswered;
            }
            else if (!treeQuestion.IsValid)
            {
                this.State = OverviewNodeState.Invalid;
            }
            else
            {
                this.State = OverviewNodeState.Answered;
            }

            base.IsAnswered = treeQuestion.IsAnswered();
            this.HasErrors = interview.GetFailedValidationMessages(treeQuestion.Identity, null).Any();

            this.HasWarnings = interview.GetFailedWarningMessages(treeQuestion.Identity, string.Empty).Any();
            HasComment = treeQuestion.AnswerComments.Count > 0;
            this.AnswerTimeUtc = treeQuestion.AnswerTimeUtc;
            this.SupportsComments = true;
        }

        public bool HasErrors { get; set; }

        public string Answer { get; set; }

        public DateTime? AnswerTimeUtc { get; set; }

        public bool HasComment { get; set; }

        public bool HasWarnings { get; set; }

        public sealed override OverviewNodeState State { get; set; }
    }

    public class OverviewItemAdditionalInfo
    {
        public OverviewItemAdditionalInfo(InterviewTreeQuestion treeQuestion, IStatefulInterview interview, Guid currentUserId)
        {
            this.Errors = interview.GetFailedValidationMessages(treeQuestion.Identity, null).ToArray();
            this.Warnings = interview.GetFailedWarningMessages(treeQuestion.Identity, string.Empty).ToArray();
            this.Comments = GetComments(treeQuestion.AnswerComments, currentUserId).ToList();
        }

        public OverviewItemAdditionalInfo(InterviewTreeStaticText treeText, IStatefulInterview interview, Guid currentUserId)
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

    public class OverviewAnswerComment 
    {
        public string Text { get; set; }
        public bool IsOwnComment { get; set; }
        public UserRoles UserRole { get; set; }
        public DateTime CommentTimeUtc { get; set; }
        public string EntityId { get; set; }
    }
}
