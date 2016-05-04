using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.V2;
using WB.Core.SharedKernels.DataCollection.V4;
using WB.Core.SharedKernels.DataCollection.V5;
using WB.Core.SharedKernels.DataCollection.V6;
using WB.Core.SharedKernels.DataCollection.V7;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public class InterviewExpressionStateForPreloading : IInterviewExpressionStateV7
    {
        public InterviewExpressionStateForPreloading()
        {
        }

        public void UpdateNumericIntegerAnswer(Guid questionId, decimal[] rosterVector, long? answer)
        {
        }

        public void UpdateNumericRealAnswer(Guid questionId, decimal[] rosterVector, double? answer)
        {
        }

        public void UpdateDateAnswer(Guid questionId, decimal[] rosterVector, DateTime? answer)
        {
        }

        public void UpdateMediaAnswer(Guid questionId, decimal[] rosterVector, string answer)
        {
        }

        public void UpdateTextAnswer(Guid questionId, decimal[] rosterVector, string answer)
        {
        }

        public void UpdateQrBarcodeAnswer(Guid questionId, decimal[] rosterVector, string answer)
        {
        }

        public void UpdateSingleOptionAnswer(Guid questionId, decimal[] rosterVector, decimal? answer)
        {
        }

        public void UpdateMultiOptionAnswer(Guid questionId, decimal[] rosterVector, decimal[] answer)
        {
        }

        public void UpdateGeoLocationAnswer(Guid questionId, decimal[] propagationVector, double latitude,
            double longitude,
            double accuracy, double altitude)
        {
        }

        public void UpdateTextListAnswer(Guid questionId, decimal[] propagationVector, Tuple<decimal, string>[] answers)
        {
        }

        public void UpdateLinkedSingleOptionAnswer(Guid questionId, decimal[] propagationVector,
            decimal[] selectedPropagationVector)
        {
        }

        public void UpdateLinkedMultiOptionAnswer(Guid questionId, decimal[] propagationVector,
            decimal[][] selectedPropagationVectors)
        {
        }

        public void DeclareAnswersInvalid(IEnumerable<Identity> invalidQuestions)
        {
        }

        public void DeclareAnswersValid(IEnumerable<Identity> validQuestions)
        {
        }

        public void DisableGroups(IEnumerable<Identity> groupsToDisable)
        {
        }

        public void EnableGroups(IEnumerable<Identity> groupsToEnable)
        {
        }

        public void DisableQuestions(IEnumerable<Identity> questionsToDisable)
        {
        }

        public void EnableQuestions(IEnumerable<Identity> questionsToEnable)
        {
        }

        public void AddRoster(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId, int? sortIndex)
        {
        }

        public void RemoveRoster(Guid rosterId, decimal[] rosterVector, decimal rosterInstanceId)
        {
        }

        public ValidityChanges ProcessValidationExpressions()
        {
            return new ValidityChanges(new List<Identity>(), new List<Identity>());
        }

        public EnablementChanges ProcessEnablementConditions()
        {
            return new EnablementChanges(new List<Identity>(), new List<Identity>(), new List<Identity>(),
                new List<Identity>());
        }

        public void SaveAllCurrentStatesAsPrevious()
        {

        }

        public bool AreLinkedQuestionsSupported()
        {
            return true;
        }

        IInterviewExpressionStateV7 IInterviewExpressionStateV7.Clone()
        {
            return new InterviewExpressionStateForPreloading();
        }


        public void SetInterviewProperties(IInterviewProperties properties)
        {
        }

        public void UpdateRosterTitle(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId,
            string rosterTitle)
        {
        }

        public void UpdateYesNoAnswer(Guid questionId, decimal[] propagationVector, YesNoAnswersOnly answers)
        {
        }

        public LinkedQuestionOptionsChanges ProcessLinkedQuestionFilters()
        {
            return new LinkedQuestionOptionsChanges(new Dictionary<Guid, RosterVector[]>());
        }

        IInterviewExpressionStateV6 IInterviewExpressionStateV6.Clone()
        {
            return new InterviewExpressionStateForPreloading();
        }

        IInterviewExpressionStateV5 IInterviewExpressionStateV5.Clone()
        {
            return new InterviewExpressionStateForPreloading();
        }

        IInterviewExpressionStateV4 IInterviewExpressionStateV4.Clone()
        {
            return new InterviewExpressionStateForPreloading();
        }

        IInterviewExpressionStateV2 IInterviewExpressionStateV2.Clone()
        {
            return new InterviewExpressionStateForPreloading();
        }

        public IInterviewExpressionState Clone()
        {
            return new InterviewExpressionStateForPreloading();
        }

        public void ApplyFailedValidations(IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> failedValidationConditions)
        {
        }
    }
}