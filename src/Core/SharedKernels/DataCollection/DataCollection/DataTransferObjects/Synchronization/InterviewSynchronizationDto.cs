using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization
{
    public class InterviewSynchronizationDto
    {
        public InterviewSynchronizationDto()
        {
            Answers = new AnsweredQuestionSynchronizationDto[0];
            this.FailedValidationConditions = new List<KeyValuePair<Identity, IList<FailedValidationCondition>>>();
        }

        public InterviewSynchronizationDto(Guid id, 
            InterviewStatus status, 
            string comments,
            DateTime? rejectDateTime,
            DateTime? interviewerAssignedDateTime,
            Guid userId, 
            Guid questionnaireId, 
            long questionnaireVersion, 
            AnsweredQuestionSynchronizationDto[] answers,
            HashSet<InterviewItemId> disabledGroups,
            HashSet<InterviewItemId> disabledQuestions,
            HashSet<InterviewItemId> validAnsweredQuestions,
            HashSet<InterviewItemId> invalidAnsweredQuestions,
            Dictionary<InterviewItemId, RosterSynchronizationDto[]> rosterGroupInstances,
            IList<KeyValuePair<Identity, IList<FailedValidationCondition>>> failedValidationConditions,
            bool wasCompleted,
            bool createdOnClient = false)
        {
            Id = id;
            Status = status;
            Comments = comments;
            RejectDateTime = rejectDateTime;
            this.InterviewerAssignedDateTime = interviewerAssignedDateTime;
            UserId = userId;
            QuestionnaireId = questionnaireId;
            QuestionnaireVersion = questionnaireVersion;
            Answers = answers;
            DisabledGroups = disabledGroups;
            DisabledQuestions = disabledQuestions;
            ValidAnsweredQuestions = validAnsweredQuestions;
            InvalidAnsweredQuestions = invalidAnsweredQuestions;
            
            RosterGroupInstances = rosterGroupInstances;
            this.FailedValidationConditions = failedValidationConditions;
            this.WasCompleted = wasCompleted;
            this.CreatedOnClient = createdOnClient;

        }

        public Guid Id { get;  set; }
        public bool CreatedOnClient { get; set; }
        public InterviewStatus Status { get;  set; }
        public string Comments { get; set; }
        public DateTime? RejectDateTime { get; set; }
        public DateTime? InterviewerAssignedDateTime { get; set; }
        public Guid UserId { get;  set; }
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }
        public AnsweredQuestionSynchronizationDto[] Answers { get;  set; }
        public HashSet<InterviewItemId> DisabledGroups { get;  set; }
        public HashSet<InterviewItemId> DisabledQuestions { get;  set; }
        public HashSet<InterviewItemId> ValidAnsweredQuestions { get;  set; }
        public HashSet<InterviewItemId> InvalidAnsweredQuestions { get;  set; }

        public IList<KeyValuePair<Identity, IList<FailedValidationCondition>>> FailedValidationConditions { get; set; } 
        
        public Dictionary<InterviewItemId, RosterSynchronizationDto[]> RosterGroupInstances { get; set; }

        public bool WasCompleted { get; set; }
    }
}
