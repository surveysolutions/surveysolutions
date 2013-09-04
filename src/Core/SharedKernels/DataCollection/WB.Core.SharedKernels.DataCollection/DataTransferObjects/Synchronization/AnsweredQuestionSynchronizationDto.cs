using System;

namespace WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization
{
    public class AnsweredQuestionSynchronizationDto
    {
        public AnsweredQuestionSynchronizationDto()
        {
        }

        public AnsweredQuestionSynchronizationDto(Guid id, int[] vector, object answer, string comments)
        {
            Id = id;
            PropagationVector = vector;
            Answer = answer;
            Comments = comments;
        }

        public Guid Id { get;  set; }
        public int[] PropagationVector { get;  set; }
        public object Answer { get;  set; }
        public string Comments { get;  set; }
    }
}
