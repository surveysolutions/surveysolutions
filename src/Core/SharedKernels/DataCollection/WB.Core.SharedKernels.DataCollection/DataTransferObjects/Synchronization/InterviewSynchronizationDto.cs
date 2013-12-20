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
            AnsweredQuestionSynchronizationDto[] answers,
            HashSet<InterviewItemId> disabledGroups,
            HashSet<InterviewItemId> disabledQuestions,
            HashSet<InterviewItemId> validAnsweredQuestions,
            HashSet<InterviewItemId> invalidAnsweredQuestions,
            Dictionary<InterviewItemId, int> propagatedGroupInstanceCounts,
            Dictionary<InterviewItemId, RosterSynchronizationDto[]> rosterGroupInstances,
            bool wasCompleted)
        {
            Id = id;
            Status = status;
            UserId = userId;
            QuestionnaireId = questionnaireId;
            QuestionnaireVersion = questionnaireVersion;
            Answers = answers;
            DisabledGroups = disabledGroups;
            DisabledQuestions = disabledQuestions;
            ValidAnsweredQuestions = validAnsweredQuestions;
            InvalidAnsweredQuestions = invalidAnsweredQuestions;
            PropagatedGroupInstanceCounts = propagatedGroupInstanceCounts;
            RosterGroupInstances = rosterGroupInstances;
            this.WasCompleted = wasCompleted;
        }

        public Guid Id { get;  set; }
        public InterviewStatus Status { get;  set; }
        public Guid UserId { get;  set; }
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }
        public AnsweredQuestionSynchronizationDto[] Answers { get;  set; }
        public HashSet<InterviewItemId> DisabledGroups { get;  set; }
        public HashSet<InterviewItemId> DisabledQuestions { get;  set; }
        public HashSet<InterviewItemId> ValidAnsweredQuestions { get;  set; }
        public HashSet<InterviewItemId> InvalidAnsweredQuestions { get;  set; }
        [Obsolete("please use RosterGroupInstances")]
        public Dictionary<InterviewItemId, int> PropagatedGroupInstanceCounts { get; set; }
        public Dictionary<InterviewItemId, RosterSynchronizationDto[]> RosterGroupInstances
        {
            get
            {
                if (rosterGroupInstances == null && PropagatedGroupInstanceCounts != null)
                {
                    RosterGroupInstances = new Dictionary<InterviewItemId, RosterSynchronizationDto[]>();
                    foreach (var propagatedGroupInstanceCount in PropagatedGroupInstanceCounts)
                    {
                        RosterGroupInstances[propagatedGroupInstanceCount.Key] = new RosterSynchronizationDto[propagatedGroupInstanceCount.Value];
                        for (int i = 0; i < propagatedGroupInstanceCount.Value; i++)
                        {
                            RosterGroupInstances[propagatedGroupInstanceCount.Key][i] =
                                new RosterSynchronizationDto(propagatedGroupInstanceCount.Key.Id,
                                    propagatedGroupInstanceCount.Key.InterviewItemPropagationVector, Convert.ToDecimal(i), null,
                                    string.Empty);
                        }
                    }
                }
                return rosterGroupInstances;
            }
            set { rosterGroupInstances = value; }
        }
        private Dictionary<InterviewItemId, RosterSynchronizationDto[]> rosterGroupInstances;
        public bool WasCompleted { get; set; }
    }
}
