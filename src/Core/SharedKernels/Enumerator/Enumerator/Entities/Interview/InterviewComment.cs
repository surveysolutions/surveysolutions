using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.Enumerator.Entities.Interview
{
    public class QuestionComment
    {
        public QuestionComment(string comment, Guid userId, UserRoles userRole, DateTime commentTime)
        {
            Comment = comment;
            UserId = userId;
            UserRole = userRole;
            CommentTime = commentTime;
        }

        public string Comment { get; set; }

        public Guid UserId { get; set; }

        public UserRoles UserRole { get; set; }

        public DateTime CommentTime { get; set; }
    }
}
