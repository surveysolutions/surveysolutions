using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Snapshots
{
    internal class InterviewState
    {
        public InterviewState(bool wasHardDeleted)
        {
            WasHardDeleted = wasHardDeleted;
        }

        public InterviewState(Guid questionnaireId, long questionnaireVersion, InterviewStatus status,
            Dictionary<string, object> answersSupportedInExpressions, Dictionary<string, Tuple<Guid, decimal[], decimal[]>> linkedSingleOptionAnswers,
            Dictionary<string, Tuple<Guid, decimal[], decimal[][]>> linkedMultipleOptionsAnswers, Dictionary<string, Tuple<decimal, string>[]> textListAnswers,
            HashSet<string> answeredQuestions,List<AnswerComment> answerComments,
            HashSet<string> disabledGroups, HashSet<string> disabledQuestions, Dictionary<string, DistinctDecimalList> rosterGroupInstanceIds,
            HashSet<string> validAnsweredQuestions, HashSet<string> invalidAnsweredQuestions, bool wasCompleted,
            IInterviewExpressionState expressionProcessorState, Guid interviewewerId)
        {
            this.QuestionnaireId = questionnaireId;
            this.QuestionnaireVersion = questionnaireVersion;
            this.Status = status;
            this.AnswersSupportedInExpressions = answersSupportedInExpressions;
            this.LinkedSingleOptionAnswers = linkedSingleOptionAnswers;
            this.LinkedMultipleOptionsAnswers = linkedMultipleOptionsAnswers;
            this.TextListAnswers = textListAnswers;
            this.AnsweredQuestions = answeredQuestions;
            this.AnswerComments = answerComments;
            this.DisabledGroups = disabledGroups;
            this.DisabledQuestions = disabledQuestions;
            this.RosterGroupInstanceIds = rosterGroupInstanceIds;
            this.ValidAnsweredQuestions = validAnsweredQuestions;
            this.InvalidAnsweredQuestions = invalidAnsweredQuestions;
            this.WasCompleted = wasCompleted;
            this.ExpressionProcessorState = expressionProcessorState;
            InterviewewerId = interviewewerId;
        }

        public Guid QuestionnaireId { get; private set; }
        public Guid InterviewewerId { get; private set; }
        public long QuestionnaireVersion { get; private set; }
        public InterviewStatus Status { get; private set; }
        public Dictionary<string, object> AnswersSupportedInExpressions { get; private set; }
        public Dictionary<string, Tuple<Guid, decimal[], decimal[]>> LinkedSingleOptionAnswers { get; private set; }
        public Dictionary<string, Tuple<Guid, decimal[], decimal[][]>> LinkedMultipleOptionsAnswers { get; private set; }
        public HashSet<string> AnsweredQuestions { get; private set; }
        public List<AnswerComment> AnswerComments { get; private set; }
        public HashSet<string> DisabledGroups { get; private set; }
        public HashSet<string> DisabledQuestions { get; private set; }
        public Dictionary<string, DistinctDecimalList> RosterGroupInstanceIds { get; private set; }
        public HashSet<string> ValidAnsweredQuestions { get; private set; }
        public HashSet<string> InvalidAnsweredQuestions { get; private set; }
        public bool WasCompleted { get; private set; }
        public bool WasHardDeleted { get; private set; }
        public Dictionary<string, Tuple<decimal, string>[]> TextListAnswers { get; set; }
        public IInterviewExpressionState ExpressionProcessorState { get; set; }
    }
}
