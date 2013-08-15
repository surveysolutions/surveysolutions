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
                                     HashSet<ItemPublicKey> disabledGroups, HashSet<ItemPublicKey> disabledQuestions,
                                     HashSet<ItemPublicKey> invalidAnsweredQuestions,
                                     Dictionary<ItemPublicKey, int> propagatedGroupInstanceCounts)
            : base(userId)
        {
            QuestionnaireId = questionnaireId;
            Status = status;
            QuestionnaireVersion = questionnaireVersion;
            AnsweredQuestions = answeredQuestions?? new List<AnsweredQuestionSynchronizationDto>();
            DisabledGroups = disabledGroups?? new HashSet<ItemPublicKey>();
            DisabledQuestions = disabledQuestions?? new HashSet<ItemPublicKey>();
            InvalidAnsweredQuestions = invalidAnsweredQuestions?? new HashSet<ItemPublicKey>();
            PropagatedGroupInstanceCounts = propagatedGroupInstanceCounts?? new Dictionary<ItemPublicKey, int>();
        }

        public Guid QuestionnaireId { get; private set; }
        public InterviewStatus Status { get; private set; }
        public long QuestionnaireVersion { get; private set; }
        public IList<AnsweredQuestionSynchronizationDto> AnsweredQuestions { get; private set; }
        public HashSet<ItemPublicKey> DisabledGroups { get; private set; }
        public HashSet<ItemPublicKey> DisabledQuestions { get; private set; }
        public HashSet<ItemPublicKey> InvalidAnsweredQuestions { get; private set; }
        public Dictionary<ItemPublicKey, int> PropagatedGroupInstanceCounts { get; private set; }
    }
}
