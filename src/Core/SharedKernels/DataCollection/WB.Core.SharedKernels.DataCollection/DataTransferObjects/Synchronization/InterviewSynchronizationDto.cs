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

        public InterviewSynchronizationDto(Guid id, InterviewStatus status, Guid userId, Guid questionnaireId, long questionnaireVersion,
                                           IList<AnsweredQuestionSynchronizationDto> answers,
                                           HashSet<ItemPublicKey> disabledGroups,
                                           HashSet<ItemPublicKey> disabledQuestions,
                                           HashSet<ItemPublicKey> invalidAnsweredQuestions,
                                           Dictionary<ItemPublicKey, int> propagatedGroupInstanceCounts)
        {
            Id = id;
            Status = status;
            UserId = userId;
            QuestionnaireId = questionnaireId;
            QuestionnaireVersion = questionnaireVersion;
            Answers = answers;
            DisabledGroups = disabledGroups;
            DisabledQuestions = disabledQuestions;
            InvalidAnsweredQuestions = invalidAnsweredQuestions;
            PropagatedGroupInstanceCounts = propagatedGroupInstanceCounts;
        }

        public Guid Id { get;  set; }
        public InterviewStatus Status { get;  set; }
        public Guid UserId { get;  set; }
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }
        public IList<AnsweredQuestionSynchronizationDto> Answers { get;  set; }
        public HashSet<ItemPublicKey> DisabledGroups { get;  set; }
        public HashSet<ItemPublicKey> DisabledQuestions { get;  set; }
        public HashSet<ItemPublicKey> InvalidAnsweredQuestions { get;  set; }
        public Dictionary<ItemPublicKey, int> PropagatedGroupInstanceCounts { get;  set; }
    }
}
