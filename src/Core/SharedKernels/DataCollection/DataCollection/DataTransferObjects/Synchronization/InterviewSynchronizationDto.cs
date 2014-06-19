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
        }

        public InterviewSynchronizationDto(Guid id, InterviewStatus status, Guid userId, Guid questionnaireId, long questionnaireVersion,
            AnsweredQuestionSynchronizationDto[] answers,
            HashSet<InterviewItemId> disabledGroups,
            HashSet<InterviewItemId> disabledQuestions,
            HashSet<InterviewItemId> validAnsweredQuestions,
            HashSet<InterviewItemId> invalidAnsweredQuestions,
            Dictionary<InterviewItemId, int> propagatedGroupInstanceCounts,
            Dictionary<InterviewItemId, RosterSynchronizationDto[]> rosterGroupInstances,
            bool wasCompleted,
            bool createdOnClient = false)
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
            this.CreatedOnClient = createdOnClient;

        }

        public Guid Id { get;  set; }
        public bool CreatedOnClient { get; set; }
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
                if (rosterGroupInstances == null)
                {
                    rosterGroupInstances = this.RestoreFromPropagatedGroupInstanceCounts();
                }
                return rosterGroupInstances;
            }
            set { rosterGroupInstances = value; }
        }
        private Dictionary<InterviewItemId, RosterSynchronizationDto[]> rosterGroupInstances;
        public bool WasCompleted { get; set; }

        private Dictionary<InterviewItemId, RosterSynchronizationDto[]> RestoreFromPropagatedGroupInstanceCounts()
        {
            if (PropagatedGroupInstanceCounts == null)
                return new Dictionary<InterviewItemId, RosterSynchronizationDto[]>();

            var result = new Dictionary<InterviewItemId, RosterSynchronizationDto[]>();
            foreach (var propagatedGroupInstanceCount in PropagatedGroupInstanceCounts)
            {
                result[propagatedGroupInstanceCount.Key] = new RosterSynchronizationDto[propagatedGroupInstanceCount.Value];
                for (int i = 0; i < propagatedGroupInstanceCount.Value; i++)
                {
                    result[propagatedGroupInstanceCount.Key][i] =
                        new RosterSynchronizationDto(propagatedGroupInstanceCount.Key.Id,
                            propagatedGroupInstanceCount.Key.InterviewItemPropagationVector, Convert.ToDecimal(i), null,
                            string.Empty);
                }
            }
            return result;
        }
    }
}
