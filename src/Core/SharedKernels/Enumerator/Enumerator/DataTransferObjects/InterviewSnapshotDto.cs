using System;

using WB.Core.SharedKernels.DataCollection.DataTransferObjects;

namespace WB.Core.SharedKernels.Enumerator.DataTransferObjects
{
    public class InterviewAnswerDto
    {
        public InterviewAnswerDto(Guid id, decimal[] rosterVector, AnswerType answerType, object answer)
        {
            Id = id;
            RosterVector = rosterVector;
            Answer = answer;
            Type = answerType;
        }

        public Guid Id { get; set; }
        public decimal[] RosterVector { get; set; }
        public AnswerType Type { get; set; }
        public object Answer { get; set; }
    }
}
