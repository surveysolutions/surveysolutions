using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class AudioQuestionAnswered : QuestionAnswered
    {
        public TimeSpan Length { get; set; }
        public string FileName { get; set; }

        public AudioQuestionAnswered(Guid userId, Guid questionId, decimal[] rosterVector,
            DateTimeOffset originDate, string fileName, TimeSpan length)
            : base(userId, questionId, rosterVector, originDate)
        {
            this.Length = length;
            this.FileName = fileName;
        }
    }
}
