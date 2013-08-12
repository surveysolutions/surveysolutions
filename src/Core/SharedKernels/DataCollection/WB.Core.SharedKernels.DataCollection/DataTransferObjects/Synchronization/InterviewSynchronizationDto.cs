using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization
{
    public class InterviewSynchronizationDto
    {
        public InterviewSynchronizationDto()
        {
        }

        public InterviewSynchronizationDto(Guid id, InterviewStatus status, Guid userId, Guid questionnaireId, Dictionary<Guid, AnswerSynchronizationDto> answers)
        {
            Id = id;
            Status = status;
            UserId = userId;
            QuestionnaireId = questionnaireId;
            Answers = answers;
        }

        public Guid Id { get; private set; }
        public InterviewStatus Status { get; private set; }
        public Guid UserId { get; private set; }
        public Guid QuestionnaireId { get; private set; }
        public Dictionary<Guid, AnswerSynchronizationDto> Answers { get; private set; }
    }
}
