using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization
{
    public class InterviewSynchronizationDto
    {
        public InterviewSynchronizationDto()
        {
            Answers = Array.Empty<AnsweredQuestionSynchronizationDto>();
            this.FailedValidationConditions = new List<KeyValuePair<Identity, IList<FailedValidationCondition>>>();
            this.ReadonlyQuestions = new HashSet<InterviewItemId>();
        }

        public InterviewSynchronizationDto(Guid id, 
            InterviewStatus status, 
            string comments, 
            DateTime? rejectDateTime,
            DateTime? interviewerAssignedDateTime,
            Guid userId, 
            Guid? supervisorId, 
            Guid questionnaireId, 
            long questionnaireVersion, 
            AnsweredQuestionSynchronizationDto[] answers,
            HashSet<InterviewItemId> disabledGroups, 
            HashSet<InterviewItemId> disabledQuestions, 
            IList<Identity> disabledStaticTexts, 
            HashSet<InterviewItemId> validAnsweredQuestions, 
            HashSet<InterviewItemId> invalidAnsweredQuestions, 
            HashSet<InterviewItemId> readonlyQuestions, 
            IList<Identity> validStaticTexts, 
            IList<KeyValuePair<Identity, List<FailedValidationCondition>>> invalidStaticTexts, 
            IList<KeyValuePair<Identity, IList<FailedValidationCondition>>> failedValidationConditions, 
            Dictionary<InterviewItemId, object> variables, 
            HashSet<InterviewItemId> disabledVariables, 
            bool wasCompleted, 
            bool createdOnClient = false)
        {
            Id = id;
            Status = status;
            Comments = comments;
            RejectDateTime = rejectDateTime;
            this.InterviewerAssignedDateTime = interviewerAssignedDateTime;
            UserId = userId;
            SupervisorId = supervisorId;
            QuestionnaireId = questionnaireId;
            QuestionnaireVersion = questionnaireVersion;
            Answers = answers;
            DisabledGroups = disabledGroups;
            DisabledQuestions = disabledQuestions;
            this.DisabledStaticTexts = disabledStaticTexts;
            ValidAnsweredQuestions = validAnsweredQuestions;
            InvalidAnsweredQuestions = invalidAnsweredQuestions;
            ReadonlyQuestions = readonlyQuestions;
            
            this.FailedValidationConditions = failedValidationConditions;
            this.WasCompleted = wasCompleted;
            this.CreatedOnClient = createdOnClient;

            this.ValidStaticTexts = validStaticTexts;
            this.InvalidStaticTexts = invalidStaticTexts;

            this.Variables = variables;
            this.DisabledVariables = disabledVariables;
        }

        public Guid Id { get;  set; }
        public bool CreatedOnClient { get; set; }
        public string Language { get; set; }
        public InterviewStatus Status { get;  set; }
        public string Comments { get; set; }
        public DateTime? RejectDateTime { get; set; }
        public DateTime? InterviewerAssignedDateTime { get; set; }
        public Guid UserId { get;  set; }
        public Guid? SupervisorId { get; set; }
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }
        public AnsweredQuestionSynchronizationDto[] Answers { get;  set; }
        public HashSet<InterviewItemId> DisabledGroups { get;  set; }
        public HashSet<InterviewItemId> DisabledQuestions { get;  set; }
        public HashSet<InterviewItemId> DisabledVariables { get; set; }
        public IList<Identity> DisabledStaticTexts { get; set; }
        public HashSet<InterviewItemId> ValidAnsweredQuestions { get;  set; }
        public HashSet<InterviewItemId> InvalidAnsweredQuestions { get;  set; }
        public HashSet<InterviewItemId> ReadonlyQuestions { get;  set; }

        public InterviewKey InterviewKey { get; set; }

        public int? AssignmentId { get; set; }

        public IList<KeyValuePair<Identity, IList<FailedValidationCondition>>> FailedValidationConditions { get; set; }
        public Dictionary<InterviewItemId, object> Variables { get; set; }

        public bool WasCompleted { get; set; }
        public IList<Identity> ValidStaticTexts { get; set; }
        public IList<KeyValuePair<Identity, List<FailedValidationCondition>>> InvalidStaticTexts { get; set; }
    }
}
