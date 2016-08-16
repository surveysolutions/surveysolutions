using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.Enumerator.Entities.Interview
{
    public class QuestionComment
    {
        public QuestionComment(string comment, Guid userId, UserRoles userRole)
        {
            this.Comment = comment;
            UserId = userId;
            this.UserRole = userRole;
        }

        public string Comment { get; set; }

        public Guid UserId { get; set; }

        public UserRoles UserRole { get; set; }
    }
}
