using System;

namespace WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization
{
    public class AnswerSynchronizationDto
    {
        public AnswerSynchronizationDto()
        {
        }

        public AnswerSynchronizationDto(Guid id, object answer, string comments)
        {
            Id = id;
            Answer = answer;
            Comments = comments;
        }

        public Guid Id { get; private set; }
        public object Answer { get; private set; }
        public string Comments { get; private set; }
    }
}
