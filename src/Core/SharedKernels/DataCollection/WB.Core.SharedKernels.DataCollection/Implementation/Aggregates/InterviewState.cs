using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    internal class InterviewState
    {
        public InterviewState(Guid questionnaireId, long questionnaireVersion, InterviewStatus status,
            Dictionary<string, object> answersSupportedInExpressions, HashSet<string> answeredQuestions,
            HashSet<string> disabledGroups, HashSet<string> disabledQuestions, Dictionary<string, int> propagatedGroupInstanceCounts,
            HashSet<string> validAnsweredQuestions, HashSet<string> invalidAnsweredQuestions)
        {
            QuestionnaireId = questionnaireId;
            QuestionnaireVersion = questionnaireVersion;
            Status = status;
            AnswersSupportedInExpressions = answersSupportedInExpressions;
            AnsweredQuestions = answeredQuestions;
            DisabledGroups = disabledGroups;
            DisabledQuestions = disabledQuestions;
            PropagatedGroupInstanceCounts = propagatedGroupInstanceCounts;
            ValidAnsweredQuestions = validAnsweredQuestions;
            InvalidAnsweredQuestions = invalidAnsweredQuestions;
        }

        public Guid QuestionnaireId { get; private set; }
        public long QuestionnaireVersion { get; private set; }
        public InterviewStatus Status { get; private set; }
        public Dictionary<string, object> AnswersSupportedInExpressions { get; private set; }
        public HashSet<string> AnsweredQuestions { get; private set; }
        public HashSet<string> DisabledGroups { get; private set; }
        public HashSet<string> DisabledQuestions { get; private set; }
        public Dictionary<string, int> PropagatedGroupInstanceCounts { get; private set; }
        public HashSet<string> ValidAnsweredQuestions { get; private set; }
        public HashSet<string> InvalidAnsweredQuestions { get; private set; }
    }
}
