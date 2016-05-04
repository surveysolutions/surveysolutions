using System;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public struct AnswerComment
    {
        public Guid UserId { get; private set; }
        public DateTime CommentTime { get; private set; }
        public string Comment { get; private set; }
        public string QuestionKey { get; private set; }

        public AnswerComment(Guid userId, DateTime commentTime, string comment, Guid questionId, decimal[] rosterVector)
            : this()
        {
            UserId = userId;
            CommentTime = commentTime;
            Comment = comment;
            this.QuestionKey = ConversionHelper.ConvertIdAndRosterVectorToString(questionId, rosterVector);
        }

        #region equals

        public bool Equals(AnswerComment other)
        {
            return this.UserId.Equals(other.UserId) && this.CommentTime.Equals(other.CommentTime) &&
                string.Equals(this.Comment, other.Comment) && string.Equals(this.QuestionKey, other.QuestionKey);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is AnswerComment && Equals((AnswerComment) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = this.UserId.GetHashCode();
                hashCode = (hashCode*397) ^ this.CommentTime.GetHashCode();
                hashCode = (hashCode*397) ^ (this.Comment != null ? this.Comment.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (this.QuestionKey != null ? this.QuestionKey.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion
    }
}