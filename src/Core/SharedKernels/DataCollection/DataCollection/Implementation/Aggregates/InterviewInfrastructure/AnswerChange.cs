using System;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public class AnswerChange
    {
        public AnswerChange(AnswerChangeType atomicChangeType, Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, object answer)
        {
            this.InterviewChangeType = atomicChangeType;
            this.UserId = userId;
            this.QuestionId = questionId;
            this.RosterVector = rosterVector;
            this.AnswerTime = answerTime;
            this.Answer = answer;
        }

        public AnswerChangeType InterviewChangeType { set; get; }

        public Guid UserId { set; get; }
        public Guid QuestionId { set; get; }
        public decimal[] RosterVector { set; get; }
        public DateTime AnswerTime { set; get;  }

        public object Answer { set; get; }
    }

    public enum AnswerChangeType
    {
        Text,
        DateTime,
        TextList,
        GeoLocation,
        QRBarcode,
        NumericInteger,
        NumericReal,
        SingleOptionLinked,
        SingleOption,
        MultipleOptionsLinked,
        MultipleOptions,
        Picture
    }
}
