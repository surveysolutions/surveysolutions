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

        public InterviewSynchronizationDto(Guid id, InterviewStatus status, Guid userId, Guid questionnaireId, IList<AnsweredQuestionSynchronizationDto> answers, HashSet<ItemPublicKey> disabledGroups, HashSet<ItemPublicKey> disabledQuestions, HashSet<ItemPublicKey> invalidAnsweredQuestions, Dictionary<ItemPublicKey, int> propagatedGroupInstanceCounts)
        {
            Id = id;
            Status = status;
            UserId = userId;
            QuestionnaireId = questionnaireId;
            Answers = answers;
            DisabledGroups = disabledGroups;
            DisabledQuestions = disabledQuestions;
            InvalidAnsweredQuestions = invalidAnsweredQuestions;
            PropagatedGroupInstanceCounts = propagatedGroupInstanceCounts;
        }

        public Guid Id { get; private set; }
        public InterviewStatus Status { get; private set; }
        public Guid UserId { get; private set; }
        public Guid QuestionnaireId { get; private set; }
        public IList<AnsweredQuestionSynchronizationDto> Answers { get; private set; }
        public HashSet<ItemPublicKey> DisabledGroups { get; private set; }
        public HashSet<ItemPublicKey> DisabledQuestions { get; private set; }
        public HashSet<ItemPublicKey> InvalidAnsweredQuestions { get; private set; }
        public Dictionary<ItemPublicKey, int> PropagatedGroupInstanceCounts { get; private set; }
    }
}
