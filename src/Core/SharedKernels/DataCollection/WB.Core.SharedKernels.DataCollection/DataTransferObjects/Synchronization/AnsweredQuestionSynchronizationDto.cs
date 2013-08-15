using System;

namespace WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization
{
    public class AnsweredQuestionSynchronizationDto
    {
        public AnsweredQuestionSynchronizationDto()
        {
        }

        public AnsweredQuestionSynchronizationDto(Guid id, object answer, string comments)
        {
            Id = id;
            Answer = answer;
            Comments = comments;
        }

        public Guid Id { get; private set; }
        public int[] PropagationVector { get; private set; }
        public object Answer { get; private set; }
        public string Comments { get; private set; }
    }
}
