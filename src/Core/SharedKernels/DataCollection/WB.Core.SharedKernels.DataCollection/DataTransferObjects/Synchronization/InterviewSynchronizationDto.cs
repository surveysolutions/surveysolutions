using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization
{
    public class InterviewSynchronizationDto
    {
        public InterviewSynchronizationDto()
        {
        }

        public InterviewSynchronizationDto(Guid id, Guid statusId, Guid userId, Guid questionnaireId, Dictionary<Guid, AnswerSynchronizationDto> answers)
        {
            Id = id;
            StatusId = statusId;
            UserId = userId;
            QuestionnaireId = questionnaireId;
            Answers = answers;
        }

        public Guid Id { get; private set; }
        public Guid StatusId { get; private set; }
        public Guid UserId { get; private set; }
        public Guid QuestionnaireId { get; private set; }
        public Dictionary<Guid, AnswerSynchronizationDto> Answers { get; private set; }
    }
}
