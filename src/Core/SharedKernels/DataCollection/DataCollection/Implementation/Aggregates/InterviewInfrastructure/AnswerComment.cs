using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public class AnswerComment
    {
        public Guid? Id { get; }
        public Guid UserId { get; private set; }
        public UserRoles UserRole { get; private set; }
        public DateTimeOffset CommentTime { get; private set; }
        public string Comment { get; private set; }
        public Identity QuestionIdentity { get; private set; }
        public bool Resolved { get; set; }
        public bool CommentOnPreviousAnswer { get; set; } = false;

        public AnswerComment(Guid userId, UserRoles userRole, DateTimeOffset commentTime, string comment, Identity questionIdentity, Guid? id, bool resolved)
        {
            UserId = userId;
            UserRole = userRole;
            CommentTime = commentTime;
            Comment = comment;
            this.QuestionIdentity = questionIdentity;
            this.Id = id;
            this.Resolved = resolved;
        }

        #region equals

        public bool Equals(AnswerComment other)
        {
            if (this.Id.HasValue) return this.Id == other.Id;

            return this.UserId.Equals(other.UserId) && this.CommentTime.Equals(other.CommentTime) &&
                string.Equals(this.Comment, other.Comment) && this.QuestionIdentity.Equals(other.QuestionIdentity);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is AnswerComment && Equals((AnswerComment) obj);
        }

        public override int GetHashCode()
        {
            if (this.Id.HasValue) return this.Id.GetHashCode();

            unchecked
            {
                int hashCode = this.UserId.GetHashCode();
                hashCode = (hashCode*397) ^ this.CommentTime.GetHashCode();
                hashCode = (hashCode*397) ^ (this.Comment != null ? this.Comment.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (this.QuestionIdentity?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        #endregion
    }
}
