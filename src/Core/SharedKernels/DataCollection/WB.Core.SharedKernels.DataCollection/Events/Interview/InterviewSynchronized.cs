using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewSynchronized : InterviewActiveEvent
    {
        public InterviewSynchronized(Guid userId, Guid questionnaireId, InterviewStatus status,
                                     long questionnaireVersion, IList<AnsweredQuestionSynchronizationDto> answeredQuestions,
                                     HashSet<InterviewItemId> disabledGroups, HashSet<InterviewItemId> disabledQuestions,
                                     HashSet<InterviewItemId> validAnsweredQuestions, HashSet<InterviewItemId> invalidAnsweredQuestions,
                                     Dictionary<InterviewItemId, int> propagatedGroupInstanceCounts)
            : base(userId)
        {
            QuestionnaireId = questionnaireId;
            Status = status;
            QuestionnaireVersion = questionnaireVersion;
            AnsweredQuestions = answeredQuestions?? new List<AnsweredQuestionSynchronizationDto>();
            DisabledGroups = disabledGroups?? new HashSet<InterviewItemId>();
            DisabledQuestions = disabledQuestions?? new HashSet<InterviewItemId>();
            ValidAnsweredQuestions = validAnsweredQuestions?? new HashSet<InterviewItemId>();
            InvalidAnsweredQuestions = invalidAnsweredQuestions?? new HashSet<InterviewItemId>();
            PropagatedGroupInstanceCounts = propagatedGroupInstanceCounts?? new Dictionary<InterviewItemId, int>();
        }

        public Guid QuestionnaireId { get; private set; }
        public InterviewStatus Status { get; private set; }
        public long QuestionnaireVersion { get; private set; }
        public IList<AnsweredQuestionSynchronizationDto> AnsweredQuestions { get; private set; }
        public HashSet<InterviewItemId> DisabledGroups { get; private set; }
        public HashSet<InterviewItemId> DisabledQuestions { get; private set; }
        public HashSet<InterviewItemId> ValidAnsweredQuestions { get; private set; }
        public HashSet<InterviewItemId> InvalidAnsweredQuestions { get; private set; }
        public Dictionary<InterviewItemId, int> PropagatedGroupInstanceCounts { get; private set; }
    }
}
